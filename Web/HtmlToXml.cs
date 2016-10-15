/****************************************************************************/
/*                                                                          */
/*    Mondo Libraries  							                            */
/*                                                                          */
/*        Namespace: Mondo.Web       							            */
/*             File: HtmlToXml.cs								            */
/*        Class(es): HtmlToXml				                                */
/*          Purpose: Cleans-up html and converts it to xhtml (xml)          */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 8 Nov 2008                                             */
/*                                                                          */
/*   Copyright (c) 2008 - Jim Lightfoot, All rights reserved                */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Mondo.Common;

namespace Mondo.Web
{
    /*********************************************************************/
    /*********************************************************************/
    public class HtmlToXml 
    {
        private const string    kSpaces           = "                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        ";
        private const int       kTabWidth         = 2;
        private int             m_iIndex          = 0;
        private Source          m_strInput        = null;
        private bool            m_bRemoveDocTypes = false;
        private StringBuilder   m_strCurrentLine  = null;
        private Stack<Element>  m_aElements       = null;

        /*********************************************************************/
        public HtmlToXml()
        {
        }

        /*********************************************************************/
        public bool RemoveEmpties
        {
            get;
            private set;
        }
    
        /*********************************************************************/
        public string NewLine
        {
            get
            {
                return(this.WriteNewLines ? "\r\n" : "");
            }
        }
    
        /**************************************************************/
        public void WriteIndentation(StringBuilder sb, int iLevel)
        {
            if(this.WriteNewLines)
                sb.Append(kSpaces.Substring(0, iLevel * kTabWidth));
        }

        /*********************************************************************/
        public bool WriteNewLines
        {
            get;
            private set;
        }
    
        /*********************************************************************/
        public virtual bool Write(HtmlToXml objParser, Node objNode, Element objParent, StringBuilder sb, int iLevel)
        {
            if(objNode is Element)
                return(WriteElement(objParser, objNode as Element, objParent, sb, iLevel));

            return(objNode.Write(objParser, sb, iLevel));
        }

        /*********************************************************************/
        protected virtual bool WriteElement(HtmlToXml objParser, Element objElement, Element objParent, StringBuilder sb, int iLevel)
        {
            switch(Allowed(objElement, objParent))
            {
                case ElementAction.Remove:
                    return(false);

                case ElementAction.Allowed:
                    return(objElement.Write(objParser, sb, iLevel));

                case ElementAction.Convert:
                    // ??? Not yet implemented
                    return(false);

                case ElementAction.PromoteChildren:
                default:
                    break;
            }

            bool bEmpty = true;

            foreach(Node objNode in objElement.Children)
                if(this.Write(objParser, objNode, objParent, sb, iLevel+1))
                    bEmpty = false;

            return(!bEmpty);
        }

        /*********************************************************************/
        protected virtual ElementAction Allowed(Attribute objAttribute, Element objParent)
        {
            return(ElementAction.Allowed);
        }

        /*********************************************************************/
        protected virtual ElementAction Allowed(Element objElement, Element objParent)
        {
            if(objParent == null)
                return(ElementAction.Allowed);

            if(objElement.Name == "__" || objElement.Name == "")
                return(ElementAction.PromoteChildren);

            if(objElement.Name == "ul" && objParent.Name == "ul")
                return(ElementAction.PromoteChildren);

            if(objElement.Name == "ol" && objParent.Name == "ol")
                return(ElementAction.PromoteChildren);

            if(objElement.Name == "li" && objParent.Name == "li")
                return(ElementAction.PromoteChildren);

            switch(objParent.Name)
            {
                case "table":
                {
                    switch(objElement.Name)
                    {
                        case "thead":
                        case "tbody":
                        case "tr":
                            break;

                        default:
                            return(ElementAction.Remove);
                    }

                    break;
                }

                case "tbody":
                case "thead":
                {
                    switch(objElement.Name)
                    {
                        case "tr":
                            break;

                        default:
                            return(ElementAction.Remove);
                    }

                    break;
                }

                case "tr":
                {
                    switch(objElement.Name)
                    {
                        case "th":
                        case "td":
                            break;

                        default:
                            return(ElementAction.Remove);
                    }

                    break;
                }

                case "ol":
                case "ul":
                {
                    switch(objElement.Name)
                    {
                        case "li":
                            break;

                        default:
                            return(ElementAction.Remove);
                    }

                    break;
                }

                default:
                    break;
            }

            return(ElementAction.Allowed);
        }

        /*********************************************************************/
        public enum ElementAction
        {
            Allowed = 1,
            Remove = 2,
            PromoteChildren = 3,
            Convert = 4
        }

        /*********************************************************************/
        private static readonly Dictionary<string, string> s_EmptiesAllowed = new Dictionary<string,string>
        {
            {"html",    ""},
            {"body",    ""},
            {"script",  ""},
            {"meta",    ""},
            {"link",    ""},
            {"input",   ""},
            {"button",  ""},
            {"a",       ""},
            {"hr",      ""},
            {"img",     ""},
            {"br",      ""}
        };

        /*********************************************************************/
        public virtual bool EmptiesAllowed(Element objElement)
        {
            return(s_EmptiesAllowed.ContainsKey(objElement.Name));
        }

        #region Child Classes

        /*********************************************************************/
        /*********************************************************************/
        public abstract class Node
        {
            private Element        m_objParent;

            /*********************************************************************/
            public Node()
            {
            }

            /*********************************************************************/
            public Element Parent
            {
                get {return(m_objParent);}
                set {m_objParent = value;}
            }

            /*********************************************************************/
            public abstract bool Write(HtmlToXml objParser, StringBuilder sb, int iLevel);
        }

        /*********************************************************************/
        /*********************************************************************/
        public class TextNode : Node
        {
            protected string m_strText;

            /*********************************************************************/
            public TextNode(string strText) 
            {
                m_strText = Normalize(strText).Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&amp;", "&");
            }

            /*********************************************************************/
            public override string ToString()
            {
                return(m_strText);
            }

            /*********************************************************************/
            public override bool Write(HtmlToXml objParser, StringBuilder sb, int iLevel)
            {
                objParser.WriteIndentation(sb, iLevel);

                // Remove duplicate spaces and replace newlines and tabs with spaces. Encode < and > and &
                string strText = m_strText;

                // Encode < and > and &
                strText = strText.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Trim();

                if(strText != "")
                { 
                    sb.Append(strText);
                    return(true);
                }

                return(false);
            }

            /*********************************************************************/
            public static string Normalize(string strText)
            {
                strText = strText.Replace("\t", " ").Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Replace("&nbsp;", " ").Replace("“", "\"").Replace("”", "\"").Replace("‘", "'").Replace("’", "'");                
                
                strText = strText.NormalizeSpace();

                return(strText);
            }
        }

        /*********************************************************************/
        /*********************************************************************/
        public class Literal : TextNode
        {
            /*********************************************************************/
            public Literal(string strText) : base(strText)
            {
            }
  
            /*********************************************************************/
            public override bool Write(HtmlToXml objParser, StringBuilder sb, int iLevel)
            {
                objParser.WriteIndentation(sb, iLevel);

                sb.Append(m_strText + objParser.NewLine);

                return(true);
            }
        }

        /*********************************************************************/
        /*********************************************************************/
        public class Attribute 
        {
            private string m_strName = "";
            private string m_strValue;

            /*********************************************************************/
            public Attribute(string strName, string strValue)
            {
                m_strName  = HtmlToXml.FormatName(strName, true);
                m_strValue = strValue;
            }

            /*********************************************************************/
            public string Name    {get{return(m_strName);}    set{m_strName = value;}}
            public string Value   {get{return(m_strValue);}   set{m_strValue = value;}}
        }

        /*********************************************************************/
        public static string FormatName(string strName, bool bAttribute)
        {
            string strNewName = "";

            foreach(char chCheck in strName)
            { 
                if(char.IsLetter(chCheck))
                    strNewName += chCheck;
                else if(char.IsDigit(chCheck) && strNewName == "")
                    strNewName += chCheck;
                else if(chCheck == '_' && (strNewName == "" || bAttribute))
                    strNewName += chCheck;
                else if(chCheck == '-' && strNewName == "")
                    strNewName += chCheck;
            }

            return(strNewName);
        }

        /*********************************************************************/
        /*********************************************************************/
        public class Element : Node
        {
            private string          m_strName;
            private List<Node>      m_aChildren     = new List<Node>();
            private List<Attribute> m_aAttributes   = new List<Attribute>();
            private HtmlToXml      m_objParser;

            /*********************************************************************/
            public Element(HtmlToXml objParser, string strName)
            {
                m_strName = HtmlToXml.FormatName(strName, false);

                if(m_strName != "__" && m_strName == "")
                    throw new Exception("Illegal Element name: \"" + strName + "\"");

                m_strName = strName;
                m_objParser = objParser;
            }

            /*********************************************************************/
            public string Name
            {
                get {return(m_strName);}
                set {m_strName = value;}
            }

            /*********************************************************************/
            public List<Node> Children
            {
                get {return(m_aChildren);}
            }

            /*********************************************************************/
            public List<Attribute> Attributes
            {
                get {return(m_aAttributes);}
            }

            /*********************************************************************/
            private enum AttributeState
            {
                Nothing     = 0,
                Name        = 1,
                AfterName   = 2,
                StartValue  = 3,
                InValue     = 4
            }

            /*********************************************************************/
            private void AddAttribute(string strName, string strValue)
            {
                if(strName != "" && strValue != "")
                    m_aAttributes.Add(new Attribute(strName.ToLower(), strValue));
            }

            /*********************************************************************/
            public void Clean(HtmlToXml objParser)
            {            
                while(this.Children.Count > 0)
                {
                    Node objNode = this.Children[0];

                    // Remove text nodes at the beginning that are empty
                    if((objNode is TextNode) && (objNode as TextNode).ToString().Trim() == "")
                        this.Children.RemoveAt(0);
                    else
                        break;
                }

                while(this.Children.Count > 0)
                {
                    Node objNode = this.Children[this.Children.Count - 1];

                    // Remove text nodes at the end that are empty
                    if((objNode is TextNode) && (objNode as TextNode).ToString().Trim() == "")
                        this.Children.RemoveAt(this.Children.Count - 1);
                    else
                        break;
                }
            }

            /*********************************************************************/
            public void ProcessAttributes(string strAttributes)
            {
                strAttributes = TextNode.Normalize(strAttributes).Trim();

                if(strAttributes == "")
                    return;

                string         strName      = "";
                string         strValue     = "";
                bool           bHaveQuote   = false;
                bool           bSingleQuote = false;
                AttributeState eState       = AttributeState.Nothing;

                foreach(char chFind in strAttributes)
                {
                    switch(chFind)
                    {
                        case ' ':
                        {
                            switch(eState)
                            {
                                // Found whitespace
                                case AttributeState.Name:
                                    eState = AttributeState.AfterName;
                                    break;

                                case AttributeState.InValue:
                                {
                                    if(bHaveQuote)
                                        strValue += " ";
                                    else
                                    {
                                        AddAttribute(strName, strValue);
                                        strName = "";
                                        strValue = "";
                                        eState = AttributeState.Nothing;
                                    }

                                    break;
                                }

                                // Ignore white space here
                                case AttributeState.Nothing:
                                case AttributeState.StartValue:
                                case AttributeState.AfterName:
                                default:
                                    break;
                            }

                            break;
                        }

                        case '=':
                        {
                            switch(eState)
                            {
                                case AttributeState.Name:
                                case AttributeState.AfterName:
                                    eState = AttributeState.StartValue;
                                    break;

                                case AttributeState.InValue:
                                {
                                    if(bHaveQuote)
                                        strValue += "+";

                                    break;
                                }

                                default:
                                    // ??? this is bad??? Ignore for now!
                                    break;
                            }

                            break;
                        }

                        case '\'':
                        {
                            switch(eState)
                            {
                                case AttributeState.StartValue:
                                    eState = AttributeState.InValue;
                                    bHaveQuote = true;
                                    bSingleQuote = true;
                                    break;

                                case AttributeState.InValue:
                                {
                                    if(bHaveQuote && !bSingleQuote)
                                        strValue += "\'";
                                    else if(bHaveQuote && bSingleQuote)
                                    {
                                        bSingleQuote = false;
                                        bHaveQuote = false;
                                        AddAttribute(strName, strValue);
                                        strName = "";
                                        strValue = "";
                                        eState = AttributeState.Nothing;
                                    }

                                    break;
                                }

                                default:
                                    // ??? this is bad??? Ignore for now!
                                    break;
                            }

                            break;
                        }

                        case '\"':
                        {
                            switch(eState)
                            {
                                case AttributeState.StartValue:
                                    eState = AttributeState.InValue;
                                    bHaveQuote = true;
                                    bSingleQuote = false;
                                    break;

                                case AttributeState.InValue:
                                {
                                    if(bHaveQuote && bSingleQuote)
                                        strValue += "\"";
                                    else if(bHaveQuote && !bSingleQuote)
                                    {
                                        bSingleQuote = false;
                                        bHaveQuote = false;
                                        AddAttribute(strName, strValue);
                                        strName = "";
                                        strValue = "";
                                        eState = AttributeState.Nothing;
                                    }

                                    break;
                                }

                                default:
                                    // ??? this is bad??? Ignore for now!
                                    break;
                            }

                            break;
                        }

                        default:
                        {
                            switch(eState)
                            {
                                case AttributeState.AfterName:
                                {
                                    AddAttribute(strName, strName.ToLower());
                                    strName = "";
                                    strValue = "";
                                    eState = AttributeState.Nothing;
                                    break;
                                }

                                case AttributeState.StartValue:
                                    strValue = chFind.ToString();
                                    eState = AttributeState.InValue;
                                    break;

                                case AttributeState.Nothing:
                                    strName += chFind;
                                    eState = AttributeState.Name;
                                    break;

                                case AttributeState.Name:
                                    strName += chFind;
                                    break;

                                case AttributeState.InValue:
                                    strValue += chFind;
                                    break;

                                default:
                                    break;
                            }

                            break;
                        }
                    }
                }

                AddAttribute(strName, strValue);
                return;
            }

            /*********************************************************************/
            private string PackAttributes()
            {
                const string kSeparator = "_$GTF6()sx2";

                if(m_aAttributes.Count > 0)
                {
                    StringList aAttributes = new StringList();

                    foreach(Attribute objAttribute in m_aAttributes)
                    {
                        // Name may be blank if all invalid characters, just ignore those
                        if(objAttribute.Name != "")
                        { 
                            if(m_objParser.Allowed(objAttribute, this) == ElementAction.Allowed)
                            { 
                                string strValue = objAttribute.Value.Replace("\"", "&quot;").Replace("&amp;", kSeparator).Replace("&", "&amp;").Replace(kSeparator, "&amp;");
                            
                                aAttributes.Add(string.Format("{0}=\"{1}\"", objAttribute.Name, strValue));
                            }
                        }
                    }

                    string strAttributes = aAttributes.Pack(" ");

                    if(strAttributes != "")
                        strAttributes = " " + strAttributes;

                    return(strAttributes);
                }

                return("");
            }

            /*********************************************************************/
            public override bool Write(HtmlToXml objParser, StringBuilder sb, int iLevel)
            {
                bool bEmpty = true;

                string strAttributes = PackAttributes();

                if(strAttributes != "")
                    bEmpty = false;

                if(m_aChildren.Count > 0)
                {
                    string strBegin = "";

                    if(this.Name != "__")
                        strBegin = string.Format("<{0}{1}>" + m_objParser.NewLine, this.Name, strAttributes);

                    StringBuilder sbChildren = new StringBuilder();

                    foreach(Node objNode in this.Children)
                        if(m_objParser.Write(objParser, objNode, objNode.Parent, sbChildren, iLevel+1))
                            bEmpty = false;

                    if(m_objParser.RemoveEmpties && bEmpty && !m_objParser.EmptiesAllowed(this))
                        return(false);

                    if(this.Name != "__")
                    { 
                        m_objParser.WriteIndentation(sb, iLevel);
                        sb.Append(" " + strBegin);
                        sb.Append(sbChildren.ToString());
                        m_objParser.WriteIndentation(sb, iLevel);
                        sb.AppendFormat("</{0}> " + m_objParser.NewLine, this.Name);  
                    }
                    else
                        sb.Append(" " + sbChildren.ToString() + " ");
                }
                else
                {
                    if(m_objParser.RemoveEmpties && bEmpty && !m_objParser.EmptiesAllowed(this))
                        return(false);

                    m_objParser.WriteIndentation(sb, iLevel);

                    sb.AppendFormat(" <{0}{1} /> " + m_objParser.NewLine, this.Name, strAttributes);
                }

                return(!bEmpty);
            }
        }

        #endregion

        /*********************************************************************/
        public int Index  {get{return(m_iIndex);}}
        
        /*********************************************************************/
        public string Process(string strInput)
        {
            return(Process(strInput, true, false));
        }

        /*********************************************************************/
        protected virtual string PreProcess(string strInput)
        {
            return(strInput);
        }

        /*********************************************************************/
        /*********************************************************************/
        private class Source
        {
            private readonly string m_strSource;
            private readonly int m_iLength;

            /*********************************************************************/
            internal Source(string strSource)
            {
                m_strSource = strSource;
                m_iLength = strSource.Length;
            }

            /*********************************************************************/
            internal int Length
            {
                get { return(m_iLength); }
            }

            /*********************************************************************/
            internal int IndexOf(string strValue)
            {
                return(m_strSource.IndexOf(strValue));
            }

            /*********************************************************************/
            internal int IndexOf(string strValue, int start)
            {
                return(m_strSource.IndexOf(strValue, start));
            }

            /*********************************************************************/
            internal string Substring(int start, int length)
            {
                return(m_strSource.Substring(start, length));
            }

            /****************************************************************************/
            public static implicit operator string(Source objSource)            
            {
                return(objSource.ToString());
            }

            /*********************************************************************/
            internal char this[int iIndex]
            {
                get
                {
                    return(m_strSource[iIndex]);
                }
            }

            /*********************************************************************/
            public override string ToString()
            {
                return(m_strSource);
            }

            /**************************************************************/
            internal bool CompareNext(int iIndex, string strValue)
            {
                if(iIndex + strValue.Length >= this.Length)
                    return(false);

                return(m_strSource.Substring(iIndex, strValue.Length) == strValue);
            }
        }

        /**************************************************************/
        private bool CompareNext(string strValue)
        {
            return(m_strInput.CompareNext(m_iIndex, strValue));
        }

        /*********************************************************************/
        public string Process(string strInput, bool bRemoveDocTypes, bool bRemoveEmpties, bool bWriteNewLines = true)
        {
            m_strCurrentLine    = new StringBuilder();
            m_aElements         = new Stack<Element>();
            m_iIndex            = 0;
            m_strInput          = new Source(PreProcess(strInput));
            m_bRemoveDocTypes   = bRemoveDocTypes;
            
            this.RemoveEmpties  = bRemoveEmpties;
            this.WriteNewLines  = bWriteNewLines;

            Element objRoot = new Element(this, "__");
            m_aElements.Push(objRoot);

            while(m_iIndex < m_strInput.Length)
                InternalProcess();

            objRoot.Clean(this);

            StringBuilder objOutput = new StringBuilder();

            objRoot.Write(this, objOutput, 0);

            objOutput.AppendLine("");

            return(objOutput.ToString().Trim());
        }

        /*********************************************************************/
        private void InternalProcess()
        {
            if(ProcessBeginEnd("<!--", "-->"))
                return;

            if(ProcessBeginEnd("<?", "?>"))
                return;

            if(ProcessBeginEnd("<!", ">", m_bRemoveDocTypes))
                return;

            if(m_iIndex >= m_strInput.Length)
              return;

            if(CompareNext("</"))
            {
                ProcessEndElement();
                return;
            }

            if(m_iIndex >= m_strInput.Length)
              return;

            if(m_strInput[m_iIndex] == '<')
            {
                ProcessStartElement();
                return;
            }

            m_strCurrentLine.Append(m_strInput[m_iIndex++]);

            return;
        }

        /**************************************************************/
        private void ProcessText(bool bStart)
        {
            string strText = m_strCurrentLine.ToString();

            m_strCurrentLine = new StringBuilder();

            if(strText == "")
                return;

            TextNode objNode = new TextNode(strText);

            if(objNode.ToString() != "")
                AddToParent(objNode);
        }

        /**************************************************************/
        private void ProcessEndElement()
        {
            ProcessText(false);

            int iEnd = m_strInput.IndexOf(">", m_iIndex + 2);

            if(iEnd == -1)
            {
                // This is bad. Just stop processing.
                m_iIndex = int.MaxValue;
                return;
            }

            string strName = m_strInput.Substring(m_iIndex + 2, iEnd - m_iIndex - 2).Trim().ToLower();

            while(m_aElements.Count > 1 && m_aElements.Peek().Name != strName)
            {
                m_aElements.Pop();
            }

            if(m_aElements.Count > 1)
                m_aElements.Pop();

            m_iIndex = iEnd + 1;

            return;
        }

        /**************************************************************/
        private void ProcessStartElement()
        {
            int iEnd = m_strInput.IndexOf(">", m_iIndex + 1);

            ProcessText(true);

            if(iEnd == -1)
            {
                // This is bad. Just stop processing.
                m_iIndex = int.MaxValue;
                return;
            }

            string strElement  = m_strInput.Substring(m_iIndex + 1, iEnd - m_iIndex - 1);
            bool   bClosed     = false;

            if(strElement.EndsWith("/"))
            {
                bClosed = true;
                strElement = strElement.Substring(0, strElement.Length-1);
            }

            int    iWhiteSpace   = strElement.FindWhiteSpace(0, true);
            string strName       = "";
            string strAttributes = "";

            if(iWhiteSpace == -1)
                strName = strElement.ToLower();
            else
            {
                strName       = strElement.Substring(0, iWhiteSpace).ToLower();
                strAttributes = strElement.Substring(strName.Length).Trim();
            }

            if(m_aElements.Peek().Name == "p")
            {
                switch(strName)
                {
                    case "div":
                    case "hr":
                        BackupCurrentElement();
                        break;

                    default:
                        break;
                }
            }

            if(!bClosed)
            {
                switch(strName)
                {
                    case "br":
                    case "hr":
                        bClosed = true;
                        break;

                    default:
                        break;
                }
            }

            Element objElement = new Element(this, strName);

            objElement.ProcessAttributes(strAttributes);

            AddToParent(objElement);

            if(!bClosed)
                m_aElements.Push(objElement);

            m_iIndex = iEnd + 1;

            return;
        }

        /**************************************************************
         * Closes the current element and moves all it's children up to it's parent 
         **************************************************************/
        private void BackupCurrentElement()
        {
            Element objCurrent = m_aElements.Pop();
            Element objParent  = m_aElements.Peek();

            foreach(Node objNode in objCurrent.Children)
                objParent.Children.Add(objNode);

            objCurrent.Children.Clear();

            return;
        }

        /**************************************************************/
        private void AddToParent(Node objNode)
        {
            Element objParent = m_aElements.Peek();

            objParent.Children.Add(objNode);
            objNode.Parent = objParent;
        }

        /**************************************************************/
        private bool ProcessBeginEnd(string strStart, string strEnd)
        {
            return(ProcessBeginEnd(strStart, strEnd, false));
        }

        /**************************************************************/
        private bool ProcessBeginEnd(string strStart, string strEnd, bool bRemove)
        {
            if(CompareNext(strStart))
            {
                int iEnd = m_strInput.IndexOf(strEnd, m_iIndex + strStart.Length);

                if(iEnd == -1)
                {
                    if(!bRemove)
                        AddToParent(new Literal(m_strInput.Substring(m_iIndex, 0) + strEnd));

                    m_iIndex = int.MaxValue;
                }
                else
                {
                    if(!bRemove)
                        AddToParent(new Literal(m_strInput.Substring(m_iIndex, iEnd - m_iIndex + strEnd.Length)));

                    m_iIndex = iEnd + strEnd.Length;
                }

                return(true);
            }

            return(false);
        }
    }
}
