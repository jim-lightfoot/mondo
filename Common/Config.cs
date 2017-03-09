/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: Config.cs												*/
/*        Class(es): Config													*/
/*          Purpose: Helper class for accessing data in the .config file    */
/*						file												*/
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 29 Jun 2006                                            */
/*                                                                          */
/*   Copyright (c) 2006-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;

namespace Mondo.Common
{
    /****************************************************************************/
	/****************************************************************************/
	public interface IConfig
    {
 		string  Get(string strAttrName, bool bRequired = false);
		string  Get(string strAttrName, string strDefault, bool bRequired = false);
        
        void    Set(string strAttrName, string strValue);

        string  GetConnectionString(string strName);
    }

	/****************************************************************************/
	/****************************************************************************/
	public static class ConfigExtensions
	{    
        /****************************************************************************/
	    public static T Get<T>(this IConfig config, string strAttrName, T defaultVal = default(T), bool required = false) where T : struct
		{
    		string val = config.Get(strAttrName, defaultVal.ToString(), required);

            return(Utility.Convert<T>(val, defaultVal));
		}

        /****************************************************************************/
	    public static T GetSetting<T>(this ISettingsStore store, string name, T defaultVal = default(T)) where T : struct
		{
    		object val = store.GetSetting(name);

            if(val == null)
              return defaultVal;

            return(Utility.Convert<T>(val, defaultVal));
		}
    }

	/****************************************************************************/
	/****************************************************************************/
	/// <summary>
	/// Get values from web.config or app.config
	/// </summary>
	public static class Config
	{
		/****************************************************************************/
		public static string Get(string strAttrName, bool bRequired = false)
		{
			return(Get(strAttrName, "", bRequired));
		}

        /****************************************************************************/
		public class MissingEntry : Exception
        {
            public MissingEntry(string name) : base(string.Format("Config file missing entry for '{0}'", name))
            {
            }
        }

        /****************************************************************************/
	    public static string GetConnectionString(string strName)
        {
            System.Configuration.ConnectionStringSettings objConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[strName];

            if(objConnectionString == null)
                throw new Exception("Named connection string not found: " + strName);

            return(objConnectionString.ConnectionString.Normalized());
        }

		/****************************************************************************/
		public static string Get(string strAttrName, string strDefault, bool bRequired = false)
		{
            string strReturn = strDefault;

			try
			{
                strReturn = System.Configuration.ConfigurationManager.AppSettings[strAttrName].Trim();

                if(strReturn != "")
                    return(strReturn);
			}
			catch
			{
			}

            if(bRequired)
                throw new MissingEntry(strAttrName);

            return(strReturn);
		}

		/****************************************************************************/
		public static void Set(string strAttrName, string strValue)
		{
            System.Configuration.ConfigurationManager.AppSettings[strAttrName] = strValue;
		}

        /****************************************************************************/
	    public static T Get<T>(string strAttrName, T defaultVal = default(T), bool required = false) where T : struct
		{
    		string val = Get(strAttrName, defaultVal.ToString(), required);

            return(Utility.Convert<T>(val, defaultVal));
		}
	}

    /****************************************************************************/
    /*  IConfig wrapper around Config class                                     */
	/****************************************************************************/
	public class AppConfig : IConfig
    {
        /****************************************************************************/
 		public string Get(string strAttrName, bool bRequired = false)
        {
            return(Config.Get(strAttrName, bRequired));
        }
         
        /****************************************************************************/
	    public string Get(string strAttrName, string strDefault, bool bRequired = false)
        {
            return(Config.Get(strAttrName, strDefault, bRequired));
        }
		 
        /****************************************************************************/
	    public void Set(string strAttrName, string strValue)
        {
            throw new NotImplementedException();
        }

        /****************************************************************************/
	    public string GetConnectionString(string strName)
        {
            return(Config.GetConnectionString(strName));
        }
    }
}
