/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Xml							                    */
/*             File: XmlDoc.cs									            */
/*        Class(es): XmlDoc										            */
/*          Purpose: Xml related utilities                                  */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 12 Sep 2003                                            */
/*                                                                          */
/*   Copyright (c) 2003-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Xml;
using Mondo.Common;

namespace Mondo.Xml
{
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
	/// 
	/// </summary>
	public class XmlDoc
	{
        /****************************************************************************/
        private XmlDoc()
		{
		}

        /****************************************************************************/
        public static XmlDocument Load(XmlReader objReader)
        {
            XmlDocument objDocument = new XmlDocument();

            objDocument.Load(objReader);

            return(objDocument);
        }

        /****************************************************************************/
        public static XmlDocument Load(string strXml)
        {
            return(LoadXml(strXml));
        }

        /****************************************************************************/
        public static XmlDocument LoadXml(string strXml)
        {
            XmlDocument objDocument = new XmlDocument();

            objDocument.LoadXml(strXml);

            return(objDocument);
        }

        /****************************************************************************/
        public static XmlDocument Load(Resource objResource)
        {
            return(Load(objResource.GetString()));
        }

        /****************************************************************************/
        public static XmlDocument LoadFile(string strPath)
        {
            XmlDocument objDocument = new XmlDocument();
          //  string      strXml      = DiskFile.LoadFile(strPath, true);

           // objDocument.LoadXml(strXml);

            objDocument.Load(strPath);

            return(objDocument);
        }

        /****************************************************************************/
        public static XmlElement RootElement(XmlDocument xmlDoc)
        {
            foreach(XmlNode xmlChild in xmlDoc.ChildNodes)
                if(xmlChild.NodeType == XmlNodeType.Element)
                    return((XmlElement)xmlChild);

            return(null);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class cXmlTextReader
    {
        /****************************************************************************/
        private cXmlTextReader()
        {
        }

        /****************************************************************************/
        public static XmlTextReader Load(string strXml)
        {
            XmlParserContext objContext = new XmlParserContext(null, null, "", null, null, null, "", "", XmlSpace.None);
            XmlTextReader    objReader  = new XmlTextReader(strXml, XmlNodeType.Element, objContext);

            return(objReader);
        }

        /****************************************************************************/
        public static XmlTextReader Load(XmlDocument xmlDoc)
        {
            XmlParserContext objContext = new XmlParserContext(null, null, "", null, null, null, "", "", XmlSpace.None);
            XmlTextReader    objReader  = new XmlTextReader(xmlDoc.OuterXml, XmlNodeType.Document, objContext);

            return(objReader);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class ReadXml : IDisposable
    {
        private XmlTextReader m_objReader;

        /****************************************************************************/
        public ReadXml(XmlTextReader objReader)
        {
            m_objReader = objReader;
        }

        /****************************************************************************/
        public XmlTextReader Reader         {get{return(m_objReader);}}

        /****************************************************************************/
        public void Dispose()
        {
            m_objReader.Close();
            m_objReader = null;
        }
    }

}
