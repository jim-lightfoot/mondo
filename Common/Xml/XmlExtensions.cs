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
using System.Xml.XPath;
using System.Text;
using Mondo.Common;

namespace Mondo.Xml
{
    /****************************************************************************/
    /****************************************************************************/
	public static class XmlExtensions
    {
        /****************************************************************************/
        public static bool HasChildElements(this XmlNode xmlNode)
        {
            if(xmlNode is XmlElement)
            {
                foreach(XmlNode child in xmlNode.ChildNodes)
                {
                    if(child is XmlElement)
                      return true;
                }
            }

            return false;
        }

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
            XmlDocumentFragment xmlFragment = GetOwnerDocument(xmlParent).CreateDocumentFragment();

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
        public static void PrependToRoot(this XmlDocument xmlDoc, string xmlChild)
        {
            if(xmlDoc.DocumentElement.ChildNodes.Count == 0)
                xmlDoc.DocumentElement.AppendNode(xmlChild);
            else
            {
                XmlDocumentFragment xmlFragment = xmlDoc.CreateDocumentFragment();

                xmlFragment.InnerXml = xmlChild;

                xmlDoc.DocumentElement.InsertBefore(xmlFragment, xmlDoc.DocumentElement.ChildNodes[0]);
            }
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
        private static XmlDocument GetOwnerDocument(XmlNode xmlNode)
        {        
          if(xmlNode.OwnerDocument != null)
            return xmlNode.OwnerDocument;

            if(xmlNode is XmlDocument)
              return xmlNode as XmlDocument;

            return null;
        }

        /****************************************************************************/
        public static XmlElement AddChild(this XmlNode xmlNode, string strNodeName, string strValue = null)
        {
            XmlNode xmlChild = GetOwnerDocument(xmlNode).CreateElement(strNodeName);

            xmlNode.AppendChild(xmlChild);               

            if(strValue != null)
                xmlChild.InnerText = strValue.Trim();

            return((XmlElement)xmlChild);
        }

        /****************************************************************************/
        public static XmlDocument ToXml(this XPathNodeIterator iterator)
        {
            cXMLWriter xmlResults = new cXMLWriter();

            xmlResults.WriteStartDocument();
                xmlResults.Write(iterator);
            xmlResults.WriteEndDocument();

            return(xmlResults.Xml);
        }

        /****************************************************************************/
        public static XmlElement AddChildNode(this XmlNode xmlNode, string strNodeName, string strXml)
        {
            XmlNode xmlChild = GetOwnerDocument(xmlNode).CreateElement(strNodeName);

            xmlNode.AppendChild(xmlChild);               

            xmlChild.InnerXml = strXml.Trim();

            return((XmlElement)xmlChild);
        }


        /****************************************************************************/
        public static string ToJSON(this XmlDocument xmlDoc)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("{");
            RenderNode(sb, XmlDoc.RootElement(xmlDoc), 2);
            sb.AppendLine("}");

            return sb.ToString();
        }

        #region Private Methods


        /****************************************************************************/
        private static void RenderNode(StringBuilder sb, XmlNode xmlNode, int iIndent)
        {
            if(xmlNode.LocalName == "List")
            {
                RenderList(sb, xmlNode, iIndent);
                return;
            }

            XmlNodeList aChildren = xmlNode.SelectNodes("*");
            string      strIndent = "".PadLeft(iIndent, ' ');

            if(aChildren.Count != 0)
            {
                sb.Append(strIndent, "\"", xmlNode.LocalName, "\":", "\r\n");
                sb.AppendLine(strIndent + "{");

                RenderChildren(sb, aChildren, iIndent);

                sb.AppendLine(strIndent + "}");
            }
            else
            {
                var value = EncodeText(xmlNode);

                sb.Append(strIndent, "\"", xmlNode.LocalName, "\": ", value);
            }
        }

        /****************************************************************************/
        private static string EncodeText(XmlNode xmlNode)
        {       
            string value  = xmlNode.InnerText.Trim();
            double dValue = 0d;
            string type   = xmlNode.GetAttribute("type", "");

            if(value == "true" || value == "false")
                return value;

            if(type != "string" && double.TryParse(value, out dValue))
                return value;

            return "\"" + value.Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n") + "\"";
        }

        /****************************************************************************/
        private static void RenderChildren(StringBuilder sb, XmlNodeList aChildren, int iIndent)
        {
            int nItems = aChildren.Count;

            for(int i = 0; i < nItems; ++i)
            {
                XmlNode xmlChild = aChildren[i];

                RenderNode(sb, xmlChild, iIndent+2);

                if(i < (nItems-1))
                    sb.AppendLine(",");
                else
                    sb.AppendLine("");
            }
        }

        /****************************************************************************/
        private static void RenderList(StringBuilder sb, XmlNode xmlNode, int iIndent)
        {
            XmlNodeList aChildren = xmlNode.SelectNodes("*");
            string      strIndent = "".PadLeft(iIndent, ' ');

            sb.Append(strIndent, "\"", xmlNode.GetAttribute("name"), "\":", "\r\n");
            sb.AppendLine(strIndent + "[");

            int nItems = aChildren.Count;

            if(nItems > 0)
            {            
                var childNodes = aChildren[0].SelectNodes("*");

                // If first node has no child nodes then this is an array of strings
                if(childNodes.Count == 0)
                {
                    var list = new StringList();    
                    
                    foreach(XmlNode xmlChild in aChildren)
                        list.Add(EncodeText(xmlChild));

                    sb.AppendLine(list.Pack(","));
                }
                else
                {
                    string strIndent2 = "".PadLeft(iIndent+2, ' ');

                    for(int i = 0; i < nItems; ++i)
                    {
                        var xmlChild = aChildren[i];

                        sb.AppendLine(strIndent2 + "{");

                        RenderChildren(sb, xmlChild.SelectNodes("*"), iIndent+2);

                        if(i < (nItems-1))
                            sb.AppendLine(strIndent2 + "},");
                        else
                            sb.AppendLine(strIndent2 + "}");
                    }
                }
            }

            sb.Append(strIndent + "]");
        }

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
                xmlChild = GetOwnerDocument(xmlNode).CreateElement(strFirst);
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
