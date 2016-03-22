/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Xml							                    */
/*             File: cXMLWriter.cs										    */
/*        Class(es): cXMLWriter										        */
/*          Purpose: An XML writer                                          */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 3 Jan 2002                                             */
/*                                                                          */
/*   Copyright (c) 2002-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using Mondo.Common;

namespace Mondo.Xml
{
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// An XML writer that writes out a valid XML document
    /// </summary>
    public class cXMLWriter : Openable
    {
        private StringBuilder m_objXMLWriter;
        private string        m_strPath;
        private bool          m_bEndDocument        = false;
        private bool          m_bHeader             = false;
        private string        m_strIndent           = "";
        private Stack         m_sTags               = new Stack();
        private bool          m_bCurrentTagEnded    = true;
        private bool          m_bSingleLine         = false;
        private TextWriter    m_objTextWriter       = null;
        private string        m_strRoot             = "";

        /****************************************************************************/
        public cXMLWriter()
        {
            m_objXMLWriter = new StringBuilder();
            m_strPath = "";
        }

        /****************************************************************************/
        public cXMLWriter(TextWriter objWriter)
        {
            m_objXMLWriter = new StringBuilder();
            m_strPath = "";

            m_objTextWriter = objWriter;
        }

        /****************************************************************************/
        public cXMLWriter(string strPath)
        {
            m_objXMLWriter = new StringBuilder();
            m_strPath = strPath;
        }

        /****************************************************************************/
        public void Clear()
        {
            m_objXMLWriter.Remove(0, m_objXMLWriter.Length);
        }

        /****************************************************************************/
        public override void Open()
        {
            base.Open();
            WriteStartDocument();
        }

        /****************************************************************************/
        public override void Close()
        {
            WriteEndDocument();
            base.Close();
        }

        /****************************************************************************/
        public virtual void WriteStartDocument(string strFunctionName, string strXSL)
        {
        }

        /****************************************************************************/
        public virtual void WriteStartDocument()
        {
        }

        /****************************************************************************/
        public string Pop()
        {
            FinishStartTag();
            string strReturn = m_objXMLWriter.ToString();

            m_objXMLWriter.Length = 0;

            return(strReturn);
        }

        /****************************************************************************/
        public virtual void WriteStartDocument(bool bHeader)
        {
            m_bHeader = bHeader;
        }

        /****************************************************************************/
        public virtual void WriteEndDocument()
        {
            if(!m_bEndDocument)
            {
                m_bEndDocument = true;

                if(m_strPath.Length > 0)
                    ToFile(m_strPath);

                if(m_objTextWriter != null)
                    m_objTextWriter.Write(m_objXMLWriter.ToString());
            }
        }

        /****************************************************************************/
        private void FinishStartTag()
        {
            FinishStartTag(true);
        }

        /****************************************************************************/
        private void FinishStartTag(bool bNewline)
        {
            if(!m_bCurrentTagEnded)
            {
                if(bNewline)
                    m_objXMLWriter.Append(">\r\n");
                else
                {
                    m_objXMLWriter.Append(">");
                    m_bSingleLine = true;
                }

                m_bCurrentTagEnded = true;
            }
        }

        /****************************************************************************/
        public IDisposable Element(string strName)
        {
            return(new XmlElementWriter(this, strName));
        }

        /****************************************************************************/
        public void WriteStartElement(string strTag)
        {
            FinishStartTag();

            if(m_strRoot == "")
                m_strRoot = strTag;

            m_objXMLWriter.Append(m_strIndent + "<" + strTag);
            m_strIndent += "  ";
            m_bSingleLine = false;
            m_sTags.Push(strTag);

            m_bCurrentTagEnded = false;
        }

        /****************************************************************************/
        public void WriteEndElement()
        {
            m_strIndent = m_strIndent.Remove(0, 2);
            
            string strTag = (string)m_sTags.Pop();

            if(m_strRoot.ToLower() == "html" && (strTag.ToLower() == "link" || strTag.ToLower() == "script"))
                FinishStartTag();

            if(m_bCurrentTagEnded)
            {
                if(!m_bSingleLine)
                    m_objXMLWriter.Append(m_strIndent);

                m_objXMLWriter.Append("</" + strTag + ">\r\n");
            }
            else
                m_objXMLWriter.Append(" />\r\n");

            m_bCurrentTagEnded = true;
            m_bSingleLine = false;
        }

        /****************************************************************************/
        public static string Encode(string strValue)
        {
            return(strValue.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;"));
        }

        /****************************************************************************/
        public static string Decode(string strValue)
        {
            return(strValue.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&"));
        }

        /****************************************************************************/
        public static string EncodeAttribute(string strValue)
        {
            return(strValue.Replace("\"", "'").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;"));
        }

        /****************************************************************************/
        public static string EncodeXPath(string strValue)
        {
            return(strValue.Replace("\'", "&quote;"));
        }

        /****************************************************************************/
        public void WriteElementString(string strTag, string strValue, bool bEncode)
        {
            FinishStartTag();

            if(bEncode)
                strValue = Encode(strValue);

            m_objXMLWriter.Append(m_strIndent + "  " + "<" + strTag + ">" + strValue + "</" + strTag + ">\r\n");
        }

        /****************************************************************************/
        public void WriteElementString(string strTag, string strValue)
        {
            WriteElementString(strTag, strValue, false);
        }

        /****************************************************************************/
        public void WriteString(string strValue)
        {
            WriteString(strValue, false);
        }

        /****************************************************************************/
        public void WriteString(string strValue, bool bEncode)
        {
            FinishStartTag(false);

            if(bEncode)
                strValue = Encode(strValue);

            m_objXMLWriter.Append(strValue);
        }

        /****************************************************************************/
        public void WriteAttributeString(string strName, string strValue, bool bEncode)
        {
            try
            {
                if(strValue != "")
                {
                    if(bEncode)
                        strValue = EncodeAttribute(strValue);

                    _WriteAttributeString(strName, strValue);
                }
            }
            catch(Exception e)
            {
                throw new Exception("WriteAttributeString(" + strName + "):" + e.Message);
            }
        }

        /****************************************************************************/
        private void _WriteAttributeString(string strName, string strValue)
        {
            m_objXMLWriter.Append(" " + strName + "=\"" + strValue + "\"");
        }

        /****************************************************************************/
        public void WriteAttributeString(string strName, string strValue)
        {
            if(strValue != "")
                _WriteAttributeString(strName, strValue);
        }

        /****************************************************************************/
        public void WriteAttributeString(string strName, uint uValue)
        {
            _WriteAttributeString(strName, uValue.ToString());
        }

        /****************************************************************************/
        public void WriteAttributeString(string strName, ulong uValue)
        {
            _WriteAttributeString(strName, uValue.ToString());
        }

        /****************************************************************************/
        public void WriteAttributeString(string strName, double dValue)
        {
            _WriteAttributeString(strName, dValue.ToString());
        }

        /****************************************************************************/
        public void WriteAttributeString(string strName, int iValue)
        {
            _WriteAttributeString(strName, iValue.ToString());
        }

        /****************************************************************************/
        public void WriteAttributeString(string strName, long iValue)
        {
            _WriteAttributeString(strName, iValue.ToString());
        }

        /****************************************************************************/
        public void WriteAttributeString(string strName, decimal dValue)
        {
            _WriteAttributeString(strName, dValue.ToString());
        }

        /****************************************************************************/
        public void WriteAttributeString(string strName, bool bValue)
        {
            if(bValue)
                _WriteAttributeString(strName, "1");
        }

        /****************************************************************************/
        public void WriteAttributeString(string strName, Guid idValue)
        {
            _WriteAttributeString(strName, idValue.ToString());
        }

        /****************************************************************************/
        public void WriteElementString(string strTag, uint uValue)
        {
            WriteElementString(strTag, uValue.ToString(), false);
        }

        /****************************************************************************/
        public void WriteElementString(string strTag, int iValue)
        {
            WriteElementString(strTag, iValue.ToString(), false);
        }

        /****************************************************************************/
        public void WriteElementString(string strTag, ushort uValue)
        {
            WriteElementString(strTag, uValue.ToString(), false);
        }

        /****************************************************************************/
        public void WriteElementString(string strTag, double dValue)
        {
            WriteElementString(strTag, dValue.ToString(), false);
        }

        /****************************************************************************/
        public void WriteElementString(string strTag, Guid idValue)
        {
            WriteElementString(strTag, idValue.ToString(), false);
        }

        /****************************************************************************/
        public void WriteNode(XmlNode objNode)
        {
            WriteRaw(objNode.OuterXml);
        }

        /****************************************************************************/
        public void WriteElementCDATA(string strTag, string strText)
        {
            WriteStartElement(strTag);
            WriteCDATA(strText);
            WriteEndElement();
        }

        /****************************************************************************/
        public void WriteCDATA(string strText)
        {
            FinishStartTag(false);

            m_objXMLWriter.Append("<![CDATA[" + strText + "]]>");
        }

        /****************************************************************************/
        public void WriteRaw(string strXml)
        {
            FinishStartTag(true);

            m_objXMLWriter.Append(strXml);
        }

        /****************************************************************************/
        public void WriteElementString(string strTag, object objWrite)
        {
            WriteElementString(strTag, objWrite.ToString(), false);
        }

        /****************************************************************************/
        public void WriteElementString(string strTag, bool bValue)
        {
            WriteElementString(strTag, bValue ? "1" : "0" , false);
        }

        /****************************************************************************/
        public void WriteText(string strText)
        {
            WriteRaw(strText);
        }

        /****************************************************************************/
        public void WriteText(string strText, bool bEncode)
        {
            if(bEncode)
                strText = Encode(strText);

            WriteRaw(strText);
        }

        /****************************************************************************/
        public XmlDocument Xml
        {
            get
            {
                XmlDocument objXML = new XmlDocument();
                GetXML(objXML);

                return(objXML);
            }
        }

        /****************************************************************************/
        public XPathDocument XPath
        {
            get
            {
                using(StringReader s = new StringReader(ToString()))
                    return(new XPathDocument(s));
            }
        }

        /****************************************************************************/
        public void GetXML(XmlDocument objXML)
        {
            string strXML = ToString();

            try
            {
                objXML.LoadXml(strXML);
            }
            catch(Exception e)
            {
                throw new Exception(e.Message + "\r\n" + strXML);
            }
        }

        /****************************************************************************/
        public void ToFile(string strPath)
        {
            if(strPath.Length > 0)
                DiskFile.ToFile(ToString(), m_strPath);
        }

        /****************************************************************************/
        public override string ToString()
        {
            WriteEndDocument();

            return(m_objXMLWriter.ToString());
        }

        /****************************************************************************/
        public void WriteElements(StringCollection aElements, string strTagName)
        {
            if(aElements != null)
            {
                int nItems = aElements.Count / 2;

                if(nItems > 0)
                {
                    if(strTagName.Length > 0)
                        WriteStartElement(strTagName);

                    for(int i = 0; i < nItems; ++i)
                        WriteElementString(aElements[i*2], aElements[i*2+1]);

                    if(strTagName.Length > 0)
                        WriteEndElement();
                }
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class XmlElementWriter : IDisposable
    {
        private readonly cXMLWriter m_objWriter;

        /****************************************************************************/
        public XmlElementWriter(cXMLWriter objWriter, string strName)
        {
            objWriter.WriteStartElement(strName);
            m_objWriter = objWriter;
        }

        #region IDisposable Members

        public void Dispose()
        {
            m_objWriter.WriteEndElement();
        }

        #endregion
    }

}
