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
            this.Cache = new Cache();
            WebUtil.Cache.AddSetting("AppContext", this);
        }

        /****************************************************************************/
        public override void Validate(ILog log = null)
        {
            // Do nothing
        }

        /****************************************************************************/
        public override string MapPath(string path)
        {
            return(HttpContext.Current.Server.MapPath(path));
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
}
