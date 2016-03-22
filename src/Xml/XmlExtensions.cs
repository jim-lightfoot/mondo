/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Xml							                    */
/*             File: XmlExtensions.cs								        */
/*        Class(es): XmlExtensions       								    */
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
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using Mondo.Common;

namespace Mondo.Xml
{
    /****************************************************************************/
    /****************************************************************************/
	public static class XmlExtensions
    {
        /****************************************************************************/
        public static string GetAttribute(this XmlNode xmlNode, string strAttrName, string defaultVal = "")
        {
            if(xmlNode != null)
            {
                XmlElement xmlElement = xmlNode as XmlElement;

                if(xmlElement.HasAttribute(strAttrName))
                    return(xmlElement.GetAttribute(strAttrName).Trim());
            }

            return(defaultVal);
        }
        
        /****************************************************************************/
        public static T GetAttribute<T>(this XmlNode xmlNode, string strAttrName, T defaultVal = default(T)) where T : struct
        {
            string val = xmlNode.GetAttribute(strAttrName, defaultVal.ToString());

            return(Utility.Convert<T>(val));
        }        
        
        /****************************************************************************/
        public static string GetChildAttribute(this XmlNode xmlNode, string strNodeName, string strAttrName, string defaultVal = "")
        {
            try
            {
                XmlNode xmlChild = EnsureNodePathExists(xmlNode, strNodeName);

                return(xmlChild.GetAttribute(strAttrName));
            }
            catch(Exception)
            {
            }

            return(defaultVal);
        }
       
        /****************************************************************************/
        public static T GetChildAttribute<T>(this XmlNode xmlNode, string strNodeName, string strAttrName, T defaultVal = default(T)) where T : struct
        {
            string val = xmlNode.GetChildAttribute(strNodeName, strAttrName, defaultVal.ToString());

            return(Utility.Convert<T>(val));
        }
    
        /****************************************************************************/
        public static string GetChildText(this XmlNode xmlNode, string strNodeName, string defaultVal = "")
        {
            try
            {
                XmlNode xmlChild = xmlNode.SelectSingleNode(strNodeName);

                return(xmlChild.InnerText.Trim());
            }
            catch
            {
            }

            return(defaultVal);
         }

        /****************************************************************************/
        public static T GetChildText<T>(this XmlNode xmlNode, string strNodeName, T defaultVal = default(T)) where T : struct
        { 
            string val = xmlNode.GetChildText(strNodeName, defaultVal.ToString());

            return(Utility.Convert<T>(val));
        }

        /****************************************************************************/
        public static void SetAttribute(this XmlNode xmlNode, string strAttrName, string strValue)
        {
            XmlElement xmlElement = xmlNode as XmlElement;

            xmlElement.SetAttribute(strAttrName, strValue);
        }

        /****************************************************************************/
        public static XmlNode SetChildText(this XmlNode xmlNode, string strNodeName, string strValue)
        {
            try
            {
                XmlNode xmlChild = EnsureNodePathExists(xmlNode, strNodeName);

                xmlChild.InnerText = strValue.Trim();

                return(xmlChild);
            }
            catch(Exception ex)
            {
                cDebug.Capture(ex);
            }

            return(null);
        }
    
        /****************************************************************************/
        public static XmlNode RootNode(this XmlDocument xmlDoc)
        {
            return(xmlDoc.DocumentElement);
        }

        /****************************************************************************/
        public static XmlNode AppendNode(this XmlNode xmlParent, XmlNode xmlChild)
        {
            return(xmlParent.AppendNode(xmlChild.OuterXml));
        }

        /****************************************************************************/
        public static XmlNode AppendNode(this XmlNode xmlParent, string xmlChild)
        {
            XmlDocumentFragment xmlFragment = xmlParent.OwnerDocument.CreateDocumentFragment();

            xmlFragment.InnerXml = xmlChild;

            return(xmlParent.AppendChild(xmlFragment));
        }

        /****************************************************************************/
        public static XmlDocument AppendToRoot(this XmlDocument xmlDoc, XmlNode xmlChild)
        {
            xmlDoc.AppendToRoot(xmlChild.OuterXml);
            return(xmlDoc);
        }

        /****************************************************************************/
        public static void AppendToRoot(this XmlDocument xmlDoc, string xmlChild)
        {
            xmlDoc.DocumentElement.AppendNode(xmlChild);
        }

        /****************************************************************************/
        public static void ForEachNode(this XmlNode xmlNode, string strChild, Action<XmlNode> action)
        {
            XmlNodeList xmlChildren = xmlNode.SelectNodes(strChild);

            foreach(XmlNode xmlChild in xmlChildren)
            {
                action(xmlChild);
            }
        }

        /****************************************************************************/
        public static bool RemoveChildren(this XmlNode xmlNode, string xPath)
        {
            XmlNodeList aChildren = xmlNode.SelectNodes(xPath);

            if(aChildren.Count == 0)
                return(false);

            foreach(XmlNode xmlChild in aChildren)
                xmlNode.RemoveChild(xmlChild);

            return(true);
        }

        /****************************************************************************/
        public static XmlElement AddChild(this XmlNode xmlNode, string strNodeName, string strValue)
        {
            XmlNode xmlChild = xmlNode.OwnerDocument.CreateElement(strNodeName);

            xmlNode.AppendChild(xmlChild);               

            if(strValue != null)
                xmlChild.InnerText = strValue.Trim();

            return((XmlElement)xmlChild);
        }

        /****************************************************************************/
        public static XmlElement AddChildNode(this XmlNode xmlNode, string strNodeName, string strXml)
        {
            XmlNode xmlChild = xmlNode.OwnerDocument.CreateElement(strNodeName);

            xmlNode.AppendChild(xmlChild);               

            xmlChild.InnerXml = strXml.Trim();

            return((XmlElement)xmlChild);
        }

        #region Private Methods

        /****************************************************************************/
        private static XmlNode EnsureNodePathExists(XmlNode xmlNode, string strNodeName)
        {
            XmlNode xmlChild = xmlNode.SelectSingleNode(strNodeName);

            if(xmlChild != null)
                return(xmlChild);

            return(EnsureNodePathExists(xmlNode, StringList.ParseString(strNodeName, "/", true)));
        }

        /****************************************************************************/
        private static XmlNode EnsureNodePathExists(XmlNode xmlNode, List<string> aNodeNames)
        {
            string  strFirst = aNodeNames[0];
            XmlNode xmlChild = xmlNode.SelectSingleNode(strFirst);

            if(xmlChild == null)
            {
                xmlChild = xmlNode.OwnerDocument.CreateElement(strFirst);
                xmlNode.AppendChild(xmlChild);
            }

            if(aNodeNames.Count == 1)
                return(xmlChild);

            aNodeNames.RemoveAt(0);

            return(EnsureNodePathExists(xmlChild, aNodeNames));
        }

        #endregion
    }
}
