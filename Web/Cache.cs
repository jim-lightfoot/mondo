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
        public override object Get(string strKey)
        {
            return(HttpContext.Current.Cache.Get(strKey));
        }

        /****************************************************************************/
        public override void Add(string strKey, object objToAdd)
        {
            Add(strKey, objToAdd, new TimeSpan(1, 0, 0));
        }

        /****************************************************************************/
        public override void Add(string strKey, object objToAdd, DateTime dtExpires, ICacheDependency dependency = null)
        {
            try
            { 
                HttpContext.Current.Cache.Insert(strKey, objToAdd, ToCacheDependency(dependency), dtExpires, System.Web.Caching.Cache.NoSlidingExpiration);
            }
            catch
            {
            }
        }

        /****************************************************************************/
        public override void Add(string strKey, object objToAdd, TimeSpan tsExpires, ICacheDependency dependency = null)
        {
            try
            { 
                HttpContext.Current.Cache.Insert(strKey, objToAdd, ToCacheDependency(dependency), System.Web.Caching.Cache.NoAbsoluteExpiration, tsExpires);
            }
            catch
            {
            }
        }

        /****************************************************************************/
        public override void Remove(string strKey)
        {
            try
            { 
                HttpContext.Current.Cache.Remove(strKey);
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
