/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  	                                            */
/*                                                                          */
/*        Namespace: Mondo.Web						    */
/*             File: WebUtil.cs						    */
/*        Class(es): WebUtil						    */
/*          Purpose: Web specific utility functions                         */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 29 Nov 2015                                            */
/*                                                                          */
/*   Copyright (c) 2015-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Linq;
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
                // ??? IPv6 are 128 bits. Need to change this to a guid

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
        public static void Redirect(string path)
        {
            HttpContext.Current.Response.Redirect(path);
        }

        /*************************************************************************/
        public static string RelativePath(string path)
        {
            if(path.StartsWith("~/"))
            {
                string strAppPath = HttpContext.Current.Request.ApplicationPath;

                return(path.Replace("~/", strAppPath + "/").Replace("//", "/"));
            }

            return(path.Replace("//", "/"));
        }

        /*************************************************************************/
        public static string AbsolutePath(string path)
        {
            if(path.StartsWith("~/"))
            {
                string strWebPath = MapPath("~/d.html");

                strWebPath = strWebPath.Substring(0, strWebPath.Length - "d.html".Length) + path.Substring(1);

                return(strWebPath);
            }

            return(path);
        }

        /*************************************************************************/
        public static string RelativeFromAbsolutePath(string path)
        {
            if(path.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
                return(path);

            string webPath = MapPath("~/d.html");

            webPath = webPath.Substring(0, webPath.Length - "d.html".Length);

            int iIndex = path.IndexOf(webPath, StringComparison.CurrentCultureIgnoreCase);

            if(iIndex == -1)
                return(path);

            webPath = "~/" + path.Substring(iIndex + webPath.Length);

            return(RelativePath(webPath.Replace(System.IO.Path.DirectorySeparatorChar, '/')));
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
        public static string MapPath(string path)
        {
            return System.Web.Hosting.HostingEnvironment.MapPath(path);
        }

        /*************************************************************************/
        public static string PageName(string strURL)
        {
            string pageName = strURL;

            if(pageName.Contains("?"))
                pageName = pageName.Substring(0, pageName.IndexOf("?"));
           
            if(pageName.Contains("/"))
                pageName = pageName.Substring(pageName.LastIndexOf("/") + 1);

            return(pageName.ToLower());
        }

        /****************************************************************************/
        public static string GetUrlParam(string name)
        {
            return(GetUrlParam(name, "", true));
        }

        /****************************************************************************/
        public static string GetUrlParam(string name, string defaultVal)
        {
            return(GetUrlParam(name, defaultVal, true));
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
        public static string GetUrlParam(string name, string defaultVal, bool bNoTrim)
        {
            object val = HttpContext.Current.Request.QueryString[name];

            if(val == null)
                return(defaultVal);

            if(bNoTrim)
                return(val.ToString());

            return(val.Normalized());
        }

        /****************************************************************************/
        public static T GetUrlParam<T>(string name, T defaultVal = default(T)) where T : struct
        {
            object val = HttpContext.Current.Request.QueryString[name];

            try
            {
                return(Utility.Convert<T>(val, defaultVal, true));
            }
            catch
            { 
                throw new Exception("The URL param named \"" + name + "\" with a value of \"" + val.ToString() + "\" is not a valid " + typeof(T).ToString().Replace("System.", ""));
            }        
        }

        /****************************************************************************/
        public static string GetFormParam(string name)
        {
            return(HttpContext.Current.Request.Form[name].Normalized());
        }

        /****************************************************************************/
        public static T GetFormParam<T>(string name, T defaultVal = default(T)) where T : struct
        {
            object val = HttpContext.Current.Request.Form[name];

            return(Utility.Convert<T>(val, defaultVal));
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
                return HttpContext.Current.Request.Url.ToString().ToLower();
            }            
        }

        #region Cookies

        /*********************************************************************/
        public class Cookie
        {
            public const int Session = -99999;
            public const int Delete  = -1;

            /*********************************************************************/
            public static string Get(string name)
            {
                if(HttpContext.Current.Request.Cookies[name] != null)
                    return(WebUtil.UrlDecode(HttpContext.Current.Request.Cookies[name].Value));

                return("");
            }

            /*********************************************************************/
            public static void Add(string name, string strValue, string domain = "")
            {
                Add(name, strValue, 30, domain);
            }

            /*********************************************************************/
            public static void Add(string name, int iValue, string domain = "")
            {
                Add(name, iValue.ToString(), domain);
            }

            /*********************************************************************/
            public static void Add(string name, string strValue, int nExpires, string strDomain)
            {
                Add(name, strValue, nExpires, true, true, strDomain);
            }

            /*********************************************************************/
            public static void Add(string name, string strValue, int nExpires, bool bHttpOnly, bool bSecure, string strDomain)
            {
                try
                {
                    HttpCookie objCookie = HttpContext.Current.Request.Cookies[name];
                
                    if(objCookie == null)
                        objCookie = new HttpCookie(name);

                    objCookie.Value     = WebUtil.UrlEncode(strValue);
                    objCookie.Shareable = true; /// false;
                    objCookie.HttpOnly  = false; /// bHttpOnly;
                    objCookie.Secure    = false; /// bSecure;

                    //if(strDomain != "")
                    //    objCookie.Domain = strDomain;

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
            public static void Remove(string name, string domain = "")
            {
                try
                {
                    // Remove the cookie from the Request so that it is no longer available server-side
                    HttpContext.Current.Request.Cookies.Remove(name);
                    HttpContext.Current.Request.Cookies.Remove(name);

                    // Ensure it is deleted from the user's browser
                    Add(name, "", Cookie.Delete, domain);
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
            public static void AddSetting(string key, object value, string domain)
            {
                WebUtil.Cookie.Add(key, value.ToString(), kExpires, domain);
            }

            /****************************************************************************/
            public static string GetSetting(string key)
            {
                return(WebUtil.Cookie.Get(key));
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
                        string language = aLanguages[0].ToLowerInvariant().Trim(); 

                        return(language);
                    } 
                    catch 
                    { 
                        return(""); 
                    } 
                }
            }

            /****************************************************************************/
            public static void RemoveSetting(string key)
            {
                WebUtil.Cookie.Remove(key);
            }
        }

        /****************************************************************************
         *** Provides settings that last the lifetime of the current page request
         ****************************************************************************/
        public static class Cache
        {
            /****************************************************************************/
            public static void AddSetting(string key, object value)
            {
                AddSetting(key, value, new TimeSpan(1, 0, 0));
            }

            /****************************************************************************/
            public static void AddSetting(string key, object value, TimeSpan tsExpires)
            {
                System.Web.Hosting.HostingEnvironment.Cache.Insert(key, value, null, System.Web.Caching.Cache.NoAbsoluteExpiration, tsExpires);
            }

            /****************************************************************************/
            public static object GetSetting(string key)
            {
                return(System.Web.Hosting.HostingEnvironment.Cache[key]);
            }
        }

        /****************************************************************************
         *** Provides settings that last the lifetime of the current page request
         ****************************************************************************/
        public static class PageRequest
        {
            /****************************************************************************/
            public static void AddSetting(string key, object value)
            {
                HttpContext.Current.Items.Add(key, value);
            }

            /****************************************************************************/
            public static object GetSetting(string key)
            {
                return(HttpContext.Current.Items[key]);
            }

            /****************************************************************************/
            public static void RemoveSetting(string key)
            {
                HttpContext.Current.Items.Remove(key);
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
