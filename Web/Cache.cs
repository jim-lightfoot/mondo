/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  	                                                */
/*                                                                          */
/*      Namespace: Mondo.Web	                                            */
/*           File: Cache.cs                                                 */
/*      Class(es): Cache                                                    */
/*        Purpose: Web caching                                              */
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
using System.Web;
using System.Web.Caching;

using Mondo.Common;

namespace Mondo.Web
{
    /****************************************************************************/
    /****************************************************************************/
    public class Cache : BaseCache
    {
        /****************************************************************************/
        public override object Get(string key)
        {
            return(System.Web.Hosting.HostingEnvironment.Cache.Get(key));
        }

        /****************************************************************************/
        public override void Add(string key, object objToAdd)
        {
            Add(key, objToAdd, new TimeSpan(1, 0, 0));
        }

        /****************************************************************************/
        public override void Add(string key, object objToAdd, DateTime expires, ICacheDependency dependency = null)
        {
            try
            { 
                System.Web.Hosting.HostingEnvironment.Cache.Insert(key, objToAdd, ToCacheDependency(dependency), expires, System.Web.Caching.Cache.NoSlidingExpiration);
            }
            catch
            {
            }
        }

        /****************************************************************************/
        public override void Add(string key, object objToAdd, TimeSpan expires, ICacheDependency dependency = null)
        {
            try
            { 
                System.Web.Hosting.HostingEnvironment.Cache.Insert(key, objToAdd, ToCacheDependency(dependency), System.Web.Caching.Cache.NoAbsoluteExpiration, expires);
            }
            catch
            {
            }
        }

        /****************************************************************************/
        public override void Remove(string key)
        {
            try
            { 
                System.Web.Hosting.HostingEnvironment.Cache.Remove(key);
            }
            catch
            {
            }
        }

        /****************************************************************************/
        private static CacheDependency ToCacheDependency(ICacheDependency dependency)
        {
            if(dependency == null)
                return(null);

            if(dependency is FileDependency)
                return(new CacheDependency((dependency as FileDependency).FileName));

            // Don't understand any other types for now
            return(null);
        }
    }
}
