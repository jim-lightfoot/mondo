/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: ICache.cs					    		                */
/*        Class(es): ICache				         		                    */
/*          Purpose: Generic cache interface                                */
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

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public interface ICache
    {
        object Get(string strKey);
        string GetString(string strKey);

        void   Add(string strKey, object objToAdd);
        void   Add(string strKey, object objToAdd, DateTime dtExpires, ICacheDependency dependency = null);
        void   Add(string strKey, object objToAdd, TimeSpan tsExpires, ICacheDependency dependency = null);

        void   Remove(string strKey);
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
        public string GetString(string strKey)
        {
            return(Get(strKey).Normalized());
        }

        /****************************************************************************/
        public abstract object Get(string strKey);
        public abstract void   Add(string strKey, object objToAdd);
        public abstract void   Add(string strKey, object objToAdd, DateTime dtExpires, ICacheDependency dependency = null);
        public abstract void   Add(string strKey, object objToAdd, TimeSpan tsExpires, ICacheDependency dependency = null);
        public abstract void   Remove(string strKey);
    }
}
