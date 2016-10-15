/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: Resource.cs										    */
/*        Class(es): Resource										        */
/*          Purpose: Loads embedded resources                               */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 17 Sep 2006                                            */
/*                                                                          */
/*   Copyright (c) 2006-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Reflection;
using System.Resources;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public class Resource
    {
        private Assembly m_objAssembly;
        private string m_idResource = "";

        /****************************************************************************/
        private Resource()
        {
        }

        /****************************************************************************/
        public Resource(Assembly objAssembly)
        {
            m_objAssembly = objAssembly;
        }

        /****************************************************************************/
        public Resource(Assembly objAssembly, string idResource)
        {
            m_objAssembly = objAssembly;
            m_idResource = idResource;
        }

        /****************************************************************************/
        public string GetString()
        {
            return(GetString(m_objAssembly, m_idResource));
        }

        /****************************************************************************/
        public Stream GetStream()
        {
            return(GetStream(m_objAssembly, m_idResource));
        }

        /****************************************************************************/
        public virtual string GetString(string strName)
        {
            return(GetString(m_objAssembly, strName));
        }

        /****************************************************************************/
        public virtual Stream GetStream(string strName)
        {
            return(GetStream(m_objAssembly, strName));
        }

        #region Static Versions

        /****************************************************************************/
        public static string GetString(Assembly objAssembly, string strName)
        {
            return(Utility.StreamToString(GetStream(objAssembly, strName)));
        }

        /****************************************************************************/
        public static Stream GetStream(Assembly objAssembly, string strName)
        {
            System.IO.Stream objFile = objAssembly.GetManifestResourceStream(strName);

            if(objFile == null)
                throw new ArgumentException("Resource not found: " + strName);

            return(objFile);
        }

        #endregion
    }
}
