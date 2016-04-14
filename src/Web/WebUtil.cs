/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: WebUtil.cs										        */
/*        Class(es): WebUtil										        */
/*          Purpose: Web specific utility functions                         */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 22 Sep 2007                                            */
/*                                                                          */
/*   Copyright (c) 2007 - Tenth Generation Software, All rights reserved    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;

using Mondo.Common;

namespace Mondo.Web
{
    /*********************************************************************/
    /*********************************************************************/
    public sealed class WebUtil
    {
        /*********************************************************************/
        private WebUtil()
        {
        }

        /*************************************************************************/
        public static long IPNumber
        {
            get
            {
              #if DEBUG
                // For some reason internal network IPs fail
                return(32767);
              #else
                StringList aParts = new StringList(IPAddress, ".", true);

                if(aParts.Count != 4)
                    return(0);

                long iIP   = 0;
                long iPart = 0;

                for(int i = 0; i < 4; ++i)
                {
                    if(!long.TryParse(aParts[i], out iPart))
                        return(0);

                    iIP |= iPart << ((3 - i) * 8);
                }

                return(iIP);
              #endif
            }
        }

        /*************************************************************************/
        public static string IPAddress
        {
            get
            {
                return(HttpContext.Current.Request.UserHostAddress);               
            }
        }

        /*************************************************************************/
        public static string RelativePath(string strPath)
        {
            if(strPath.StartsWith("~/"))
            {
                string strAppPath = HttpContext.Current.Request.ApplicationPath;

                return(strPath.Replace("~/", strAppPath + "/").Replace("//", "/"));
            }

            return(strPath.Replace("//", "/"));
        }

        /*************************************************************************/
        public static string AbsolutePath(string strPath)
        {
            if(strPath.StartsWith("~/"))
            {
                string strWebPath = HttpContext.Current.Request.MapPath("~/d.html");

                strWebPath = strWebPath.Substring(0, strWebPath.Length - "d.html".Length) + strPath.Substring(1);

                return(strWebPath);
            }

            return(strPath);
        }

        /*************************************************************************/
        public static string RelativeFromAbsolutePath(string strPath)
        {
            if(strPath.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
                return(strPath);

            string strWebPath = HttpContext.Current.Request.MapPath("~/d.html");

            strWebPath = strWebPath.Substring(0, strWebPath.Length - "d.html".Length);

            int iIndex = strPath.IndexOf(strWebPath, StringComparison.CurrentCultureIgnoreCase);

            if(iIndex == -1)
                return(strPath);

            strWebPath = "~/" + strPath.Substring(iIndex + strWebPath.Length);

            return(RelativePath(strWebPath.Replace(System.IO.Path.DirectorySeparatorChar, '/')));
        }

        /*************************************************************************/
        public static string CurrentPageName
        {
            get
            {
                return(PageName(HttpContext.Current.Request.Url.ToString()));
            }
        }

        /*************************************************************************/
        public static string MapPath(string strPath)
        {
            return(HttpContext.Current.Request.MapPath(strPath));
        }

        /*************************************************************************/
        public static string PageName(string strURL)
        {
            string strPageName = strURL;

            if(strPageName.Contains("?"))
                strPageName = strPageName.Substring(0, strPageName.IndexOf("?"));
           
            if(strPageName.Contains("/"))
                strPageName = strPageName.Substring(strPageName.LastIndexOf("/") + 1);

            return(strPageName.ToLower());
        }

        /****************************************************************************/
        public static string GetUrlParam(string strName)
        {
            return(GetUrlParam(strName, "", true));
        }

        /****************************************************************************/
        public static string GetUrlParam(string strName, string strDefault)
        {
            return(GetUrlParam(strName, strDefault, true));
        }

        /****************************************************************************/
        public static string GetUrlParam(string strName, string strDefault, bool bNoTrim)
        {
            object objValue = HttpContext.Current.Request.QueryString[strName];

            if(objValue == null)
                return(strDefault);

            if(bNoTrim)
                return(objValue.ToString());

            return(objValue.Normalized());
        }

        /****************************************************************************/
        public static long GetUrlParamInt(string strName, long iDefault)
        {
            string strValue = HttpContext.Current.Request.QueryString[strName].Normalized();

            if(strValue == "")
                return(iDefault);

            long iValue = 0L;

            if(!long.TryParse(strValue, out iValue))
                throw new Exception("The URL param named \"" + strName + "\" with a value of \"" + strValue + "\" is not a valid integer.");

            return(iValue);
        }

        /****************************************************************************/
        public static long GetUrlParamInt(string strName)
        {
            return(GetUrlParamInt(strName, 0L));
        }

        /*********************************************************************/
        public static string UrlEncode(string strURL)
        {
            return(HttpUtility.UrlEncode(strURL));
        }

        /*********************************************************************/
        public static string UrlDecode(string strURL)
        {
            return(HttpUtility.UrlDecode(strURL));
        }

        /****************************************************************************/
        public static string GetFormParam(string strName)
        {
            return(HttpContext.Current.Request.Form[strName].Normalized());
        }

        /****************************************************************************/
        public static int GetFormParamInt(string strName, int iDefault)
        {
            return(Utility.ToInt(HttpContext.Current.Request.Form[strName], iDefault));
        }

        /****************************************************************************/
        public static int GetFormParamInt(string strName)
        {
            return(Utility.ToInt(HttpContext.Current.Request.Form[strName]));
        }

        /****************************************************************************/
        public static string Referrer
        {
            get
            {
                return(HttpContext.Current.Request.UrlReferrer.Normalized().ToLower());
            }            
        }

        /****************************************************************************/
        public static string CurrentUrl
        {
            get
            {
                return(HttpContext.Current.Request.Url.ToString().ToLower());
            }            
        }

        #region Cookies

        /*********************************************************************/
        public class Cookie
        {
            public const int Session = -99999;
            public const int Delete  = -1;

            /*********************************************************************/
            public static string Get(string strName)
            {
                if(HttpContext.Current.Request.Cookies[strName] != null)
                    return(WebUtil.UrlDecode(HttpContext.Current.Request.Cookies[strName].Value));

                return("");
            }

            /*********************************************************************/
            public static void Add(string strName, string strValue)
            {
                Add(strName, strValue, 30);
            }

            /*********************************************************************/
            public static void Add(string strName, int iValue)
            {
                Add(strName, iValue.ToString());
            }

            /*********************************************************************/
            public static void Add(string strName, string strValue, int nExpires)
            {
                string strDomain = "zoomla.com";

              #if DEBUG
                strDomain = "";
              #endif

                Add(strName, strValue, nExpires, true, true, strDomain);
            }

            /*********************************************************************/
            public static void Add(string strName, string strValue, int nExpires, bool bHttpOnly, bool bSecure, string strDomain)
            {
                try
                {
                    HttpCookie objCookie = HttpContext.Current.Request.Cookies[strName];
                
                    if(objCookie == null)
                        objCookie = new HttpCookie(strName);

                    objCookie.Value     = WebUtil.UrlEncode(strValue);
                    objCookie.Shareable = false;
                    objCookie.HttpOnly  = bHttpOnly;
                    objCookie.Secure    = bSecure;

                    if(strDomain != "")
                        objCookie.Domain = strDomain;

                    if(nExpires == Cookie.Session)
                        objCookie.Expires = DateTime.MinValue;
                    else
                        objCookie.Expires = DateTime.Now.AddDays(nExpires);

                    HttpContext.Current.Response.Cookies.Add(objCookie);
                }
                catch
                {
                }
            }

            /*********************************************************************/
            public static void Remove(string strName)
            {
                try
                {
                    // Remove the cookie from the Request so that it is no longer available server-side
                    HttpContext.Current.Request.Cookies.Remove(strName);

                    // Ensure it is deleted from the user's browser
                    Add(strName, "", Cookie.Delete);
                }
                catch
                {
                }
            }
        }

        #endregion

        /****************************************************************************
         *** Provides settings that last the lifetime of the current browser session 
         ****************************************************************************/
        public static class BrowserSession
        {
            private const int kExpires = Cookie.Session;

            /****************************************************************************/
            public static void AddSetting(string strKey, object objValue)
            {
                WebUtil.Cookie.Add(strKey, objValue.ToString(), kExpires);
            }

            /****************************************************************************/
            public static string GetSetting(string strKey)
            {
                return(WebUtil.Cookie.Get(strKey));
            }

            /****************************************************************************/
            public static string Language
            {
                get
                {
                    string[] aLanguages = HttpContext.Current.Request.UserLanguages; 

                    if(aLanguages == null || aLanguages.Length == 0) 
                        return(""); 

                    try
                    { 
                        string strLanguage = aLanguages[0].ToLowerInvariant().Trim(); 

                        //return(CultureInfo.CreateSpecificCulture(strLanguage)); 
                        return(strLanguage);
                    } 
                    catch 
                    { 
                        return(""); 
                    } 
                }
            }

            /****************************************************************************/
            public static void RemoveSetting(string strKey)
            {
                WebUtil.Cookie.Remove(strKey);
            }
        }

        /****************************************************************************
         *** Provides settings that last the lifetime of the current page request
         ****************************************************************************/
        public static class Cache
        {
            /****************************************************************************/
            public static void AddSetting(string strKey, object objValue)
            {
                AddSetting(strKey, objValue, new TimeSpan(1, 0, 0));
            }

            /****************************************************************************/
            public static void AddSetting(string strKey, object objValue, TimeSpan tsExpires)
            {
                HttpContext.Current.Cache.Insert(strKey, objValue, null, System.Web.Caching.Cache.NoAbsoluteExpiration, tsExpires);
            }

            /****************************************************************************/
            public static object GetSetting(string strKey)
            {
                return(HttpContext.Current.Cache[strKey]);
            }
        }

        /****************************************************************************
         *** Provides settings that last the lifetime of the current page request
         ****************************************************************************/
        public static class PageRequest
        {
            /****************************************************************************/
            public static void AddSetting(string strKey, object objValue)
            {
                HttpContext.Current.Items.Add(strKey, objValue);
            }

            /****************************************************************************/
            public static object GetSetting(string strKey)
            {
                return(HttpContext.Current.Items[strKey]);
            }

            /****************************************************************************/
            public static void RemoveSetting(string strKey)
            {
                HttpContext.Current.Items.Remove(strKey);
            }
        }

        /****************************************************************************
         *** Provides information about MIME types 
         ****************************************************************************/
        public static class MIMETypes
        {
            /****************************************************************************/
            public static bool IsText(string strMIMEType)
            {
                strMIMEType = strMIMEType.ToLower();

                if(strMIMEType.StartsWith("text"))
                    return(true);

                if(strMIMEType.EndsWith("xml"))
                    return(true);

                if(strMIMEType.Contains("json"))
                    return(true);

                if(strMIMEType.Contains("javascript"))
                    return(true);

                if(strMIMEType.Contains("edi-x12"))
                    return(true);

                if(strMIMEType.Contains("edifact"))
                    return(true);

                if(strMIMEType.Contains("/xml"))
                    return(true);

                return(false);
            }
        }
    }
}
