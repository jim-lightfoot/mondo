/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: Choice.cs				   		                        */
/*        Class(es): IChoice, Choice, ChoiceList  		  		            */
/*          Purpose: Classes for listing choices                            */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 16 Sep 2003                                            */
/*                                                                          */
/*   Copyright (c) 2003 Tenth Generation - All rights reserved              */
/*                                                                          */
/****************************************************************************/

using System;
using System.Data;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using Mondo.Xml;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public interface IChoice : IDictionaryEntry<string>, IIdObject<string> 
    {
        string Caption   {get;}
    }

    /****************************************************************************/
    /****************************************************************************/
    public class Choice : XmlLoadable, IComparable<Choice>, IChoice
    {
        private string m_id         = "";
        private string m_strCaption = "";

        /****************************************************************************/
        public Choice(XmlElement xmlChoice)
        {
            m_id         = xmlChoice.GetAttribute("id");
            m_strCaption = xmlChoice.GetAttribute("caption");
        }

        /****************************************************************************/
        public Choice(DataRow objRow, bool bUpperId)
        {
            m_id         = DBRow.GetColumn(objRow, "id");
            m_strCaption = DBRow.GetColumn(objRow, "caption");

            if(bUpperId)
                m_id = m_id.ToUpper();
        }

        /****************************************************************************/
        public Choice(string idChoice, string strCaption)
        {
            m_id         = idChoice;
            m_strCaption = strCaption;
        }

        /****************************************************************************/
        public Choice()
        {
        }

        /****************************************************************************/
        public Choice(XmlTextReader objReader)
        {
            Load(objReader);
        }

        /****************************************************************************/
        public override void SetAttribute(string strAttrName, string strValue)
        {
            switch(strAttrName)
            {
                case "id":          m_id = strValue;         break;
                case "caption":     m_strCaption = strValue; break;
                case "value":       m_strCaption = strValue; break;
            }
        }

        /****************************************************************************/
        public string Key(int i)
        {
            return(Id);
        }

        /****************************************************************************/
        public string Id        {get{return(m_id);} set{m_id = value;}}
        public string Value     {get{return(m_id);}}
        public string Caption   {get{return(m_strCaption);} set{m_strCaption = value;}}
        public string Name      {get{return(m_strCaption);}}

        /****************************************************************************/
        public static string GetId(object objChoice)
        {
            if(objChoice == null)
                return("");
                
            if(objChoice is IChoice)
                return((objChoice as IChoice).Id);

            return(objChoice.ToString());
        }

        /****************************************************************************/
        public static string GetCaption(object objChoice)
        {
            if(objChoice == null)
                return("");
                
            if(objChoice is IChoice)
                return((objChoice as IChoice).Caption);

            return(objChoice.ToString());
        }

        /****************************************************************************/
        public override bool Equals(object obj)
        {
            try
            {
                IChoice objChoice = obj as IChoice;

                return(this.Id == objChoice.Id);
            }
            catch
            {
                return(this.Id == obj.ToString());
            }
        }

        /****************************************************************************/
        public override int GetHashCode()
        {
            return(this.Id.GetHashCode());
        }

        /****************************************************************************/
        public override string ToString()
        {
            return(m_strCaption);
        }
       
        #region IComparable<Choice> Members

        public int CompareTo(Choice other)
        {
 	        return(this.Caption.CompareTo(other.Caption));
        }

        #endregion
    }

    /****************************************************************************/
    /****************************************************************************/
    public class ChoiceList : XmlLoadable, IDataObjectSource, IList, ITabularData
    {
        private KeyedDictionary<string, IChoice> m_aChoices = new KeyedDictionary<string,IChoice>(17);

        /****************************************************************************/
        public ChoiceList()
        {
        }

        /****************************************************************************/
        public ChoiceList(IDataObjectSource objSource, IDataObjectSourceFilter objFilter)
        {
            Load(objSource, objFilter);
        }

        /****************************************************************************/
        public ChoiceList(IDataObjectSource objSource)
        {
            Load(objSource, null);
        }

        /****************************************************************************/
        public ChoiceList(XmlNode xmlChoiceList, bool bIgnoreDuplicates)
        {
            Load(xmlChoiceList, bIgnoreDuplicates);
        }

        /****************************************************************************/
        public ChoiceList(XmlNodeList aChoices)
        {
            AddChoices(aChoices, false);
        }

        /****************************************************************************/
        public ChoiceList(XmlTextReader objReader)
        {
            Load(objReader);
        }

        /****************************************************************************/
        public ChoiceList(DataSet dsChoiceList) : this(dsChoiceList.Tables[0], false)
        {
        }

        /****************************************************************************/
        public ChoiceList(DataTable dtChoiceList, bool bUpperId)
        {
            DataRowCollection aRows = dtChoiceList.Rows;

            foreach(DataRow objRow in aRows)
                Add(new Choice(objRow, bUpperId));

            return;
        }

        /****************************************************************************/
        public ChoiceList(string[] aList)
        {
            int nChoices = aList.Length / 2;

            if(aList.Length != nChoices * 2)
                throw new ArgumentException("The string array must contain an even number of strings");

            for(int i = 0; i < nChoices; ++i)
                Add(aList[i*2], aList[i*2+1]);
        }

        /****************************************************************************/
        public ChoiceList(IList aIds, IList aCaptions)
        {
            Init(aIds, aCaptions);
        }
        
        /****************************************************************************/
        public virtual void Clear()
        {
            m_aChoices.Clear();
        }

        /****************************************************************************/
        public bool Contains(string strId)
        {
            return(m_aChoices.ContainsKey(strId));
        }

        /****************************************************************************/
        public int Count
        {
            get
            {
                return(m_aChoices.Count);
            }
        }

        /****************************************************************************/
        public IChoice this [string Key] 
        {
            get
            {             
                return(m_aChoices[Key]);
            }
        }

        /****************************************************************************/
        public void Sort()
        {
            m_aChoices.Sort();
        }

        /****************************************************************************/
        public void Add(string idChoice, string strCaption)
        {
            Add(new Choice(idChoice, strCaption));
        }

        /****************************************************************************/
        public void Add(IChoice objChoice)
        {
            m_aChoices.Add(objChoice);
        }

        /****************************************************************************/
        public void Insert(int iPosition, IChoice objChoice)
        {
            m_aChoices.Insert(iPosition, objChoice);
        }

        /****************************************************************************/
        public override void LoadElement(XmlTextReader objReader)
        {
            if(objReader.Name == "choice")
            {
                Choice objChoice = new Choice();

                objChoice.Load(objReader);

                Add(objChoice);
            }
            else
                Load(objReader);
        }

        /****************************************************************************/
        private void Load(IDataObjectSource objSource, IDataObjectSourceFilter objFilter)
        {
            foreach(string idColumn in objSource.Columns)
                if(objFilter == null || objFilter.Matches(objSource, idColumn))
                    Add(idColumn, objSource.Get(idColumn));
        }

        /****************************************************************************/
        protected virtual void Load(XmlNode xmlChoiceList, bool bIgnoreDuplicates)
        {
            if(xmlChoiceList != null)
            {
                XmlNodeList aChoices = xmlChoiceList.SelectNodes("//choice");

                if(aChoices.Count == 0)
                {
                    aChoices = xmlChoiceList.SelectNodes("//object");

                    foreach(XmlElement xmlChoice in aChoices)
                    {
                        string id         = xmlChoice.GetChildText("id");
                        string strCaption = xmlChoice.GetChildText("caption");

                        if(bIgnoreDuplicates && m_aChoices.ContainsKey(id))
                            continue;

                        Add(CreateChoice(id, strCaption));
                    }
                }
                else
                    AddChoices(aChoices, bIgnoreDuplicates);
            }
        }

        /****************************************************************************/
        private void AddChoices(XmlNodeList aChoices, bool bIgnoreDuplicates)
        {
            foreach(XmlElement xmlChoice in aChoices)
            {
                IChoice objChoice = CreateChoice(xmlChoice);

                if(bIgnoreDuplicates && m_aChoices.ContainsKey(objChoice.Id))
                    continue;

                Add(objChoice);
            }
        }

        /****************************************************************************/
        protected virtual IChoice CreateChoice(XmlElement xmlChoice)
        {
            return(new Choice(xmlChoice));
        }

        /****************************************************************************/
        protected virtual IChoice CreateChoice(string idChoice, string strCaption)
        {
            return(new Choice(idChoice, strCaption));
        }

        /****************************************************************************/
        public IChoice GetByIndex(int iIndex)
        {
            return(m_aChoices.GetValueAt(iIndex));
        }

        /****************************************************************************/
        public int IndexOfKey(string strKey)
        {
            return(m_aChoices.IndexOfKey(strKey));
        }

        /****************************************************************************/
        private void Init(IList aIds, IList aCaptions)
        {
            if(aIds.Count != aCaptions.Count)
                throw new ArgumentException();

            int nItems = aIds.Count;

            for(int i = 0; i < nItems; ++i)
            {
                try
                {
                    Add(aIds[i].ToString(), aCaptions[i].ToString());
                }
                catch
                {
                }
            }
        }

        /****************************************************************************/
        public IEnumerator GetEnumerator() 
        {
            return(m_aChoices.GetArrayEnumerator());
        }

        /****************************************************************************/
        public IEnumerable Columns    
        {
            get
            {
                string[] aStrings = new string[this.Count];
                int iIndex = 0;

                foreach(Choice objChoice in this)
                    aStrings[iIndex++] = objChoice.Id;

                return(aStrings);
            }
        }

        #region IDataObjectSource Members

        public string Get(string strColumnName, string defaultVal = "")
        {
            return(m_aChoices[strColumnName].Caption);
        }

        public IDataObjectSource GetSubObject(string idSubObject)
        {
            return(null);
        }

        public DataSourceList GetSubList(string idSubList)
        {
            return(null);
        }

        public void SetValue(string strColumnName, string strValue)
        {
            // Not allowed
        }

        public void SetValue(string strColumnName, int iValue)
        {
            // Not allowed
        }

        public void SetValue(string strColumnName, float fValue)
        {
            // Not allowed
        }

        public void SetValue(string strColumnName, long iValue)
        {
            // Not allowed
        }

        public void SetValue(string strColumnName, bool bValue)
        {
            // Not allowed
        }

        public void SetValue(string strColumnName, decimal dValue)
        {
            // Not allowed
        }

        public void SetValue(string strColumnName, double dValue)
        {
            // Not allowed
        }

        public void SetValue(string strColumnName, Guid guidValue)
        {
            // Not allowed
        }

        public void SetValue(string strColumnName, DateTime dtValue)
        {
            // Not allowed
        }

        #endregion

        #region IList Members

        /****************************************************************************/
        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        /****************************************************************************/
        public bool Contains(object value)
        {
            if(value is IChoice)
                return(m_aChoices.ContainsKey((value as IChoice).Id));

            return(false);
        }

        /****************************************************************************/
        public int IndexOf(object value)
        {
            if(value is IChoice)
                return(m_aChoices.IndexOf(value as IChoice));

            return(-1);
        }

        /****************************************************************************/
        public void Insert(int index, object value)
        {
            if(value is IChoice)
                m_aChoices.Insert(index, value as IChoice);
        }

        /****************************************************************************/
        public bool IsFixedSize
        {
            get { return(false); }
        }

        /****************************************************************************/
        public bool IsReadOnly
        {
            get { return(false); }
        }

        /****************************************************************************/
        public void Remove(object value)
        {
            if(value is IChoice)
                m_aChoices.Remove((value as IChoice).Id);
        }

        /****************************************************************************/
        public void RemoveAt(int index)
        {
            m_aChoices.RemoveAt(index);
        }

        /****************************************************************************/
        public object this[int index]
        {
            get
            {
                return(m_aChoices.GetValueAt(index));
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection Members

        /****************************************************************************/
        public void CopyTo(Array array, int index)
        {
            m_aChoices.CopyTo(array, index);
        }

        /****************************************************************************/
        public bool IsSynchronized
        {
            get { return(m_aChoices.IsSynchronized); }
        }

        /****************************************************************************/
        public object SyncRoot
        {
            get { return(m_aChoices.SyncRoot); }
        }

        #endregion

        #region ITabularData Members

        public int NumRows
        {
            get { return(this.Count); }
        }

        public int NumColumns
        {
            get { return(2); }
        }

        public object GetValue(int iRow, int iColumn)
        {
            return(Choice.GetId(GetByIndex(iRow)));
        }

        public string GetDisplay(int iRow, int iColumn)
        {
            return(Choice.GetCaption(GetByIndex(iRow)));
        }

        public void SetValue(int iRow, int iColumn, object objValue)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void InsertRow(int iIndex)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void AddRow()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void DeleteRow(int iIndex)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string ToXmlString()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        /****************************************************************************/
        public bool Modified
        {
            get {return(false);}
            set {}
        }

        /****************************************************************************/
        public bool ReadOnly
        {
            get {return(false);}
            set {}
        }

        /****************************************************************************/
        public void OnBeforeSave()
        {
        }

        /****************************************************************************/
        public void OnAfterSave()
        {
        }
    }
}
