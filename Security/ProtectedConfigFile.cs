/****************************************************************************/
/*                                                                          */
/*           Module: Mondo.Security         				                */
/*             File: ProtectedConfigFile.cs								    */
/*        Class(es): ProtectedConfigFile								  	*/
/*          Purpose: A config file encrypted and stored in Isolated Storage */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 4 May 2014                                             */
/*                                                                          */
/*   Copyright (c) 2014 - Zoomla Corp,  All rights reserved                 */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Mondo.Common;
using Mondo.Xml;

namespace Mondo.Security
{
    /****************************************************************************/
    /****************************************************************************/
    public class ProtectedConfigFile : IDisposable /* IConfig */
    {
        private readonly StringDictionary m_aValues = new StringDictionary();
        private IStorage m_objStorage = null;

        /****************************************************************************/
        public ProtectedConfigFile(string strName) : this("", strName, null, null, false)
        {
        }

        /****************************************************************************/
        public ProtectedConfigFile(string strName, bool bClear) : this("", strName, null, null, bClear)
        {
        }

        /****************************************************************************/
        public ProtectedConfigFile(string strPath, string strName, IStorage objStorage, IEncryptor objEncryptor, bool bClear)
        {
            m_objStorage   = objStorage;
            this.Encryptor = objEncryptor;
            this.Name      = strName;
            this.Path      = strPath;
            this.Modified  = false;

            if(!bClear)
            {
                try 
                { 
                    IStorage    stor          = this.Storage;
                    string      strData       = stor.LoadFile(this.Name + ".config");
                    XmlDocument xmlConfigFile = XmlDoc.LoadXml(strData);
                    XmlNodeList aItems        = xmlConfigFile.SelectNodes("//Config/*");

                    foreach(XmlNode xmlItem in aItems)
                    {
                        string strValue = xmlItem.InnerText.Trim();

                        if(this.Encryptor != null)
                            strValue = this.Encryptor.Decrypt(strValue);

                        m_aValues.Add(xmlItem.LocalName.Replace("KeyName", ""), strValue);
                    }
                }
                catch
                {
                }
            }
        }

        /****************************************************************************/
        public IEncryptor Encryptor
        {
            get;
            private set;
        }

        /****************************************************************************/
        public string Name
        {
            get;
            private set;
        }

        /****************************************************************************/
        public string Path
        {
            get;
            private set;
        }

        /****************************************************************************/
        private bool Modified
        {
            get;
            set;
        }

        /****************************************************************************/
        public byte[] Get(string strName)
        {
            return(this[strName].FromBase64String());
        }

        /****************************************************************************/
        public void Set(string strName, byte[] aData)
        {
            this[strName] = aData.ToBase64String();
        }

        /****************************************************************************/
        public string this [string strName]
        {
            get
            {
                if(m_aValues.ContainsKey(strName))
                    return(m_aValues[strName]);

                throw new ArgumentException("Config item not found: " + strName);
            }

            set
            {
                string strValue = value.Trim();

                if(m_aValues.ContainsKey(strName))
                {
                    if(m_aValues[strName] != strValue)
                    {
                        m_aValues[strName] = strValue;
                        this.Modified = true;
                    }
                }
                else
                {
                    m_aValues.Add(strName, strValue);
                    this.Modified = true;
                }
            }
        }

        /****************************************************************************/
        protected virtual IStorage Storage
        {
            get
            {
                if(m_objStorage != null)
                    return(m_objStorage);

                string strOrgName = Config.Get("OrganizationName");
                string strAppName = Config.Get("ApplicationName");
                string strPath    = strAppName;
                
                if(this.Path != "")
                    strPath = strPath.EnsureEndsWithSeparator() + this.Path;

                return(new IsolatedStorage(strPath));
            }
        }

        /****************************************************************************/
        public void Dispose()
        {
            if(this.Modified)
            {
                cXMLWriter w = new cXMLWriter();

                using(w.Acquire)
                {
                    using(w.Element("Config"))
                    {
                        foreach(string strKey in m_aValues.Keys)
                        { 
                            string strValue = m_aValues[strKey];

                            if(this.Encryptor != null)
                                strValue = this.Encryptor.Encrypt(strValue);

                            w.WriteElementString("KeyName" + strKey, strValue);
                        }
                    }
                }

                string   xmlConfigFile = w.ToString();
                IStorage stor          = this.Storage;

                stor.DropData(this.Name + ".config", xmlConfigFile, true);
            }
        }
    }
}
