/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  	                                                */
/*                                                                          */
/*      Namespace: Mondo.Common	                                            */
/*           File: ICache.cs                                                */
/*      Class(es): ICache, ICacheDependency, FileDependency, BaseCache,     */
/*                    CacheExtensions                                       */
/*        Purpose: Generic cache interface and generic cache implmentation  */
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
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public interface ICache
    {
        object Get(string key);

        void   Add(string key, object objToAdd);
        void   Add(string key, object objToAdd, DateTime dtExpires, ICacheDependency dependency = null);
        void   Add(string key, object objToAdd, TimeSpan tsExpires, ICacheDependency dependency = null);

        void   Remove(string key);
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface ICacheDependency
    {
        string Type { get; }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class FileDependency : ICacheDependency
    {
        /****************************************************************************/
        public FileDependency(string strFileName)
        {
            this.FileName = strFileName;
        }

        /****************************************************************************/
        public string Type      { get { return("file"); } }
        public string FileName  { get; private set; }
    }

    /****************************************************************************/
    /****************************************************************************/
    public abstract class BaseCache : ICache
    {
        /****************************************************************************/
        public abstract object Get(string key);
        public abstract void   Add(string key, object objToAdd);
        public abstract void   Add(string key, object objToAdd, DateTime dtExpires, ICacheDependency dependency = null);
        public abstract void   Add(string key, object objToAdd, TimeSpan tsExpires, ICacheDependency dependency = null);
        public abstract void   Remove(string key);
    }

    /****************************************************************************/
    /****************************************************************************/
    public static class CacheExtensions 
    {
        /****************************************************************************/
        public static string GetString(this ICache cache, string key)
        {
            return(cache.Get(key).Normalized());
        }

        /****************************************************************************/
        public delegate T CreateItem<T>();

        /****************************************************************************/
        public static T Get<T>(this ICache cache, string key, CreateItem<T> fnCreate, DateTime? dtExpires = null, TimeSpan? tsExpires = null, ICacheDependency dependency = null , ILog log = null)
        {
            object obj = cache.Get(key);

            if(obj == null)
            { 
                obj = fnCreate();

                Task.Run( ()=>
                {
                    try
                    { 
                        if(dtExpires != null)
                            cache.Add(key, obj, dtExpires.Value, dependency);
                        else if(tsExpires != null)
                            cache.Add(key, obj, tsExpires.Value, dependency);
                        else
                            cache.Add(key, obj);
                    }
                    catch(Exception ex)
                    {
                        if(log != null)
                            log.WriteError(ex);
                    }
                });
            }

            return((T)obj);
        }
    }
}
