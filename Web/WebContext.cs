/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  	                                                */
/*                                                                          */
/*        Namespace: Mondo.Web							                    */
/*             File: WebContext.cs											*/
/*        Class(es): WebContext 							                */
/*          Purpose: Provide an Http request context  	                    */
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;

using Mondo.Common;

namespace Mondo.Web
{
    /****************************************************************************/
    /****************************************************************************/
    public class WebContext : ApplicationContext
    {
        /****************************************************************************/
        public WebContext() : this(true, true)
        {
        }

        /****************************************************************************/
        protected WebContext(bool config, bool log) : base(config, log)
        {
            this.Cache   = new Cache();
            this.Cookies = new WebCookies();
            this.Session = new WebSession();
            this.Request = new WebRequest();

            WebUtil.Cache.AddSetting("AppContext", this);
        }

        /****************************************************************************/
        public override long LocationID
        {
            get { return WebUtil.IPNumber; }
        }

        /****************************************************************************/
        public override void Validate(ILog log = null)
        {
            // Do nothing
        }

        /****************************************************************************/
        public override string MapPath(string path)
        {
            if(string.IsNullOrWhiteSpace(path))
              path = "";

            return(WebUtil.MapPath(path));
        }

        /****************************************************************************/
        public override IEncryptor Encryptor
        {
            get 
            { 
                return null; 
            }
        }

        /****************************************************************************/
        public static IAppContext Current
        {
            get
            {
                IAppContext context = WebUtil.Cache.GetSetting("AppContext") as IAppContext;

                if(context == null)
                  context = new WebContext();

                return context;
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class WebCookies : ISettingsStore
    {
        /****************************************************************************/
        public object GetSetting(string name)
        {
    		return WebUtil.Cookie.Get(name);
        }

        /****************************************************************************/
        public void AddSetting(string name, object value, int expires = 1, bool httpOnly = true, bool secure = true, string domain = "")
        {
            WebUtil.Cookie.Add(name, value.ToString(), expires, httpOnly, secure, domain);
        }

        /****************************************************************************/
        public void RemoveSetting(string name, string domain = "")
        {
            WebUtil.Cookie.Remove(name, domain);
        }
    }

    /****************************************************************************
     * These are cookies that expire when the browser is closed
    /****************************************************************************/
    internal class WebSession : ISettingsStore
    {
        /****************************************************************************/
        public object GetSetting(string name)
        {
    		return WebUtil.BrowserSession.GetSetting(name);
        }

        /****************************************************************************/
        public void AddSetting(string name, object value, int expires = 1, bool httpOnly = true, bool secure = true, string domain = "")
        {
            WebUtil.BrowserSession.AddSetting(name, value, domain);        
        }

        /****************************************************************************/
        public void RemoveSetting(string name, string domain = "")
        {
            WebUtil.BrowserSession.RemoveSetting(name);
        }
    }

    /****************************************************************************
     * These settings are for the lifetime of the page request
    /****************************************************************************/
    internal class WebRequest : ISettingsStore
    {
        /****************************************************************************/
        public object GetSetting(string name)
        {
    		return WebUtil.PageRequest.GetSetting(name);
        }

        /****************************************************************************/
        public void AddSetting(string name, object value, int expires = 1, bool httpOnly = true, bool secure = true, string domain = "")
        {
            WebUtil.PageRequest.AddSetting(name, value.ToString());        
        }

        /****************************************************************************/
        public void RemoveSetting(string name, string domain = "")
        {
            WebUtil.PageRequest.RemoveSetting(name);
        }
    }
}
