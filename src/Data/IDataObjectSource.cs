/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: DataObjectSource.cs				   		            */
/*        Class(es): IDataObjectSource, DataObjectSource, DBRow,            */
/*                      DataReaderObjectSource, XmlObjectSource,            */
/*                      XmlAttributeSource, DataSourceDictionary,           */
/*                      DBRowDictionary, DataSourceList, DBRowList          */
/*                                                                          */
/*          Purpose: Data objects and collections                           */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 16 Sep 2004                                            */
/*                                                                          */
/*   Copyright (c) 2004-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Data;
using System.Xml;
using System.Reflection;
using System.Data.SqlTypes;
using System.Data.Common;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Win32;
using Mondo.Xml;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public interface IModel
    {
        bool        Modified    {get;set;}
        bool        ReadOnly    {get;set;}

        void        OnBeforeSave();
        void        OnAfterSave();
    }

    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// interface for objects that can return attributed (column) data
    /// The enumeration enumerates the columns names
    /// </summary>
    public interface IDataObjectSource : IModel
    {
        string              Get(string strColumnName);
        IDataObjectSource   GetSubObject(string idSubObject);
        DataSourceList      GetSubList(string idSubList);

        void                SetValue(string strColumnName, string strValue);
        void                SetValue(string strColumnName, int iValue);
        void                SetValue(string strColumnName, long iValue);
        void                SetValue(string strColumnName, bool bValue);
        void                SetValue(string strColumnName, decimal dValue);
        void                SetValue(string strColumnName, double dValue);
        void                SetValue(string strColumnName, float fValue);
        void                SetValue(string strColumnName, Guid guidValue);
        void                SetValue(string strColumnName, DateTime dtValue);

        IEnumerable         Columns     {get;}
    }

	/****************************************************************************/
	/****************************************************************************/
	public static class DataObjectSourceExtensions
	{    
        /****************************************************************************/
	    public static T Get<T>(this IDataObjectSource ds, string strColumnName, T defaultVal = default(T), bool required = false) where T : struct
		{
    		string val = ds.Get(strColumnName);

            return(Utility.Convert<T>(val, defaultVal));
		}
    }

    /****************************************************************************/
    public interface IDataObjectSourceFilter
    {
        bool Matches(IDataObjectSource objSource, string strColumnName);
    }

    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// interface for objects that can return attributed (column) data
    /// The enumeration enumerates the columns names
    /// </summary>
    public interface IHierarchicalDataSource : IDataObjectSource
    {
        IEnumerable<IHierarchicalDataSource> GetChildren(string strFilter);
        IDataObjectSource                    Attributes {get;}
    }

     /****************************************************************************/
    /****************************************************************************/
    public interface IDataSourceContainer
    {
        IDataObjectSource DataSource        {get;}
    }

   /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Abstract implementation of IDataObjectSource
    /// </summary>
    public abstract class DataObjectSource : IDataObjectSource
    {
        private bool m_bModified = false;
        private bool m_bReadOnly = false;
        
        /****************************************************************************/
        protected DataObjectSource()
        {
        }

        /****************************************************************************/
        public static IDataObjectSource Create(object objValue)
        {
            if(objValue == null)
                return(new NullDataSource());
                
            if(objValue is IDataObjectSource)
                return(objValue as IDataObjectSource);
                
            if(objValue is IDataSourceContainer)
                return((objValue as IDataSourceContainer).DataSource);
                
            if(objValue is XmlNode)
                return(new XmlObjectSource(objValue as XmlNode));
                
            if(objValue is DataRow)
                return(new DBRow(objValue as DataRow));
                
            if(objValue is DataTable)
            {
                DataTable dtTable = objValue as DataTable;
                
                if(dtTable.Rows.Count == 0)
                    return(new NullDataSource());
                
                return(new DBRow(dtTable.Rows[0]));
            }
                
            if(objValue is DataView)
            {
                foreach(DataRow objRow in (objValue as DataView))
                    return(new DBRow(objRow));
                    
                return(new NullDataSource());
            }
            
            if(objValue is DataSet)
            {
                DataSet dsDataSet = objValue as DataSet;
                
                if(dsDataSet.Tables.Count == 0)
                    return(new NullDataSource());
                    
                DataTable dtTable = dsDataSet.Tables[0];
                
                if(dtTable.Rows.Count == 0)
                    return(new NullDataSource());
                
                return(new DBRow(dsDataSet));
            }
            
            if(objValue is DbDataReader)
                return(new DataReaderObjectSource(objValue as DbDataReader));
            
            if(objValue is RegistryKey)
                return(new RegistryDataSource(objValue as RegistryKey));

            return(new DataObject(objValue));
        }
        
        #region IDataObjectSource Members

        public abstract string              Get(string strColumnName);
        public abstract IEnumerable         Columns    {get;}
        
        /****************************************************************************/
        public virtual IDataObjectSource GetSubObject(string idSubObject)
        {
            return(null);
        }

        /****************************************************************************/
        public virtual DataSourceList GetSubList(string idSubList)
        {
            return(null);
        }

        /****************************************************************************/
        public abstract void SetValue(string strColumnName, string strValue);
        public virtual  void SetValue(string strColumnName, int iValue)          {SetValue(strColumnName, iValue.ToString());}
        public virtual  void SetValue(string strColumnName, float fValue)        {SetValue(strColumnName, fValue.ToString());}
        public virtual  void SetValue(string strColumnName, long iValue)         {SetValue(strColumnName, iValue.ToString());}
        public virtual  void SetValue(string strColumnName, bool bValue)         {SetValue(strColumnName, bValue.ToString());}
        public virtual  void SetValue(string strColumnName, decimal dValue)      {SetValue(strColumnName, dValue.ToString());}
        public virtual  void SetValue(string strColumnName, double dValue)       {SetValue(strColumnName, dValue.ToString());}
        public virtual  void SetValue(string strColumnName, Guid guidValue)      {SetValue(strColumnName, guidValue.ToString());}
        public virtual  void SetValue(string strColumnName, DateTime dtValue)    {SetValue(strColumnName, dtValue.ToString());}

        #endregion

        /****************************************************************************/
        public virtual bool Modified
        {
            get {return(m_bModified);}
            set {m_bModified = value;}
        }

        /****************************************************************************/
        public virtual bool ReadOnly
        {
            get {return(m_bReadOnly);}
            set {m_bReadOnly = value;}
        }

        /****************************************************************************/
        public virtual void OnBeforeSave()
        {
            // Do nothing in base class
        }

        /****************************************************************************/
        public virtual void OnAfterSave()
        {
            // Do nothing in base class
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// A DataObjectSource implementation for a registry key (RegistryKey)
    /// </summary>
    public class RegistryDataSource : DataObjectSource
    {
        private RegistryKey m_objKey;

        /****************************************************************************/
        public RegistryDataSource(RegistryKey objKey)
        {
            m_objKey = objKey;
        }

        /****************************************************************************/
        public override string Get(string strColumnName)
        {
            return(m_objKey.GetValue(strColumnName).Normalized());
        }

        /****************************************************************************/
        public override void SetValue(string strColumnName, string strValue)
        {
            if(!this.ReadOnly)
                m_objKey.SetValue(strColumnName, strValue);
        }

        /****************************************************************************/
        public override IEnumerable Columns
        {
            get { return(new RegistryDataSourceList(m_objKey)); }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// A DataObjectSource implementation for null data source
    /// </summary>
    public sealed class NullDataSource : DataObjectSource
    {
        /****************************************************************************/
        public NullDataSource()
        {
        }

        /****************************************************************************/
        public override IEnumerable Columns
        {
            get { return(null); }
        }
        
        /****************************************************************************/
        public override string Get(string idColumn)
        {
            return("");
        }

        /****************************************************************************/
        public override void SetValue(string strColumnName, string strValue)
        {
            // Override to do nothing
        }

        /****************************************************************************/
        public override bool ReadOnly
        {
            get {return(true);}
            set {}
        }
    }
    
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// A DataObjectSource implementation for a database row (DataRow)
    /// </summary>
    public sealed class DBRow : DataObjectSource
    {
        private DataRow m_objRow = null;
        private DataSet m_objDataSet = null;

        /****************************************************************************/
        public DBRow(DataRow objRow)
        {
            m_objRow = objRow;
        }

        /****************************************************************************/
        public DBRow(DataSet objDataSet)
        {
            m_objRow     = objDataSet.Tables[0].Rows[0];
            m_objDataSet = objDataSet;
        }

        /****************************************************************************/
        public DBRow(DataTable objTable)
        {
            m_objRow = objTable.Rows[0];
        }

        /****************************************************************************/
        public DBRow()
        {
        }

        /****************************************************************************/
        public DataRow DataSource
        {
            get {return(m_objRow);}
            set {m_objRow = value;}
        }
               
        /****************************************************************************/
        public DataSet DataSet
        {
            get {return(m_objDataSet);}
        }
               
        /****************************************************************************/
        public override DataSourceList GetSubList(string idSubList)
        {
            if(m_objDataSet != null && m_objDataSet.Tables.Contains(idSubList))
                return(DataSourceList.Create(m_objDataSet.Tables[idSubList]));
            
            return(null);
        }

        /****************************************************************************/
        public DataSourceList GetSubList(int iIndex)
        {
            if(m_objDataSet != null && m_objDataSet.Tables.Count > iIndex)
                return(DataSourceList.Create(m_objDataSet.Tables[iIndex]));
            
            return(null);
        }

        /****************************************************************************/
        public override IEnumerable Columns    
        {
            get
            {
                return(m_objRow.Table.Columns);
            }
        }

        /****************************************************************************/
        public override void SetValue(string strColumnName, string strValue)
        {
            throw new NotSupportedException();
        }

        /****************************************************************************/
        public override string Get(string idColumn)
        {
            try
            {
                return(m_objRow[idColumn].ToString().Trim());
            }
            catch
            {
                return("");
            }
        }

        /****************************************************************************/
        public static string GetColumn(DataRow objRow, string idColumn)
        {
            try
            {
                return(objRow[idColumn].ToString().Trim());
            }
            catch
            {
                return("");
            }
        }

        /****************************************************************************/
        public static T GetColumn<T>(DataRow objRow, string idColumn, T defaultVal = default(T)) where T : struct
        {
    		string val = GetColumn(objRow, idColumn);

            return(Utility.Convert<T>(val, defaultVal));
        }
    }
    
    /****************************************************************************/
    /****************************************************************************/
    public class DataSourceDictionary : Dictionary<Guid, IDataObjectSource>
    {
        /****************************************************************************/
        public DataSourceDictionary()
        {
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class DBRowDictionary : DataSourceDictionary
    {
        /****************************************************************************/
        public DBRowDictionary(DataTable dtData, string strColumnName)
        {
            DataRowCollection aRows = dtData.Rows;
            
            foreach(DataRow row in aRows)
            {
                DBRow dbRow = new DBRow(row);

                this.Add(dbRow.Get<Guid>(strColumnName), dbRow);
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// A DataObjectSource implementation for a database reader (DbDataReader)
    /// </summary>
    public class DataReaderObjectSource : DataObjectSource
    {
        private DbDataReader m_objDataReader;

        /****************************************************************************/
        public DataReaderObjectSource(DbDataReader objReader)
        {
            m_objDataReader = objReader;
        }

        /****************************************************************************/
        public override IEnumerable Columns    
        {
            get
            {
                int      nColumns = m_objDataReader.FieldCount;
                string[] aColumns = new string[nColumns];

                for(int i = 0; i < nColumns; ++i)
                    aColumns[i] = m_objDataReader.GetName(i);

                return(aColumns);
            }
        }

        /****************************************************************************/
        public override string Get(string strColumnName)
        {
            try
            {
                return(m_objDataReader[strColumnName].ToString().Trim());
            }
            catch
            {
                return("");
            }
        }

        /****************************************************************************/
        public override void SetValue(string strColumnName, string strValue)
        {
            throw new NotSupportedException();
        }

        /****************************************************************************/
        public override bool ReadOnly
        {
            get {return(true);}
            set {}
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// A DataObjectSource implementation for an XmlNode where the data items are child nodes
    /// </summary>
    public class XmlObjectSource : DataObjectSource, IHierarchicalDataSource
    {
        private XmlNode m_xmlSource;

        /****************************************************************************/
        public XmlObjectSource(XmlNode xmlSource)
        {
            m_xmlSource = xmlSource;
        }

        /****************************************************************************/
        public XmlNode DataSource
        {
            get {return(m_xmlSource);}
            set {m_xmlSource = value;}
        }
        
        /****************************************************************************/
        public override IEnumerable Columns    
        {
            get
            {
                int      nColumns = m_xmlSource.ChildNodes.Count;
                string[] aColumns = new string[nColumns];

                for(int i = 0; i < nColumns; ++i)
                    aColumns[i] = m_xmlSource.ChildNodes[i].LocalName;

                return(aColumns);
            }
        }

        /****************************************************************************/
        public override string Get(string strColumnName)
        {
            return(m_xmlSource.GetChildText(strColumnName));
        }
        
        /****************************************************************************/
        public override void SetValue(string strColumnName, string strValue)     
        {
            if(!this.ReadOnly)
            {
                string strCurrentValue = Get(strColumnName);

                if(strCurrentValue != strValue)
                {
                    m_xmlSource.SetChildText(strColumnName, strValue);
                    this.Modified = true;
                }
            }

            return;
        }

        #region IHierarchicalDataSource Members

        /****************************************************************************/
        public IEnumerable<IHierarchicalDataSource> GetChildren(string strFilter)
        {
            if(string.IsNullOrEmpty(strFilter))
                strFilter = "*";

            List<IHierarchicalDataSource> aChildren = new List<IHierarchicalDataSource>();

            foreach(XmlNode xmlChild in m_xmlSource.SelectNodes(strFilter))
                aChildren.Add(new XmlObjectSource(xmlChild));

            return(aChildren);
        }

        /****************************************************************************/
        public IDataObjectSource Attributes 
        {
            get
            {
                return(new XmlAttributeSource(m_xmlSource));
           }
        }

        #endregion
    }

    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// A DataObjectSource implementation for an XmlNode where the data items are attributes
    /// </summary>
    public class XmlAttributeSource : DataObjectSource
    {
        private XmlNode m_xmlSource;

        /****************************************************************************/
        public XmlAttributeSource(XmlNode xmlSource)
        {
            m_xmlSource = xmlSource;
        }

        /****************************************************************************/
        public XmlNode DataSource
        {
            get {return(m_xmlSource);}
            set {m_xmlSource = value;}
        }

        /****************************************************************************/
        public override IEnumerable Columns    
        {
            get
            {
                int      nColumns = m_xmlSource.Attributes.Count;
                string[] aColumns = new string[nColumns];

                for(int i = 0; i < nColumns; ++i)
                    aColumns[i] = m_xmlSource.Attributes[i].Name;

                return(aColumns);
            }
        }

        /****************************************************************************/
        public override string Get(string strColumnName)
        {
            return(m_xmlSource.GetAttribute(strColumnName));
        }
        
        /****************************************************************************/
        public override void SetValue(string strColumnName, string strValue)     
        {
            if(!this.ReadOnly)
            {
                string strCurrentValue = Get(strColumnName);

                if(strCurrentValue != strValue)
                {
                    m_xmlSource.SetAttribute(strColumnName, strValue);
                    this.Modified = true;
                }
            }

            return;
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface IDataSourceSortable
    {
        void Sort(string idAttribute, bool bAscending);
    }

    /****************************************************************************/
    /****************************************************************************/
    public abstract class DataSourceList : ICollection, IEnumerable, IDataSourceSortable
    {
        private IEnumerable m_objSource = null;
        
        /****************************************************************************/
        protected DataSourceList(IEnumerable objSource)
        {
            m_objSource = objSource;
        }

        /****************************************************************************/
        protected DataSourceList()
        {
        }
        
        /****************************************************************************/
        public static DataSourceList Create(object objData)
        {
            if(objData == null)
                return(null);

            if(objData is DataSet)
                return(new DBRowList(objData as DataSet));

            if(objData is DataTable)
                return(new DBRowList(objData as DataTable));

            if(objData is DataView)
                return(new DBRowList(objData as DataView));

            if(objData is DataRowCollection)
                return(new DBRowList(objData as DataRowCollection));

            if(objData is XmlNodeList)
                return(new XmlObjectList(objData as XmlNodeList));

            if(objData is XmlDocument)
                return(new XmlObjectList((objData as XmlDocument).FirstChild, "*"));

            if(objData is XmlNode)
                return(new XmlObjectList(objData as XmlNode, "*"));

            if(objData is IEnumerable)
                return(new DataObjectList(objData as IEnumerable));
                
            if(objData is RegistryKey)
                return(new RegistryDataSourceList(objData as RegistryKey));
                
            throw new ArgumentException("Unknown list type!");
        }
        
        /****************************************************************************/
        public IEnumerable Source
        {
            get {return(m_objSource);}
            set {m_objSource = value;}
        }

        /****************************************************************************/
        public    abstract int               Count    {get;}
        protected abstract IDataObjectSource GetObject(object objValue);

        #region IEnumerable Members

        /****************************************************************************/
        public virtual IEnumerator GetEnumerator()
        {
            return(new MyEnumerator(this, m_objSource));
        }
        
        /****************************************************************************/
        public class MyEnumerator : IEnumerator 
        {
            IEnumerator    m_objClient;
            DataSourceList m_objList;

            /****************************************************************************/
            public MyEnumerator(DataSourceList objList, IEnumerable objClient) 
            {
                m_objList   = objList;
                m_objClient = objClient.GetEnumerator();
            }

            #region IEnumerator Members

            /****************************************************************************/
            public object Current
            {
                get 
                { 
                    return(m_objList.GetObject(m_objClient.Current)); 
                }
            }

            /****************************************************************************/
            public bool MoveNext()
            {
                return(m_objClient.MoveNext());
            }

            /****************************************************************************/
            public void Reset()
            {
                m_objClient.Reset();
            }

            #endregion
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool IsSynchronized
        {
            get { return(false); }
        }

        public object SyncRoot
        {
            get { return(null); }
        }

        #endregion

        #region IDataSourceSortable Members

        public virtual void Sort(string idAttribute, bool bAscending)
        {
            if(m_objSource is IDataSourceSortable)
                (m_objSource as IDataSourceSortable).Sort(idAttribute, bAscending);
        }

        #endregion
    }
   
    /****************************************************************************/
    /****************************************************************************/
    public class DBRowList : DataSourceList
    {
        private DBRow m_objRow = new DBRow();
        private int   m_nRows  = 0;
        
        /****************************************************************************/
        public DBRowList(DataRowCollection aRows) : base(aRows)
        {
            m_nRows = aRows.Count;
        }

        /****************************************************************************/
        public DBRowList(DataTable dtData) : this(dtData.Rows)
        {
        }

        /****************************************************************************/
        public DBRowList(DataView dvData) : base(dvData)
        {
            m_nRows = dvData.Count;
        }

        /****************************************************************************/
        public DBRowList(DataSet dsData) 
        {
            if(dsData.Tables.Count == 0)
                this.Source = new List<int>();
            else
            {
                this.Source = dsData.Tables[0].Rows;
                m_nRows = dsData.Tables[0].Rows.Count;
            }
        }

        /****************************************************************************/
        public override int Count
        {
            get { return(m_nRows); }
        }
        
        /****************************************************************************/
        protected override IDataObjectSource GetObject(object objValue)
        {
            if(objValue is DataRowView)
                m_objRow.DataSource = (objValue as DataRowView).Row;
            else
                m_objRow.DataSource = objValue as DataRow;
            
            return(m_objRow);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class RegistryDataSourceList : DataSourceList
    {
        private List<RegistryKey> m_aSubKeys = null;
        
        /****************************************************************************/
        public RegistryDataSourceList(RegistryKey objParent) : base(null)
        {
            m_aSubKeys = new List<RegistryKey>();

            try
            {
                string[] aNames = objParent.GetSubKeyNames();

                foreach(string strName in aNames)
                {
                    try
                    {
                        m_aSubKeys.Add(objParent.OpenSubKey(strName, true));
                    }
                    catch(Exception ex)
                    {
                        cDebug.Capture(ex);
                    }
                }
            }
            catch(Exception ex)
            {
                cDebug.Capture(ex);
            }

            this.Source = m_aSubKeys;
        }

        /****************************************************************************/
        public RegistryDataSourceList(RegistryKey objParent, string strSubKey) : base(null)
        {
            m_aSubKeys = new List<RegistryKey>();

            try
            {
                RegistryKey objKey = objParent.OpenSubKey(strSubKey, true);
                string[]    aNames = objKey.GetSubKeyNames();

                foreach(string strName in aNames)
                {
                    try
                    {
                        m_aSubKeys.Add(objKey.OpenSubKey(strName, true));
                    }
                    catch(Exception ex)
                    {
                        cDebug.Capture(ex);
                    }
                }
            }
            catch(Exception ex)
            {
                cDebug.Capture(ex);
            }

            this.Source = m_aSubKeys;
        }

        /****************************************************************************/
        public override int Count
        {
            get { return(m_aSubKeys.Count); }
        }
        
        /****************************************************************************/
        protected override IDataObjectSource GetObject(object objValue)
        {
            return(new RegistryDataSource(objValue as RegistryKey));
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class XmlObjectList : DataSourceList
    {
        private XmlObjectSource m_objData = new XmlObjectSource(null);
        private int             m_nRows   = 0;

        /****************************************************************************/
        public XmlObjectList(XmlNodeList aNodes) : base(aNodes)
        {          
            m_nRows = aNodes.Count;
        }

        /****************************************************************************/
        public XmlObjectList(XmlNode objParent, string strXPath) : this(objParent.SelectNodes(strXPath))
        {
        }

        /****************************************************************************/
        public override int Count
        {
            get { return(m_nRows); }
        }
        
        /****************************************************************************/
        protected override IDataObjectSource GetObject(object objValue)
        {
            m_objData.DataSource = objValue as XmlNode;
            
            return(m_objData);
        }
    }
    
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Wraps an IDataObjectSource over on arbitrary object using reflection to pull properties
    /// </summary>
    public class DataObject : DataObjectSource
    {
        private object m_objSource;
        
        /****************************************************************************/
        public DataObject(object objSource)
        {
            m_objSource = objSource;
        }

        /****************************************************************************/
        public object DataSource
        {
            get {return(m_objSource);}
            set {m_objSource = value;}
        }

        /****************************************************************************/
        public override string Get(string strColumnName)
        {
            try
            {
                if(strColumnName == "")
                    return("");

                List<string> aNames    = StringList.ParseString(strColumnName, ".", true);
                object       objSource = m_objSource;
                
                foreach(string strName in aNames)
                {
                    Type         objType     = objSource.GetType();
                    PropertyInfo objProperty = objType.GetProperty(strName);

                    objSource = objProperty.GetValue(objSource, null);
                }
                    
                return(objSource.ToString());
            }
            catch
            {
                return("");
            }
        }        
        
        /****************************************************************************/
        public static string GetString(object objSource, string strColumnName)
        {
            try
            {
                List<string> aNames    = StringList.ParseString(strColumnName, ".", true);
                
                foreach(string strName in aNames)
                    objSource = objSource.GetType().GetProperty(strName).GetValue(objSource, null);
                    
                return(objSource.ToString());
            }
            catch
            {
                return("");
            }
        }

        /****************************************************************************/
        private void _SetValue(string strColumnName, object objValue)     
        {
            if(this.ReadOnly)
                return;
                
            try
            {
                List<string> aNames    = StringList.ParseString(strColumnName, ".", true);
                object       objSource = m_objSource;
                int          nNames    = aNames.Count-1;

                for(int i = 0; i < nNames; ++i)
                {
                    PropertyInfo objProperty = objSource.GetType().GetProperty(aNames[i]);
                    
                    objSource = objProperty.GetValue(objSource, null);
                }

                string       strPropName  = aNames[nNames];
                PropertyInfo objProperty2 = objSource.GetType().GetProperty(strPropName);
                System.Type  objType      = objProperty2.PropertyType;

                objValue = Utility.ConvertType(objValue, objType);

                if(objValue is IComparable)
                {
                    object objCurrentValue = Utility.ConvertType(Get(strColumnName), objType);

                    if((objValue as IComparable).CompareTo(objCurrentValue) == 0)
                        return;
                }

                this.Modified = true;

                objProperty2.SetValue(objSource, objValue, null);
            }
            catch(Exception ex)
            {
                cDebug.Capture(ex);
            }
                   
            return;
        }
        
        /****************************************************************************/
        public override void SetValue(string strColumnName, string value)       {_SetValue(strColumnName, value);}
        public override void SetValue(string strColumnName, int value)          {_SetValue(strColumnName, value);}
        public override void SetValue(string strColumnName, float value)        {_SetValue(strColumnName, value);}
        public override void SetValue(string strColumnName, long value)         {_SetValue(strColumnName, value);}
        public override void SetValue(string strColumnName, bool value)         {_SetValue(strColumnName, value);}
        public override void SetValue(string strColumnName, decimal value)      {_SetValue(strColumnName, value);}
        public override void SetValue(string strColumnName, double value)       {_SetValue(strColumnName, value);}
        public override void SetValue(string strColumnName, Guid value)         {_SetValue(strColumnName, value);}
        public override void SetValue(string strColumnName, DateTime value)     {_SetValue(strColumnName, value);}

        /****************************************************************************/
        public override IEnumerable Columns
        {
            get { return(null); }
        }
        
        /****************************************************************************/
        public override IDataObjectSource GetSubObject(string idSubObject)
        {
            return(new DataObject(m_objSource.GetType().GetProperty(idSubObject).GetValue(m_objSource, null)));
        }
        
        /****************************************************************************/
        public override DataSourceList GetSubList(string idSubList)
        {
            return(DataSourceList.Create(m_objSource.GetType().GetProperty(idSubList).GetValue(m_objSource, null)));
        }
    }
    
    /****************************************************************************/
    /****************************************************************************/
    public class DataObjectList : DataSourceList
    {
        private List<DataObject> m_aItems = new List<DataObject>();

        /****************************************************************************/
        public DataObjectList(IEnumerable aList) : base(aList)
        {          
            this.DataList = aList;
        }

        /****************************************************************************/
        protected IEnumerable DataList
        {
            set
            {
                m_aItems.Clear();

                if(value != null)
                    foreach(object objData in value)
                        m_aItems.Add(new DataObject(objData));
            }
        }

        /****************************************************************************/
        public override int Count
        {
            get { return(m_aItems.Count); }
        }

        /****************************************************************************/
        public override IEnumerator GetEnumerator()
        {
            return(m_aItems.GetEnumerator());
        }

        /****************************************************************************/
        protected override IDataObjectSource GetObject(object objValue)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
