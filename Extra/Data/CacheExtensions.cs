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
/*   Copyright (c) 2015 - Jim Lightfoot, All rights reserved                */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public static class CacheExtensions
    {
        public delegate T LoadDataProc<T>(string name);
        public delegate ICacheDependency CreateDependency(string name);

        /****************************************************************************/
        public static T LoadObject<T>(this ICache      cache, 
                                      string           strName, 
                                      LoadDataProc<T>  fnLoad, 
                                      TimeSpan         tsExpires,
                                      CreateDependency fnCreateDep = null) 
        {
            try
            {
                T objData = default(T);

                try
                {
                    // Check to see if the file is in the cache
                    objData = (T)cache.Get(strName);
                }
                catch
                {
                }

                if(objData == null)
                {
                    // Create the object
                    objData = fnLoad(strName);

                    try
                    {
                        ICacheDependency objDependency = fnCreateDep == null ? null : fnCreateDep(strName);

                        cache.Add(strName, objData, tsExpires, objDependency);
                    }
                    catch
                    {
                        // Can't cache - do nothing
                    }
                }

                return(objData);
            }
            catch(Exception ex)
            {
                throw new Exception("Error in loading data: [" + strName + "]", ex);
            }
        }

        /****************************************************************************/
        public static IXPathNavigable LoadXml(this ICache cache, 
                                            string      strFullPath,
                                            TimeSpan    tsExpires) 
        {
           return(cache.LoadObject<IXPathNavigable>(strFullPath,
                                                    (path)=> { return(new XPathDocument(path)); },
                                                    tsExpires,
                                                    (path)=> { return(new FileDependency(path)); }));

        }
    }
}
