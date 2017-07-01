/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Xml.Xsl							                */
/*             File: Transform.cs									        */
/*        Class(es): Transform										        */
/*          Purpose: A class to facilitate Xslt transformations             */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 7 Jun 2008                                             */
/*                                                                          */
/*   Copyright (c) 2008-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Xml.Schema;

using Mondo.Common;

namespace Mondo.Xml.Xsl
{
    /****************************************************************************/
    /****************************************************************************/
    public static class Transform
    {
        /****************************************************************************/
        public static XslCompiledTransform FromStream(Stream objStream)
        {
            return(FromStream(objStream, XsltSettings.Default, new XmlUrlResolver()));
        }

        /****************************************************************************/
        private static XslCompiledTransform CreateTransform(bool bOmitXmlDeclaration = false)
        {
            XslCompiledTransform transform = null;

           #if DEBUG
            transform = new XslCompiledTransform(true);
          #else
            transform = new XslCompiledTransform();
          #endif

            if(transform.OutputSettings != null)
            { 
                transform.OutputSettings.NamespaceHandling = NamespaceHandling.OmitDuplicates;

                if(bOmitXmlDeclaration)
                    transform.OutputSettings.OmitXmlDeclaration = true;

               #if DEBUG
                transform.OutputSettings.Indent = true;
               #endif
            }

            return(transform);
       }

        /****************************************************************************/
        public static XslCompiledTransform FromStream(Stream objStream, XsltSettings objSetting, XmlResolver objResolver, bool bOmitXmlDeclaration = false)
        {
            XslCompiledTransform objTransform = CreateTransform(bOmitXmlDeclaration);
            XPathDocument objXslDocument = new XPathDocument(objStream);

            objTransform.Load(objXslDocument, objSetting, objResolver);

            return(objTransform);
        }

        /****************************************************************************/
        public static XslCompiledTransform FromFile(string strPath, XsltSettings objSetting, XmlResolver objResolver, bool bOmitXmlDeclaration = false)
        {
            XslCompiledTransform objTransform = CreateTransform(bOmitXmlDeclaration);

            objTransform.Load(strPath, objSetting, objResolver);

            return(objTransform);
        }

        /****************************************************************************/
        public static XslCompiledTransform FromString(string strXslt)
        {
            return(FromString(strXslt, XsltSettings.Default, new XmlUrlResolver()));
        }

        /****************************************************************************/
        public static XslCompiledTransform FromString(string strXslt, XmlResolver objResolver)
        {
            return(FromString(strXslt, XsltSettings.Default, objResolver));
        }

        /****************************************************************************/
        public static XslCompiledTransform FromString(string strXslt, XsltSettings objSetting, XmlResolver objResolver)
        {
            XslCompiledTransform objTransform = CreateTransform();
            StringReader         objReader    = new StringReader(strXslt);

            try
            {
                XPathDocument objXslDocument = new XPathDocument(objReader);

                objTransform.Load(objXslDocument, objSetting, objResolver);
            }
            finally
            {
                objReader.Close();
            }

            return(objTransform);
        }

        /*********************************************************************/
        public static string Run(XslCompiledTransform objTransform, IXPathNavigable xmlDoc, XsltArgumentList objArgs)
        {
            using(MemoryStream objStream = new MemoryStream())
            { 
                objTransform.Transform(xmlDoc, objArgs, objStream);

                objStream.Seek(0, SeekOrigin.Begin);

                string strXml = Utility.StreamToString(objStream);

                return(strXml.Replace("xmlns:xsi=\"http://www.w3.org/1999/XMLSchema/instance\"", ""));
            }
        }

        /****************************************************************************/
        public static XPathDocument XPathDocFromString(string strXml)
        {
            using(StringReader objReader = new StringReader(strXml))
            {
                return(new XPathDocument(objReader));
            }
        }

        /****************************************************************************/
        public static XPathDocument XPathDocFromString(string strXml, XmlReaderSettings objSettings)
        {
            using(StringReader objStringReader = new StringReader(strXml))
            {
                XmlReader objReader = XmlReader.Create(objStringReader, objSettings);

                try
                {
                    return(new XPathDocument(objReader));
                }
                finally
                {
                    objReader.Close();
                }
            }
        }
    }
}
