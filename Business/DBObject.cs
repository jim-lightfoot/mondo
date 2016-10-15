/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Business							                */
/*             File: DBObject.cs						                    */
/*        Class(es): DBObject, DBObjectList						            */
/*          Purpose: Abstract classes for retrieving data from a database   */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 25 Apr 2006                                            */
/*                                                                          */
/*   Copyright (c) 2006-2008 - Tenth Generation Software, LLC               */
/*                              All rights reserved                         */
/*                                                                          */
/****************************************************************************/

using System;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.IO;

using Mondo.Common;
using Mondo.Database;
using Mondo.Xml;

namespace Mondo.Business
{
    /*************************************************************************/
    /*************************************************************************/
    public abstract class DBBase
    {
        private string   m_strConnection = "ConnectionString";
        protected string m_strTableName = "";

        /*************************************************************************/
        protected DBBase(string strTableName)
        {
            m_strTableName = strTableName;
        }

        /*************************************************************************/
        protected DBBase()
        {
        }

        /*************************************************************************/
        public virtual string TableName
        {
            get  {return(m_strTableName);}
            set  {m_strTableName = value;}
        }

        /*************************************************************************/
        public virtual string SelectTableName
        {
            get  {return(this.TableName);}
        }

        /*************************************************************************/
        public string ConnectionString 
        {
            get {return(m_strConnection);}
            set {m_strConnection = value;}
        }        

        /*************************************************************************/
        protected virtual Mondo.Database.Database CreateDatabase() 
        {
            return(Mondo.Database.Database.Create(m_strConnection, true));
        }

        /*************************************************************************/
        public string QueryString(DbCommand cmd)
        {
            using(Mondo.Database.Database db = CreateDatabase())
            {
                return(db.QueryString(cmd));
            }
        }

        /*************************************************************************/
        public uint QueryUint(DbCommand cmd)
        {
            using(Mondo.Database.Database db = CreateDatabase())
            {
                return(db.QueryUint(cmd));
            }
        }

        /*************************************************************************/
        public int QueryInt(DbCommand cmd)
        {
            using(Mondo.Database.Database db = CreateDatabase())
            {
                return(db.QueryInt(cmd));
            }
        }

        /*************************************************************************/
        public DataTable ExecuteTable(DbCommand cmd)
        {
            using(Mondo.Database.Database db = CreateDatabase())
            {
                return(db.ExecuteDataTable(cmd));
            }
        }

        /*************************************************************************/
        public DataSet ExecuteDataSet(DbCommand cmd)
        {
            using(Mondo.Database.Database db = CreateDatabase())
            {
                return(db.ExecuteDataSet(cmd));
            }
        }

        /*************************************************************************/
        public DataSet ExecuteDataSet(string strSQL)
        {
            return(ExecuteDataSet(strSQL));
        }

        /*************************************************************************/
        public DataTable ExecuteStoredProc(string strProcName)
        {
            using(Mondo.Database.Database db = CreateDatabase())
            {
                using(StoredProc sp = new StoredProc(db, strProcName))
                {
                    return(db.ExecuteDataTable(sp));
                }
            }
        }

        /*************************************************************************/
        public void ExecuteNonQuery(DbCommand cmd)
        {
            using(Mondo.Database.Database db = CreateDatabase())
            {
                db.ExecuteNonQuery(cmd);
            }
        }

        /*************************************************************************/
        public void ExecuteNonQuery(string strSQL)
        {
            using(Mondo.Database.Database db = CreateDatabase())
            {
                db.ExecuteNonQuery(strSQL);
            }
        }
    }

    /*************************************************************************/
    /*************************************************************************/
    public abstract class DBObject : DBBase 
    {
        private long m_id = 0;

        /*************************************************************************/
        protected DBObject(string strPrefix) : base(strPrefix)
        {
        }

        /*************************************************************************/
        protected DBObject()
        {
        }

        /*************************************************************************/
        public long ID 
        {
            get { return m_id; }
            set { m_id = value; }
        }

        /*************************************************************************/
        public virtual string Key 
        {
            get
            {
                return(m_id.ToString());
            }
        }

        /*************************************************************************/
        public void Load(IDataObjectSource objSource) 
        {
            FillData(objSource);
        }

        /*************************************************************************/
        public void Load() 
        {
            GetFromDB();
        }

        /*************************************************************************/
        public virtual string IDColumn 
        {
            get {return(this.TableName + "ID");}
        }

        /*************************************************************************/
        public virtual string GetSQL
        {
            get 
            {
                return(string.Format("SELECT * FROM dbo.[{0}] (NOLOCK) WHERE {1} = {2}", this.SelectTableName, this.IDColumn, this.ID));
            }
        }

        /*************************************************************************/
        public virtual XmlDocument GetXml
        {
            get 
            {
                using(Mondo.Database.Database db = CreateDatabase())
                {
                    using(DataSet ds = db.ExecuteDataSet(this.GetSQL))
                    {
                        return(XmlDoc.LoadXml(ds.GetXml()));
                    }
                }
            }
        }

        /*************************************************************************/
        public virtual string DeleteSQL
        {
            get 
            {
                return(string.Format("DELETE FROM dbo.[{0}] WHERE {1} = {2}", this.TableName, this.IDColumn, this.ID));
            }
        }

        /*************************************************************************/
        public class NotFound : Exception {}

        /*************************************************************************/
        /*************************************************************************/
        public class UniqueKeyViolation : Exception 
        {
            private string m_strKey;

            /*************************************************************************/
            public UniqueKeyViolation(DbException ex) : base("Violation of unique key", ex)
            {
                m_strKey = ex.Message.ToLower();

                m_strKey = cString.StripUpTo(m_strKey, "constraint '");
                m_strKey = cString.StripAfter(m_strKey, "'");
            }

            /*************************************************************************/
            public string Key {get{return(m_strKey);}}
        }

        /*************************************************************************/
        public static string MakeDisplay(string strValue)
        {
            return(strValue.Replace("\r\n", "<br/>"));
        }

        /*************************************************************************/
        protected virtual void GetFromDB() 
        {
            GetFromDB(this.GetSQL);
        }

        /*************************************************************************/
        protected virtual void GetFromDB(string strSQL) 
        {
            using(Mondo.Database.Database db = CreateDatabase())
            {
                _Load(db, db.MakeSelectCommand(strSQL));
            } 
        }

        /*************************************************************************/
        protected virtual void Load(DbCommand cmd) 
        {
            using(Mondo.Database.Database db = CreateDatabase())
            {
                _Load(db, cmd);
            }           
        }

        /*************************************************************************/
        private void _Load(Mondo.Database.Database db, DbCommand cmd) 
        {
            try
            {
                using(DataSet ds = db.ExecuteDataSet(cmd))
                {
                    if(ds.Tables == null || ds.Tables.Count == 0 || ds.Tables[0].Rows == null || ds.Tables[0].Rows.Count == 0)
                        throw new NotFound();

                    FillData(new DBRow(ds));

                    int nTables = ds.Tables.Count;

                    for(int i = 1; i < nTables; ++i)
                        LoadTable(ds.Tables[i], i);
                }

                return;
            }
            catch(DbException ex)
            {
                string strMessage = ex.Message.ToLower();

                if(strMessage.Contains("there is no row at position"))
                    throw new NotFound();

                throw;
            }
        }

        /*************************************************************************/
        protected virtual void LoadTable(DataTable dt, int iIndex)
        {
            // Do nothing in base class
        }

        /*************************************************************************/
        protected static IEnumerable<long> ReadIDList(DataTable dt)
        {
            return(ReadIDList(dt, "ID"));
        }

        /*************************************************************************/
        protected static IEnumerable<long> ReadIDList(DataTable dt, string idColumn)
        {
            List<long> aList = new List<long>();

            foreach(DataRow objRow in dt.Rows)
                aList.Add(DBRow.GetColInt(objRow, idColumn));

            return(aList);
        }

        /*************************************************************************/
        protected void Load(StoredProc cmd) 
        {
            Load(cmd.Command);      
        }

        /*****************************************************************************/
        protected virtual void FillData(IDataObjectSource objSource)
        {
            Type           objType       = this.GetType();
            PropertyInfo[] aProperties   = objType.GetProperties();
            string         strIdProperty = this.IDColumn;

            foreach(PropertyInfo objProperty in aProperties)
            {
                string idField = objProperty.Name;

                if(idField.StartsWith("pa_"))
                {
                    idField = idField.Substring(3);

                    object objValue = objProperty.GetValue(this, null);

                    if(objValue is IPersistAttribute)
                        FillProperty(objProperty, idField, objValue as IPersistAttribute, objSource);
                }
            }

            this.ID = objSource.GetInt(this.IDColumn);

            return;
        }

        /*****************************************************************************/
        protected virtual void FillProperty(PropertyInfo objProperty, string idField, IPersistAttribute objAttribute, IDataObjectSource objSource)
        {
            objAttribute.SetValue(objSource, idField);
        }

        /*****************************************************************************/
        protected virtual SQLWriter CreateWriter()
        {
            if(this.ID == 0)
                return(new InsertWriter(this));

            return(new UpdateWriter(this));
        }

        /*****************************************************************************/
        public virtual void Save()
        {
            Save(null, null);
        }

        /*****************************************************************************
         * Riders are extra SQL statements run as part of the save within the same
         *   transaction.
         *****************************************************************************/
        public virtual void Save(IEnumerable aRiders, IEnumerable<DbParameter> aParameters)
        {
            using(Mondo.Database.Database db = CreateDatabase())
            {
                SQLWriter objWriter  = CreateWriter();
                DbCommand objCommand = null;

                try
                {
                    objCommand = objWriter.MakeCommand(db);
                }
                catch(SQLWriter.NoProps)
                {
                }

                // No properties to update and no riders to execute
                if(objCommand == null && aRiders == null)
                    return;

                StringBuilder sb = new StringBuilder();

                if(aParameters != null)
                {
                    if(objCommand == null)
                    {
                        objCommand = db.MakeCommand();

                        sb.Insert(0, string.Format("SELECT @idObject = {0}\r\n\r\n", this.ID));
                        sb.Insert(0, "DECLARE @idObject int\r\n\r\n");
                    }

                    foreach(DbParameter objParameter in aParameters)
                        objCommand.Parameters.Add(objParameter);
                }

                StringList aParts = null;
            
                if(aRiders != null)
                {
                    if(aRiders is StringList)
                        aParts = aRiders as StringList;
                    else
                        aParts = new StringList(aRiders);

                    aParts.Pack(sb, "IF(@@ERROR = 0)\r\n  BEGIN\r\n    {0}\r\n  END", "\r\n\r\n");
                }

                // Just in case an empty list was passed in
                if(objCommand == null && sb.Length == 0)
                    return;

                if(objCommand != null)
                    sb.Insert(0, objCommand.CommandText + "\r\n");

                if(aParts != null && aParts.Count > 0)
                {
                    if(objCommand != null)
                        BuildTransaction(sb, "    SELECT @idObject\r\n");
                    else
                        BuildTransaction(sb, "");
                }
                else
                    sb.Append("\r\nIF(@@ERROR = 0) SELECT @idObject");

                try
                {
                    if(objCommand != null)
                    {
                        objCommand.CommandText = sb.ToString();

                        if(objWriter is InsertWriter)
                            this.ID = db.QueryInt(objCommand);
                        else
                            db.ExecuteNonQuery(objCommand);
                    }
                    else
                    {
                        sb.Insert(0, string.Format("SELECT @idObject = {0}\r\n\r\n", this.ID));
                        sb.Insert(0, "DECLARE @idObject int\r\n\r\n");
                        db.ExecuteNonQuery(sb.ToString());
                    }
                }
                catch(DbException ex)
                {
                    string strMessage = ex.Message.ToLower();

                    if(strMessage.Contains("unique key"))
                        throw new UniqueKeyViolation(ex);

                    throw;
                }
                finally
                {
                    if(objCommand != null)
                        objCommand.Dispose();
                }
            }

            return;
        }

        #region SQL Writers

        /*****************************************************************************/
        public abstract class SQLWriter
        {
            protected DBObject m_objBiz;
            private readonly OrderedDictionary<string, object> m_aParameters = new OrderedDictionary<string,object>(17);

            /*****************************************************************************/
            public SQLWriter(DBObject objBiz)
            {
                m_objBiz = objBiz;
            }

            /*****************************************************************************/
            protected OrderedDictionary<string, object> Parameters 
            {
                get {return(m_aParameters);}
            }

            /*****************************************************************************/
            public DbCommand MakeCommand(Mondo.Database.Database db)
            {
                Type           objType       = m_objBiz.GetType();
                PropertyInfo[] aProperties   = objType.GetProperties();
                StringBuilder  sb            = new StringBuilder();

                WriteProperties(sb, objType, aProperties);

                DbCommand objCommand = db.MakeSelectCommand(sb.ToString());

                objCommand.CommandType = CommandType.Text;

                foreach(string strKey in m_aParameters.Keys)
                    objCommand.Parameters.Add(db.CreateParameter(strKey, m_aParameters[strKey]));

                return(objCommand);
            }

            /*****************************************************************************/
            public class NoProps : Exception {}

            /*****************************************************************************/
            protected virtual void WriteProperties(StringBuilder sb, Type objType, PropertyInfo[] aProperties)
            {
                string strIdProperty = objType.Name + "ID";

                foreach(PropertyInfo objProperty in aProperties)
                {
                    string idField = objProperty.Name;

                    if(idField.StartsWith("pa_"))
                    {
                        idField = idField.Substring(3);

                        if(idField != strIdProperty)
                        {
                            object objValue = objProperty.GetValue(m_objBiz, null);

                            if(objValue is IPersistAttribute)
                            {
                                IPersistAttribute objAttribute = objValue as IPersistAttribute;

                                WriteProperty(sb, objProperty, idField, objAttribute);
                            }
                        }
                    }
                }
            }

            /*****************************************************************************/
            protected abstract void WriteProperty(StringBuilder sb, PropertyInfo objProperty, string idField, IPersistAttribute objAttribute);

            /*****************************************************************************/
            protected virtual object GetValue(IPersistAttribute objAttribute)
            {
                string strValue    = objAttribute.ToString();
                Type   objPropType = objAttribute.ValueType;

                if(objPropType.Equals(typeof(DateTime)))
                {
                    MemberAttribute<DateTime> dtDate = objAttribute as MemberAttribute<DateTime>;

                    if(dtDate.Value == DateTime.MinValue)
                        return(DBNull.Value);
                    
                    return(dtDate.Value);
                }

                if(objPropType.Equals(typeof(bool)))
                    return(Utility.ToBool(strValue));

                if(objAttribute.IsNull)
                   return(DBNull.Value);

                return(strValue);
            }
        }

        /*****************************************************************************/
        public class InsertWriter : SQLWriter
        {
            private StringList m_aNames  = null;
            private StringList m_aValues = null;

            /*****************************************************************************/
            public InsertWriter(DBObject objBiz) : base(objBiz)
            {
            }

            /*****************************************************************************/
            protected override void WriteProperties(StringBuilder sb, Type objType, PropertyInfo[] aProperties)
            {
                m_aNames  = new StringList();
                m_aValues = new StringList();

                sb.Append("DECLARE @idObject int\r\n\r\n");
                sb.AppendFormat("INSERT INTO dbo.[{0}]\r\n       (", m_objBiz.TableName);

                base.WriteProperties(sb, objType, aProperties);

                if(m_aNames.Count == 0)
                    throw new Exception("No properties to save");

                sb.Append(m_aNames.Pack(", ") + ")\r\nVALUES (" + m_aValues.Pack(", ") + ")\r\n\r\n");
                sb.Append("IF(@@ERROR = 0 ) SELECT @idObject = SCOPE_IDENTITY()\r\n\r\n");
            }

            /*****************************************************************************/
            protected override void WriteProperty(StringBuilder sb, PropertyInfo objProperty, string idField, IPersistAttribute objAttribute)
            {
                if(objAttribute.PersistType != PersistType.Transient && objAttribute.PersistType != PersistType.ReadOnly)
                {
                    m_aNames.Add(string.Format("[{0}]", idField));
                    m_aValues.Add("@" + idField);
                    this.Parameters.Add("@" + idField, GetValue(objAttribute));
                }
            }
        }

        /*****************************************************************************/
        public class UpdateWriter : SQLWriter
        {
            private StringList m_aList = null;
            
            /*****************************************************************************/
            public UpdateWriter(DBObject objBiz) : base(objBiz)
            {
            }

            /*****************************************************************************/
            protected override void WriteProperties(StringBuilder sb, Type objType, PropertyInfo[] aProperties)
            {
                m_aList = new StringList();

                sb.Append("DECLARE @idObject int\r\n\r\n");
                sb.AppendFormat("SELECT @idObject = {0}\r\n\r\n", m_objBiz.ID);
                sb.AppendFormat("UPDATE dbo.[{0}]\r\nSET    ", m_objBiz.TableName);

                base.WriteProperties(sb, objType, aProperties);

                if(m_aList.Count == 0)
                    throw new NoProps();

                sb.Append(m_aList.Pack(",\r\n       "));
                sb.AppendFormat("\r\nWHERE {0} = {1}\r\n\r\n", this.m_objBiz.IDColumn, m_objBiz.ID);
            }

            /*****************************************************************************/
            protected override void WriteProperty(StringBuilder sb, PropertyInfo objProperty, string idField, IPersistAttribute objAttribute)
            {
                // objProperty.CanWrite
                if(objAttribute.PersistType == PersistType.Normal && objAttribute.Modified)
                {
                    m_aList.Add(string.Format("[{0}] = @{0}", idField));
                    this.Parameters.Add("@" + idField, GetValue(objAttribute));
                }
            }
        }

        #endregion

        #region Utility Functions

        /*************************************************************************/
        private void BuildTransaction(StringBuilder sb, string strCommit)
        {
            sb.Insert(0, "BEGIN TRANSACTION\r\n");

            sb.Append("\r\n\r\nIF(@@ERROR = 0)\r\n  BEGIN\r\n    COMMIT TRANSACTION\r\n");
            sb.Append(strCommit);
            sb.Append("END\r\nELSE\r\n    ROLLBACK TRANSACTION\r\n");
        }

        #endregion

        #region XML Writers

        /*************************************************************************/
        public XmlDocument ToXml()
        {
            return(ToXml("Mondo"));
        }

        /*************************************************************************/
        public virtual XmlDocument ToXml(string strRoot)
        {
            cXMLWriter objWriter = new cXMLWriter();

            objWriter.WriteStartDocument();

                using(new XmlElementWriter(objWriter, strRoot))
                    Write(objWriter);

            objWriter.WriteEndDocument();

            return(objWriter.Xml);
        }

        /*************************************************************************/
        public virtual void Write(cXMLWriter objWriter)
        {
            using(new XmlElementWriter(objWriter, this.GetType().Name))
                WriteProperties(objWriter);
        }

        /*************************************************************************/
        protected virtual void WriteProperties(cXMLWriter objWriter)
        {
            Type           objType       = this.GetType();
            PropertyInfo[] aProperties   = objType.GetProperties();

            foreach(PropertyInfo objProperty in aProperties)
            {
                string idField = objProperty.Name;

                if(idField.StartsWith("pa_"))
                {
                    object objValue = objProperty.GetValue(this, null);

                    if(objValue is IPersistAttribute)
                    {
                        IPersistAttribute objAttribute = objValue as IPersistAttribute;
                        string            strValue     = objAttribute.ToString();
                        Type              objPropType  = objAttribute.ValueType;

                        if(objPropType.Equals(typeof(DateTime)))
                        {
                            MemberAttribute<DateTime> dtDate = objAttribute as MemberAttribute<DateTime>;

                            if(dtDate.Value == DateTime.MinValue)
                                strValue = "";
                            else 
                                strValue = dtDate.Value.ToString();
                        }
                        else if(objPropType.Equals(typeof(bool)))
                            strValue = Utility.ToBool(strValue) ? "1" : "0";
                        else if(objAttribute.IsNull)
                            strValue = "";

                        idField = idField.Substring(3);

                        objWriter.WriteElementString(idField, strValue);
                    }
                }
            }
        }

        #endregion

        /*************************************************************************/
        public virtual void Delete()
        {
            using(Mondo.Database.Database db = CreateDatabase())
            {
                db.ExecuteNonQuery(this.DeleteSQL);
            }
        }

        /*************************************************************************/
        protected void RunObjectNonQueryProc(string strProcName)
        {
            using(Mondo.Database.Database db = CreateDatabase())
            {
                using(StoredProc sp = db.CreateStoredProc(strProcName))
                {
                    DbParameter objParameter = db.CreateParameter("@idObject", DbType.Int64);

                    objParameter.Value = this.ID;
                    sp.Parameters.Add(objParameter);

                    db.ExecuteNonQuery(sp);
                }
            }
        }
    }

    /*************************************************************************/
    /*************************************************************************/
    public abstract class DBList : DBBase
    {
        /*************************************************************************/
        protected DBList(string strTableName) : base(strTableName) 
        {
        }

        /*************************************************************************/
        public static void QueryDatabase(StoredProc sp, IList<DBList> aParams)
        {
            if(aParams.Count == 0)
                return;

            DataSet ds = null;

            // Run a query against a database that returns multiple datasets (tables)
            using(Mondo.Database.Database db = aParams[0].CreateDatabase())
            {
                ds = db.ExecuteDataSet(sp);
            }

            try
            {
                int iIndex = 0;

                // For each dataset (table) returned fill the matching DBList with that data
                foreach(DataTable dt in ds.Tables)
                {
                    if(aParams.Count <= iIndex)
                        break;

                    aParams[iIndex].FillData(dt);

                    ++iIndex;
                }
            }
            finally
            {
                ds.Dispose();
            }

            return;
        }

        /*************************************************************************/
        public abstract void FillData(DataTable objTable);
    }

    /*************************************************************************/
    /*************************************************************************/
    public abstract class DBObjectList<T> : DBList, IList<T>, IList where T : DBObject, new()
    {
        protected List<T>               list = new List<T>();
        protected Dictionary<string, T> dict = new Dictionary<string, T>();

        /*************************************************************************/
        protected DBObjectList(string strTableName) : base(strTableName) 
        {
        }

        /*************************************************************************/
        public virtual string Fields
        {
            get 
            {
                return("*");
            }
        }

        /*************************************************************************/
        public virtual string GetSQL
        {
            get 
            {
                return(string.Format("SELECT {0} FROM dbo.[{1}] (NOLOCK)", this.Fields, this.TableName));
            }
        }

        /*************************************************************************/
        public int Count 
        {
            get { return list.Count; }
        }
         
        /*************************************************************************/
        public virtual void Add(T objAdd)
        {
            objAdd.ConnectionString = this.ConnectionString;
            list.Add(objAdd);

            try
            {
                dict.Add(objAdd.Key, objAdd);
            }
            catch
            {
            }
        }

        /*************************************************************************/
        public bool Remove(T objRemove)
        {
            if(dict.ContainsKey(objRemove.Key))
            {
                list.Remove(objRemove);
                dict.Remove(objRemove.Key);
                return(true);
            }

            return(false);
        }

        /*************************************************************************/
        public T Get(int index)        
        {
            return(this[index]);
        }

        /*************************************************************************/
        public T this[string objKey]    {get{return (dict[objKey]);}}

        /*************************************************************************/
        public T this[int index]        
        {
            get {return(list[index]);}
            set {}
        }
        
        /*************************************************************************/
        public IEnumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }
        
        /*************************************************************************/
        public bool ContainsKey(string strKey)
        {
            return(dict.ContainsKey(strKey));
        }
        
        /*************************************************************************/
        public void Sort() 
        {
            list.Sort();
        }

        /*************************************************************************/
        public void Load()
        {
            GetDataFromDB();
        }

        /*************************************************************************/
        protected DataSet AllRecords
        {
            get
            {
                return(Query(this.GetSQL));
            }
        }

        /*************************************************************************/
        public DataSet Query(string strSQL)
        {
            using(Mondo.Database.Database db = CreateDatabase())
            {
                return(db.ExecuteDataSet(strSQL));
            }
        }

        /*************************************************************************/
        protected virtual void GetDataFromDB() 
        {
            GetDataFromDB(this.GetSQL);
        }

        /*************************************************************************/
        protected virtual void GetDataFromDB(string strSQL) 
        {
            using(Mondo.Database.Database db = CreateDatabase())
            {
                using(DbCommand cmd = db.MakeSelectCommand(strSQL))
                {
                    _GetDataFromDB(db, cmd);
                }
            }
        }

        /*************************************************************************/
        protected void GetDataFromDB(StoredProc sp) 
        {
            if(sp.Database != null)
                _GetDataFromDB(sp.Database, sp.Command);
            else
                GetDataFromDB(sp.Command);
        }

        /*************************************************************************/
        protected virtual void GetDataFromDB(DbCommand cmd) 
        {
            using(Mondo.Database.Database db = CreateDatabase())
            {
                _GetDataFromDB(db, cmd);
            }
        }

        /*************************************************************************/
        private void _GetDataFromDB(Mondo.Database.Database db, DbCommand cmd) 
        {
            using(DataTable dt = db.ExecuteDataTable(cmd))
            {               
                // check for result
                if(dt.Rows.Count == 0) 
                    return;

                FillData(dt);
            }
        }

        /*************************************************************************/
        public override void FillData(DataTable objTable) 
        {
            if(objTable != null && objTable.Rows != null)
                foreach(DataRow row in objTable.Rows) 
                    Add(CreateObject(new DBRow(row)));
        }

        /*************************************************************************/
        public void FillData(DataRow[] aRows) 
        {
            if(aRows != null)
                foreach(DataRow row in aRows) 
                    Add(CreateObject(new DBRow(row)));
        }

        /*************************************************************************/
        protected virtual T CreateObject(IDataObjectSource objSource)
        {
            T objNew = new T();

            objNew.Load(objSource);

            return(objNew);
        }

        /*************************************************************************/
        public virtual void Save()
        {
            foreach(T objItem in this)
                SaveItem(objItem);
        }

        /*************************************************************************/
        protected virtual void SaveItem(T objItem)
        {
            objItem.Save();
        }

        #region ICollection Members

        /****************************************************************************/
        public void CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        /****************************************************************************/
        public bool IsSynchronized
        {
            get { return(false); }
        }

        /****************************************************************************/
        public object SyncRoot
        {
            get { return(null); }
        }

        #endregion

        #region IList<T> Members

        /****************************************************************************/
        public int IndexOf(T item)
        {
            return(list.IndexOf(item));
        }

        /****************************************************************************/
        public void Insert(int index, T item)
        {
            dict.Add(item.Key, item);
            list.Insert(index, item);
        }

        /****************************************************************************/
        public void RemoveAt(int index)
        {
            T item = list[index];

            dict.Remove(item.Key);
            list.RemoveAt(index);
        }

        #endregion

        #region ICollection<T> Members

        /****************************************************************************/
        public void Clear()
        {
            list.Clear();
            dict.Clear();
        }

        /****************************************************************************/
        public bool Contains(T item)
        {
            return(dict.ContainsKey(item.Key));
        }

        /****************************************************************************/
        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /****************************************************************************/
        public bool IsReadOnly
        {
            get { return(false); }
        }

        #endregion

        #region IEnumerable<T> Members

        /****************************************************************************/
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return(list.GetEnumerator());
        }

        #endregion

        #region IList Members

        public int Add(object value)
        {
            if(value is T)
            {
                Add(value as T);
                return(this.Count-1);
            }

            return(-1);
        }

        public bool Contains(object value)
        {
            if(value is T)
                return(Contains(value as T));

            return(false);
        }

        public int IndexOf(object value)
        {
            if(value is T)
                return(IndexOf(value as T));

            return(-1);
        }

        public void Insert(int index, object value)
        {
            if(value is T)
                Insert(index, value as T);
        }

        public bool IsFixedSize
        {
            get { return(false); }
        }

        public void Remove(object value)
        {
            if(value is T)
                Remove(value as T);
        }

        object IList.this[int index]
        {
            get {return(Get(index)); }
            set {}
        }

        #endregion
    }
}