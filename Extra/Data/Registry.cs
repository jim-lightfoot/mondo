/****************************************************************************/
/*                                                                          */
/*           Module: Mondo.Registry                                         */
/*             File: Mondo.Registry.cs                                      */
/*        Class(es): Registry                                               */
/*          Purpose: Read and write registry values                         */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 25 Aug 2010                                            */
/*                                                                          */
/*   Copyright (c) 2010 - TenthGeneration Software                          */
/*                          All rights reserved                             */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Runtime.InteropServices;
using Microsoft.Win32;

using Mondo.Common;

namespace Mondo.Registry
{
    /****************************************************************************/
    /****************************************************************************/
    public class Registry : IDisposable /* IConfig */
    {
        private readonly RegistryKey m_keyApp;

        /****************************************************************************/
        public Registry() : this("")
        {
        }

        /****************************************************************************/
        public Registry(string strAppName)
        {
            m_keyApp = LoadApplicationKey(strAppName);
        }

        /****************************************************************************/
        public string this[string strName]
        {
            get
            {
                try
                {
                    return(m_keyApp.GetValue(strName).Normalized());
                }
                catch(System.Security.SecurityException)
                {
                }

                return("");
            }

            set
            {
                m_keyApp.SetValue(strName, value);
            }
        }

        /****************************************************************************/
        public byte[] Get(string strName)
        {
            return( Registry.GetBytes(strName));
        }

        /****************************************************************************/
        public void  Set(string strName, byte[] aData)
        {
            Registry.SetValue(strName, aData);
        }

        /****************************************************************************/
        public string GetString(string strName)
        {
            return( Registry.GetValue(strName));
        }

        /****************************************************************************/
        public void Set(string strName, string strData)
        {
            Registry.SetValue(strName, strData);
        }

        #region Static Functions

        /****************************************************************************/
        public static RegistryKey ApplicationKey
        {
            get
            {
                return(LoadApplicationKey(""));
            }
        }

        /****************************************************************************/
        public static bool DisableCurrentUserCache()
        {
            return(RegDisablePredefinedCacheEx() == 0);
        }

        /****************************************************************************/
        public static RegistryKey ReadApplicationKey
        {
            get
            {
              #if DEBUG
                string strUserName = WindowsIdentity.GetCurrent().Name;
              #endif

                string strOrganizationName = Config.Get("OrganizationName", "Mondo");
                string strApplicationName  = Config.Get("ApplicationName", "Mondo");

                return(Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\" + strOrganizationName + "\\" + strApplicationName));
            }
        }

        /****************************************************************************/
        public static void SetValue(string strName, string strValue)
        {
            using(RegistryKey appKey = Mondo.Registry.Registry.ApplicationKey)
            {
                appKey.SetValue(strName, strValue);
            }
        }

        /****************************************************************************/
        public static void SetValue(string strName, byte[] aData)
        {
            using(RegistryKey appKey = Mondo.Registry.Registry.ApplicationKey)
            {
                appKey.SetValue(strName, aData, RegistryValueKind.Binary);
            }
        }

        /****************************************************************************/
        public static string GetValue(string strName, string strDefault)
        {
            try
            {
                using(RegistryKey appKey = Mondo.Registry.Registry.ReadApplicationKey)
                {
                    return(appKey.GetValue(strName).Normalized());
                }
            }
            catch(System.Security.SecurityException)
            {
            }

            return(strDefault);
        }

        /****************************************************************************/
        public static byte[] GetBytes(string strName)
        {
            using(RegistryKey appKey = Mondo.Registry.Registry.ReadApplicationKey)
            {
                object objValue = appKey.GetValue(strName);

                return(objValue as byte[]);
            }
        }

        /****************************************************************************/
        public static string GetValue(string strName)
        {
            return(GetValue(strName, ""));
        }

        /****************************************************************************/
        public static string GetName(RegistryKey objKey)
        {
            return(objKey.Name.StripUpToLast("\\"));
        }

        /****************************************************************************/
        public static RegistryKey OpenKey(string strName, bool bWritable)
        {
            using(RegistryKey appKey = Mondo.Registry.Registry.ApplicationKey)
            {
                return(appKey.OpenSubKey(strName, bWritable));
            }
        }

        /****************************************************************************/
        public static RegistryKey CreateKey(string strName)
        {
            using(RegistryKey appKey = Mondo.Registry.Registry.ApplicationKey)
            {
                return(appKey.CreateSubKey(strName));
            }
        }

        /****************************************************************************/
        public static IList<RegistryKey> GetSubKeys(string strKey)
        {
            List<RegistryKey> aSubKeys = new List<RegistryKey>();

            try
            {
                using(RegistryKey appKey = Mondo.Registry.Registry.ApplicationKey)
                {
                    using(RegistryKey objKey = appKey.OpenSubKey(strKey, true))
                    {
                        string[] aNames = objKey.GetSubKeyNames();

                        foreach(string strName in aNames)
                        {
                            try
                            {
                                aSubKeys.Add(objKey.OpenSubKey(strName, true));
                            }
                            catch(Exception ex)
                            {
                                cDebug.Capture(ex);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                cDebug.Capture(ex);
            }

            return(aSubKeys);
        }

        #endregion

        /****************************************************************************/
        public void Dispose()
        {
 	        if(m_keyApp != null)
                m_keyApp.Dispose();
        }

        #region Private Functions

        /****************************************************************************/
        private static RegistryKey LoadApplicationKey(string strApplicationName)
        {
            string strOrganizationName = Config.Get("OrganizationName", "Mondo");

            if(strApplicationName.IsEmpty())
                strApplicationName  = Config.Get("ApplicationName", "Mondo");

            return(Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\" + strOrganizationName + "\\" + strApplicationName));
        }

        /****************************************************************************/
        [DllImport(@"c:\Windows\System32\advapi32.dll")]
        private static extern long RegDisablePredefinedCacheEx();

        #endregion
    }

    /****************************************************************************/
    /****************************************************************************/
    public static class RegistryExtension
    {
        /****************************************************************************/
        public static void DeleteAllValues(this RegistryKey objKey)
        {
            string[] aNames = objKey.GetValueNames();

            foreach(string strName in aNames)
                objKey.DeleteValue(strName);
        }
    }
 }
