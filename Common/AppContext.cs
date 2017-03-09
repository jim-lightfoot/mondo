/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: AppContext.cs											*/
/*        Class(es): AppContext, IAppContext								*/
/*          Purpose: Provide an application wide context to all operations	*/
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*                                                                          */
/*   Copyright (c) 2015-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Mondo.Xml;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public interface IAppContext
    {
        IConfig         Config          { get; }
        ICache          Cache           { get; }
        ILog            Log             { get; }
        IEncryptor      Encryptor       { get; }
        ISettingsStore  Cookies         { get; }
        ISettingsStore  Session         { get; }
        ISettingsStore  Request         { get; }
                                   
        bool            IsDebug         { get; }
        string          Environment     { get; }
        long            LocationID      { get; }
        string          CurrentLanguage { get; }

        void            Validate(ILog log = null);
        string          MapPath(string path);
    }

    /****************************************************************************/
    /****************************************************************************/
    public abstract class ApplicationContext : IAppContext, IConfig
    {
        private readonly Dictionary<string, string> _configKeys = new Dictionary<string,string>(107);
        private IConfig                             _config     = null;

        /****************************************************************************/
        public ApplicationContext() : this(true, true)
        {
        }

        /****************************************************************************/
        protected ApplicationContext(bool config, bool log)
        {
            if(config)
                this.Config = new AppConfig();

            if(log)
                this.Log = new ApplicationLog();

           #if DEBUG
            this.IsDebug = true;
           #else
            this.IsDebug = false;
           #endif
        }

        #region IAppContext

        /****************************************************************************/
        public IConfig              Config          { get; protected set; }
        public ICache               Cache           { get; protected set; }
        public ILog                 Log             { get; protected set; }
        public abstract IEncryptor  Encryptor       { get; }
        public ISettingsStore       Cookies         { get; protected set; }
        public ISettingsStore       Session         { get; protected set; }
        public ISettingsStore       Request         { get; protected set; }
                 
        public bool                 IsDebug         { get; protected set; }
        public string               Environment     { get; protected set; }
        public virtual string       CurrentLanguage { get {return("eng"); }  }
        public abstract long        LocationID      { get; }

        public abstract void        Validate(ILog log = null);
        public abstract string      MapPath(string path);

        #endregion

        #region Exceptions

        /****************************************************************************/
		public class UnregisteredConfigEntry : Exception
        {
            public UnregisteredConfigEntry(string name) : base(string.Format("Use of unregistered config entry named '{0}'", name))
            {
            }
        }

        /****************************************************************************/
		public class ConfigEntryWrongType : Exception
        {
            public ConfigEntryWrongType(string name, string type) : base(string.Format("Config file entry for '{0}' is not expected type of '{1}'", name, type))
            {
            }
        }

        /****************************************************************************/
		public class InvalidManifestEntry : Exception
        {
            public InvalidManifestEntry(string name, string msg) : base(string.Format("Invalid manifest entry for '{0}': {1}", name, msg))
            {
            }
        }

        /****************************************************************************/
		public class MissingConnectionString : Exception
        {
            public MissingConnectionString(string name) : base(string.Format("Expecting connection string named '{0}'", name))
            {
            }
        }

        /****************************************************************************/
		public class MissingConnectionstringRequiredPart : Exception
        {
            public MissingConnectionstringRequiredPart(string name, string part) : base(string.Format("Expecting a part named '{0}' in the connection string named '{1}'", part, name))
            {
            }
        }

        #endregion

        #region Validation Methods

        /****************************************************************************/
        protected void ValidateManifest(XmlDocument xmlManifest, ILog log = null)
        {
            ValidateConfig(xmlManifest, log);
            ValidateConnectionStrings(xmlManifest, log);

            // ??? Validate other items as well
        }

        /****************************************************************************/
        private void ValidateConfig(XmlDocument xmlManifest, ILog log)
        {
            XmlNodeList xmlConfigs  = xmlManifest.SelectNodes("//Config/Item");
            _configKeys.Clear();

            foreach(XmlNode node in xmlConfigs)
            {
                string key = "";
                string value = "";

                if(log != null)
                { 
                    try 
                    { 
                        key = ValidateConfigEntry(node, out value);
                    }
                    catch(Exception ex)
                    {
                        log.WriteError(ex);
                    }
                }
                else
                    key = ValidateConfigEntry(node, out value);

                _configKeys.Add(key, value);
            }

            _config = this.Config;
            this.Config = this;
        }

        /****************************************************************************/
        private string ValidateConfigEntry(XmlNode node, out string value)
        {        
            string key      = node.GetAttribute("key");
            bool   required = node.GetAttribute("required") == "true";
            string type     = node.GetAttribute("type", "string");
            
            value = this.Config.Get(key, required);

            if(type != "string")
            {
                switch(type)
                {
                    case "int":
                    {
                        long val = 0;

                        if(!long.TryParse(value, out val))
                            throw new ConfigEntryWrongType(key, type);

                        break;
                    }

                    case "guid":
                    { 
                        Guid val;

                        if(!Guid.TryParse(value, out val))
                            throw new ConfigEntryWrongType(key, type);

                        break;
                    }

                    case "decimal":
                    { 
                        decimal val;

                        if(!decimal.TryParse(value, out val))
                            throw new ConfigEntryWrongType(key, type);

                        break;
                    }

                    case "path":
                    { 
                        if(value.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                            throw new ConfigEntryWrongType(key, type);

                        if(Path.GetPathRoot(value).Normalized() == "")
                            throw new ConfigEntryWrongType(key, type);

                        break;
                    }

                    case "url":
                    { 
                        value = value.ToLower();

                        if(!value.StartsWith("http://") && !value.StartsWith("https://") )
                            throw new ConfigEntryWrongType(key, type);

                        break;
                    }

                    default:
                        break;
                }
            }

            return(key);
        }

        /****************************************************************************/
        private void ValidateConnectionStrings(XmlDocument xmlManifest, ILog log)
        {        
            xmlManifest.ForEachNode("//ConnectionStrings/Item", (node) =>
            {
                if(log != null)
                { 
                    try 
                    { 
                        ValidateConnectionString(node);
                    }
                    catch(Exception ex)
                    {
                        log.WriteError(ex);
                    }
                }
                else
                     ValidateConnectionString(node);
            });
        }

        /****************************************************************************/
        private void ValidateConnectionString(XmlNode node)
        {        
            string name = node.GetAttribute("name");

            if(name == "")
                throw new InvalidManifestEntry("ConnectionStrings", "Missing name attribute.");

            string strConnectionString = this.Config.GetConnectionString(name);

            if(strConnectionString == "")
                throw new MissingConnectionString(name);

            StringDictionary aParts = new StringDictionary(strConnectionString, ";", "=");

            node.ForEachNode("Require", (required) =>
            {
                string reqName = required.GetAttribute("name");

                // ??? Would be better to validate against a schema
                if(reqName == "")
                    throw new InvalidManifestEntry("ConnectionStrings", "Missing name attribute for Required element.");

                if(!aParts.ContainsKey(reqName))
                    throw new MissingConnectionstringRequiredPart(name, reqName);
            });
        }

        #endregion

        #region IConfig

        /****************************************************************************/
        public string Get(string strAttrName, bool bRequired = false)
        {
            if(!_configKeys.ContainsKey(strAttrName))
                throw new UnregisteredConfigEntry(strAttrName);

            return(_config.Get(strAttrName, bRequired));
        }

        /****************************************************************************/
        public string Get(string strAttrName, string strDefault, bool bRequired = false)
        {
            if(!_configKeys.ContainsKey(strAttrName))
                throw new UnregisteredConfigEntry(strAttrName);

            return(_config.Get(strAttrName, strDefault, bRequired));
        }

        /****************************************************************************/
        public void Set(string strAttrName, string strValue)
        {
            _config.Set(strAttrName, strValue);
        }

        /****************************************************************************/
        public string GetConnectionString(string strName)
        {
            // ??? Need to add check for this

            return(_config.GetConnectionString(strName));
        }

        #endregion
    }
}
