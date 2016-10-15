/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common.Query							            */
/*             File: Query.cs					    		                */
/*        Class(es): Query, Condition				         		        */
/*          Purpose: A query                                                */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 28 Nov 2003                                            */
/*                                                                          */
/*   Copyright (c) 2003-2008 - Tenth Generation Software, LLC               */
/*                          All rights reserved                             */
/*                                                                          */
/****************************************************************************/

using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using Mondo.Common;
using Mondo.Xml;

namespace Mondo.Common.Query
{
    /*********************************************************************/
    /*********************************************************************/
    public class ComplexBool : Choice
    {
        const string kAND = "     and";
        const string kOR  = "     or";

        /*********************************************************************/
        public ComplexBool(bool bAND)
        {
            if(bAND)
            {
                this.Id = "and";
                this.Caption = kAND;
            }
            else
            {
                this.Id = "or";
                this.Caption = kOR;
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// A single condition for a query, i.e. [name] = 'Jim'
    /// </summary>
    public class Condition : Model
    {
        private   object m_objLeft  = null;
        private   object m_objOp    = "eq";
        private   object m_objRight = null;
        protected Query  m_objQuery;

        private static ChoiceList s_aOperators;
        private static ChoiceList s_aChoiceListOperators;
        private static ChoiceList s_aPrivateListOperators;
        private static Hashtable  s_aUnits;

        /****************************************************************************/
        static Condition()
        {
            s_aOperators            = new OperatorList();
            s_aChoiceListOperators  = new ChoiceListOperatorList();
            s_aPrivateListOperators = new _PrivateListOperators();
            s_aUnits                = new Hashtable(5);

            s_aUnits.Add("mn", "Minutes");  // ??? Language
            s_aUnits.Add("hr", "Hours");
            s_aUnits.Add("dy", "Days");
            s_aUnits.Add("wk", "Weeks");
            s_aUnits.Add("mo", "Months");
        }

        /****************************************************************************/
        private class OperatorList : ChoiceList
        {
            /****************************************************************************/
            internal OperatorList() 
            {
                Add(new Choice("eq",     "equals")); // ??? resource
                Add(new Choice("ne",     "not equal to"));
                Add(new Choice("gt",     "greater than"));
                Add(new Choice("ge",     "greater than or equal to"));
                Add(new Choice("lt",     "less than"));
                Add(new Choice("le",     "less than or equal to"));
                Add(new Choice("co",     "contains"));
                Add(new Choice("bt",     "between"));
                Add(new Choice("oc",     "occurred within"));
                Add(new Choice("wi",     "will occur within"));
                Add(new Choice("bef",    "before"));
                Add(new Choice("aft",    "after"));
                Add(new Choice("is",     "is"));
                Add(new Choice("isnt",   "is not"));
                Add(new Choice("isin",   "is in"));
                Add(new Choice("isntin", "is not in"));
                Add(new Choice("and",    "and"));
                Add(new Choice("or",     "or"));
                Add(new Choice("starts", "starts with"));
                Add(new Choice("end",    "ends with"));
            }
        }

        /****************************************************************************/
        private class ChoiceListOperatorList : ChoiceList
        {
            /****************************************************************************/
            internal ChoiceListOperatorList() 
            {
                Add(new Choice("is", "is"));
                Add(new Choice("isnt", "isn't"));
            }
        }

        /****************************************************************************/
        private class _PrivateListOperators : ChoiceList
        {
            /****************************************************************************/
            internal _PrivateListOperators() 
            {
                Add(new Choice("eq", "count equals"));
                Add(new Choice("ne", "count not equal to"));
                Add(new Choice("gt", "count greater than"));
                Add(new Choice("ge", "count greater than or equal to"));
                Add(new Choice("lt", "count less than"));
                Add(new Choice("le", "count less than or equal to"));
            }
        }

        /****************************************************************************/
        protected Condition(Query objQuery)
        {
            m_objQuery = objQuery;
        }

        /****************************************************************************/
        protected Condition()
        {
            m_objQuery = null;
        }

        /****************************************************************************/
        public Condition(Query objQuery, object objLeft, object objOp, object objRight)
        {
            m_objQuery = objQuery;
            m_objLeft  = objLeft;
            this.Op    = objOp;
            m_objRight = objRight;
        }

        /****************************************************************************/
        public Condition(Query objQuery, XmlElement xmlCondition)
        {
            m_objQuery = objQuery;
            LoadXml(xmlCondition);
        }

        /****************************************************************************/
        public override void Load(XmlNode xmlData)
        {
            base.Load(xmlData);

            this.Op    = xmlData.GetAttribute("op");
            m_objLeft  = xmlData.GetAttribute("prop");
            m_objRight = xmlData.GetAttribute("value");

            XmlNodeList aConditions = xmlData.SelectNodes("condition");

            foreach(XmlElement xmlCondition in aConditions)
                LoadCondition(xmlCondition);
        }

        /****************************************************************************/
        private void LoadCondition(XmlElement xmlCondition)
        {
            Condition objCondition = new Condition(m_objQuery);

            objCondition.Load(xmlCondition);

            if(m_objLeft == null)
                m_objLeft = objCondition;
            else
                m_objRight = objCondition;
        }

        /****************************************************************************/
        protected void LoadXml(XmlElement xmlCondition)
        {
            if(xmlCondition != null)
            {
                this.Op = xmlCondition.GetAttribute("op");

                if(xmlCondition.HasAttribute("prop")) 
                {
                    m_objLeft   = xmlCondition.GetAttribute("prop");
                    
                    if(xmlCondition.HasAttribute("value"))
                    {
                        if(xmlCondition.HasAttribute("display"))
                        {
                            Choice objChoice = new Choice(xmlCondition.GetAttribute("value"), xmlCondition.GetAttribute("display"));

                            m_objRight = objChoice;
                        }
                        else
                            m_objRight  = xmlCondition.GetAttribute("value");
                    }
                    else                    
                    {
                        if(xmlCondition.HasAttribute("display"))
                        {
                            Choice objChoice = new Choice(xmlCondition.InnerText.Trim(), xmlCondition.GetAttribute("display"));

                            m_objRight = objChoice;
                        }
                        else
                            m_objRight  = xmlCondition.InnerText.Trim();
                    }
                }
                else
                {
                    XmlNodeList aConditions = xmlCondition.SelectNodes("condition");

                    if(aConditions.Count == 2)
                    {
                        m_objLeft   = new Condition(m_objQuery, (XmlElement)aConditions[0]);
                        m_objRight  = new Condition(m_objQuery, (XmlElement)aConditions[1]);
                    }
                }
            }
        }

        /****************************************************************************/
        public object Left      {get{return(m_objLeft);}    set{m_objLeft = value;  Modified = true;}}
        public object Right     {get{return(m_objRight);}   set{m_objRight = value; Modified = true;}}

        /****************************************************************************/
        public object Op        
        {
            get
            {
                return(m_objOp);
            }      
            
            set
            {
                m_objOp = value;  
                Modified = true;

                if(m_objOp is string)
                {
                    if(m_objOp.ToString() == "and")
                        m_objOp = new ComplexBool(true);
                    else if(m_objOp.ToString() == "or")
                        m_objOp = new ComplexBool(false);
                }
            }
        }

        /****************************************************************************/
        public void RemoveLeft()
        {
            if(!Promote(this.Right))
            {
                this.Left = this.Right;
                this.Right = "";
            }
        }

        /****************************************************************************/
        public void RemoveRight()
        {
            if(!Promote(this.Left))
                this.Right = "";
        }

        /****************************************************************************/
        private bool Promote(object objToPromote)
        {
            if(objToPromote is Condition)
            {
                Condition objPromote = objToPromote as Condition;

                this.Left  = objPromote.Left;
                this.Op    = objPromote.Op;
                this.Right = objPromote.Right;

                return(true);
            }

            return(false);
        }

        /****************************************************************************/
        public void AddCondition(XmlElement xmlCondition)
        {
            AddCondition(new Condition(m_objQuery, xmlCondition));
        }

        /****************************************************************************/
        public void AddCondition(object objLeft, object objOp, object objRight)
        {
            if(Left == null)
            {
                Left  = objLeft;
                Op    = objOp;
                Right = objRight;
            }
            else
            {
                Left  = new Condition(m_objQuery, this.Left, this.Op, this.Right);
                Op    = "and";
                Right = new Condition(m_objQuery, objLeft, objOp, objRight);
            }
        }

        /****************************************************************************/
        public void AddCondition(Condition objCondition)
        {
            AddCondition(objCondition.Left, objCondition.Op, objCondition.Right);
        }

        /****************************************************************************/
        protected virtual string GetLeftCaption(object objLeft)
        {
            if(m_objQuery != null)
                return(m_objQuery.GetLeftCaption(objLeft));

            return(Choice.GetCaption(objLeft));
        }

        /****************************************************************************/
        protected virtual string GetRightCaption(object objLeft, object objRight)
        {
            if(m_objQuery != null)
                return(m_objQuery.GetRightCaption(objLeft, objRight));

            return(Choice.GetCaption(objRight));
        }

        /****************************************************************************/
        public override string ToString()
        {
            try
            {
                string strOp = Choice.GetId(m_objOp);

                switch(strOp)
                {
                    case "":
                        return("");

                    case "bt":
                    {
                        StringList aParts   = new StringList(this.Right.ToString(), "///", false);
                        string     dtLeft   = aParts[0] == "" ? "" : m_objQuery.FormatValue(m_objLeft, this.Op, aParts[0]);
                        string     dtRight  = aParts[1] == "" ? "" : m_objQuery.FormatValue(m_objLeft, this.Op, aParts[1]);

                        return(string.Format("[{0}] between \"{1}\" and \"{2}\"", GetLeftCaption(m_objLeft), dtLeft, dtRight));
                    }

                    case "bef":
                    case "aft":
                        return("[" + GetLeftCaption(m_objLeft) + "] " + GetOperatorCaption(strOp) + " '" + m_objQuery.FormatValue(this.Left, this.Op, m_objRight) + "'");

                    case "oc":
                    case "wi":
                    {
                        StringList aParts   = new StringList(this.Right.ToString(), "///", false);
                        long       iCount   = Utility.ToLong(aParts[0]);
                        string     strUnits = aParts[1];
                        string     strUnitName = "";

                        try
                        {
                            strUnitName = s_aUnits[strUnits].ToString();
                        }
                        catch
                        {
                        }

                        return(string.Format("[{0}] {1} {2} {3}", GetLeftCaption(m_objLeft), strOp == "oc" ? "occurred within" : "will occur within", iCount, strUnitName));
                    }

                    case "and":
                    case "or":
                        return("(" + GetLeftCaption(m_objLeft) + ") " + GetOperatorCaption(strOp) + " (" + GetRightCaption(m_objLeft, m_objRight) + ")");

                    default:
                        return("[" + GetLeftCaption(m_objLeft) + "] " + GetOperatorCaption(strOp) + " '" + GetRightCaption(m_objLeft, m_objRight) + "'");
                }
            }
            catch(Exception ex)
            {
                cDebug.Capture(ex);
                return("");
            }
        }

        /****************************************************************************/
        public static string GetOperatorCaption(string strOperator)
        {
            return(s_aOperators[strOperator].ToString());
        }

        /****************************************************************************/
        public static ChoiceList Operators             {get {return(s_aOperators);}}
        public static ChoiceList ChoiceListOperators   {get {return(s_aChoiceListOperators);}}
        public static ChoiceList PrivateListOperators  {get {return(s_aPrivateListOperators);}}
        
        public string Occurs        {get{return(Choice.GetId(this.Op));} set{Op = value;}}
        public string FromDate      {get{return(GetSeparatedValue(0));}  set{SetSeparatedValue(value, 0);}}
        public string ToDate        {get{return(GetSeparatedValue(1));}  set{SetSeparatedValue(value, 1);}}
        public string WithinAmount  {get{return(GetSeparatedValue(0));}  set{SetSeparatedValue(value, 0);}}
        public string Units         {get{return(GetSeparatedValue(1));}  set{SetSeparatedValue(value, 1);}}

        /****************************************************************************/
        private void SetSeparatedValue(string strValue, int iIndex)
        {
            string strOp = Choice.GetId(this.Op);

            if(strOp == "bt" || strOp == "wi" || strOp == "oc")
            {
                List<string> aParts    = StringList.ParseString(GetValue(), "///", false);
                int          nParts    = aParts.Count;
                string       strFirst  = iIndex == 0 ? strValue : (nParts > 0 ? aParts[0] : "");
                string       strSecond = iIndex == 1 ? strValue : (nParts > 1 ? aParts[1] : "");

                SetValue(strFirst + "///" + strSecond);
            }
        }

        /****************************************************************************/
        private string GetSeparatedValue(int iIndex)
        {
            return(new StringList(GetValue(), "///", false)[iIndex]);
        }

        /****************************************************************************/
        public void SetPropertyId(object objNew)
        {
            m_objLeft = objNew;
        }

        /****************************************************************************/
        public void SetValue(object objValue)
        {
            m_objRight = objValue;
        }

        /****************************************************************************/
        public string GetPropertyId()
        {
            try
            {
                if(this.Left is Choice)
                {
                    Choice objChoice = this.Left as Choice;

                    return(objChoice.Id);
                }
            }
            catch(Exception)
            {
            }

            return(this.Left.ToString());
        }

        /****************************************************************************/
        public string GetValue()
        {
            try
            {
                if(Right is Choice)
                {
                    Choice objChoice = (Choice)Right;

                    return(objChoice.Id);
                }
            }
            catch(Exception)
            {
            }

            return(Right.ToString());
        }

        /****************************************************************************/
        public void WriteElements(cXMLWriter objWriter)
        {
            if(Left != null)
            {
                objWriter.WriteElementString("op", Op);

                try
                {
                    Condition objLeft = (Condition)Left;

                    objLeft.Write(objWriter);

                    Condition objRight = (Condition)Right;

                    objRight.Write(objWriter);
                }
                catch(Exception)
                {
                    string strValue = GetValue();

                    objWriter.WriteElementString("prop", GetPropertyId());
                    objWriter.WriteElementString("value", strValue, true);

                    switch(Choice.GetId(this.Op))
                    {
                        case "bt":
                        {
                            StringList aParts = new StringList(strValue, "///", false);

                            objWriter.WriteElementString("occurs", Op);
                            objWriter.WriteElementString("from",   aParts[0]);
                            objWriter.WriteElementString("to",     aParts[1]);
                            break;
                        }

                        case "oc":
                        case "wi":
                        {
                            StringList aParts = new StringList(strValue, "///", false);

                            objWriter.WriteElementString("occurs",     Op);
                            objWriter.WriteElementString("units",      aParts[0]);
                            objWriter.WriteElementString("within_amt", aParts[1]);
                            break;
                        }
                    }

                    if(this.Right is Choice)
                        objWriter.WriteElementString("display", ((Choice)this.Right).Caption, true);
                }
            }
        }  
     
        /****************************************************************************/
        public virtual void Write(cXMLWriter objWriter)
        {
            if(Left != null)
            {
                objWriter.WriteStartElement("condition");
                objWriter.WriteAttributeString("op", Choice.GetId(this.Op));

                try
                {
                    Condition objLeft = (Condition)Left;

                    objLeft.Write(objWriter);

                    Condition objRight = (Condition)Right;

                    objRight.Write(objWriter);
                }
                catch(Exception)
                {
                    objWriter.WriteAttributeString("prop", GetPropertyId());
                    objWriter.WriteAttributeString("value", GetValue(), true);

                    if(this.Right is Choice)
                        objWriter.WriteAttributeString("display", ((Choice)this.Right).Caption, true);
                }

                objWriter.WriteEndElement();
            }
        }       

        /****************************************************************************/
        public bool Evaluate(IDataObject objData)
        {
            try
            {
                Condition objLeft = (Condition)this.Left;
                bool       bLeft   = objLeft.Evaluate(objData);

                if(this.Right == null)
                    return(bLeft);

                bool bAnd = Choice.GetId(this.Op) == "and";

                if(bLeft && !bAnd)
                    return(true);

                Condition objRight = (Condition)this.Right;
                bool       bRight   = objRight.Evaluate(objData);

                if(!bAnd)
                    return(bRight);

                return(bLeft && bRight);
            }
            catch
            {
                string strProp        = this.Left.ToString();
                string strCheckValue  = this.Right.ToString();
                string strActualValue = objData.GetAttributeValue(strProp);

                return(Evaluates(strProp, Choice.GetId(this.Op), strActualValue, strCheckValue));
            }
        }

        /****************************************************************************/
        private bool Evaluates(string strProp, string strOp, string strActualValue, string strCheckValue)
        {
            int iCompare = CompareValues(strProp, strActualValue.ToUpper(), strCheckValue.ToUpper());

            // ??? ends, starts

            switch(strOp)
            {
                case "is": 
                case "eq":  return(iCompare == 0);
                case "isnt": 
                case "ne":  return(iCompare != 0);
                case "gt":  return(iCompare == 1);
                case "ge":  return(iCompare >= 0);
                case "lt":  return(iCompare == -1);
                case "le":  return(iCompare <= 0);
                case "co":  return(strActualValue.IndexOf(strCheckValue) != -1);
                default:    return(false);
            }
        }

        /****************************************************************************/
        public bool Evaluate(XmlElement xmlObject)
        {
            try
            {
                Condition objLeft = (Condition)this.Left;
                bool       bLeft   = objLeft.Evaluate(xmlObject);

                if(this.Right == null)
                    return(bLeft);

                bool bAnd = Choice.GetId(this.Op) == "and";

                if(bLeft && !bAnd)
                    return(true);

                Condition objRight = (Condition)this.Right;
                bool       bRight  = objRight.Evaluate(xmlObject);

                if(!bAnd)
                    return(bRight);

                return(bLeft && bRight);
            }
            catch
            {
                string strProp        = this.Left.ToString();
                string strCheckValue  = Choice.GetId(this.Right); // ??????
                string strActualValue = xmlObject.GetChildText(strProp);

                return(Evaluates(strProp, Choice.GetId(this.Op), strActualValue, strCheckValue));
            }
        }

        /****************************************************************************/
        public virtual int CompareValues(string strProp, string strActualValue, string strCheckValue)
        {
            try
            {
                return(m_objQuery.CompareValues(strProp, strActualValue, strCheckValue));
            }
            catch(ArgumentException ex)
            {
                if(ex.Message != "ERR_NOATTRIBUTE")
                    throw;

                // just compare as strings
                return(strActualValue.CompareTo(strCheckValue));
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// A generic query
    /// </summary>
    public class Query : Condition
    {
        private List<string> m_aFields = new List<string>();

        /****************************************************************************/
        public Query()
        {
            m_objQuery = this;
        }

        /****************************************************************************/
        public Query(string xmlQuery)
        {
            Init(XmlDoc.Load(xmlQuery));

            /*
            XmlParserContext objContext = new XmlParserContext(null, null, "", null, null, null, "", "", XmlSpace.None);
            XmlTextReader    objReader  = new XmlTextReader(strValue, XmlNodeType.Element, objContext);

            winControl.Query = new cQuery();
            winControl.Query.Load(objReader);
            */

        }

        /****************************************************************************/
        public Query(XmlNode xmlQuery)
        {
            if(xmlQuery != null)
                Init(xmlQuery);
        }

        /****************************************************************************/
        public override void Load(XmlNode xmlData)
        {
            Init(xmlData);
        }

        /****************************************************************************/
        private void Init(XmlNode xmlQuery)
        {
            m_objQuery = this;

            string strXPath = "//query/condition";

            if(xmlQuery.LocalName == "conditions")
                strXPath = "condition";

            XmlNodeList aConditions = xmlQuery.SelectNodes(strXPath);

            foreach(XmlElement xmlCondition in aConditions)
                AddCondition(xmlCondition);

            XmlNodeList aFields = xmlQuery.SelectNodes("//fields/field");

            foreach(XmlElement xmlField in aFields)
                m_aFields.Add(xmlField.InnerText.Trim());
        }

        /****************************************************************************/
        public virtual List<string> Fields      
        {
            get{return(m_aFields);}
            set{m_aFields = value;}
        }

        /****************************************************************************/
        public void SetFields(ChoiceList aFields)
        {
            m_aFields.Clear();

            foreach(Choice objField in aFields)
                m_aFields.Add(objField.Id);
        }

        /****************************************************************************/
        public override int CompareValues(string strProp, string strActualValue, string strCheckValue)
        {
            return(strActualValue.CompareTo(strCheckValue));
        }

        /****************************************************************************/
        protected override string GetLeftCaption(object objLeft)
        {
            return(Choice.GetCaption(objLeft));
        }

        /****************************************************************************/
        protected override string GetRightCaption(object objLeft, object objRight)
        {
            return(Choice.GetCaption(objRight));
        }

        /****************************************************************************/
        public virtual string FormatValue(object objProperty, object objOperator, object objValue)
        {
            string strOperator = Choice.GetId(objOperator);

            switch(strOperator)
            {
                case "bt":
                case "bef":
                case "aft":
                    return(Utility.ToDateTime(objValue).Date.ToShortDateString());

                default:
                    return(Choice.GetCaption(objValue));
            }
        }

        /****************************************************************************/
        public override void Write(cXMLWriter objWriter)
        {
            using(new XmlElementWriter(objWriter, "query"))
            {
                base.Write(objWriter);

                using(new XmlElementWriter(objWriter, "fields"))
                {
                    foreach(string strField in m_aFields)
                        objWriter.WriteElementString("field", strField);
                }
            }
        }

        /****************************************************************************/
        public XmlDocument ToXml()
        {
            cXMLWriter objWriter = new cXMLWriter();

            Write(objWriter);

            return(objWriter.Xml);
        }

        /****************************************************************************/
        public string ToXmlString()
        {
            cXMLWriter objWriter = new cXMLWriter();

            Write(objWriter);

            return(objWriter.ToString());
        }
    }
}
