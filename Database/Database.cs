/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Database							                */
/*             File: Database.cs										    */
/*        Class(es): Database										        */
/*          Purpose: A connection to a database                             */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 12 Sep 2001                                            */
/*                                                                          */
/*   Copyright (c) 2001-2017 - Jim Lightfoot, All rights reserved           */
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
using System.Security;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Mondo.Common;
using Mondo.Xml;

namespace Mondo.Database
{
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
	/// A connection string for a database
	/// </summary>
	public class ConnectionString 
	{
        private string m_connectionString;
        private string m_type = "";

        /****************************************************************************/
        public ConnectionString(string connectionString, string type)
        {
            m_connectionString = connectionString;
            m_type = type;
        }

        /****************************************************************************/
        public ConnectionString(string connectionString)
        {
            m_connectionString = connectionString;
        }

        /****************************************************************************/
        public static string Format(IConfig config, string connectionString, bool bLookup)
        {
            if(connectionString.Normalized() == "")
            {
                connectionString = "ConnectionString";
                bLookup = true;
            }

            if(bLookup)
                connectionString = Lookup(config, connectionString);

             return(connectionString);
        }        
        
        /****************************************************************************/
        public static string Normalize(string connectionString)
        {
            connectionString = connectionString.Replace("mondo=oledb", "");
            connectionString = connectionString.Replace("mondo=odbc", "");
            connectionString = connectionString.Replace(";;", ";");

            if(connectionString.StartsWith(";"))
                connectionString = connectionString.Substring(1);

            return(connectionString);
        }

        /*************************************************************************/
        /// <summary>
        /// Lookup the actual connection string in the web.config
        /// </summary>
        /// <param name="connectionStringParam"></param>
        public static string Lookup(IConfig config, string connectionStringParam)
        {
            return(config.GetConnectionString(connectionStringParam));
        }

        /****************************************************************************/
        public string Type
        {
            get {return(m_type);}
        }

        /****************************************************************************/
        public override string ToString()
        {
            return(m_connectionString);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
	/// A connection to a database
	/// </summary>
	public abstract class Database : Openable
	{
        protected string                    _connectionString = "";
        private DbConnection                _connection       = null;
        private DbTransaction               _transaction      = null;
        private readonly DbProviderFactory  _factory;
        private readonly RetryPolicy        _retryPolicy      = new DatabaseRetryPolicy();

        private static string[] _aOLEDBProviders = new string[] {"msdaora", "oledb", "sqlncl", "mysqlprov"};
        private static string[] _aODBCProviders  = new string[] {"odbc", "sql server native client", "sql native client", "driver={mysql}", "orahome92"};

        /*************************************************************************/
        /// <summary>
        /// Construct the database with a static connection string.
        /// </summary>
        protected Database(DbProviderFactory factory, IConfig config) : this(factory, config, "", true)
        {
        }
        
        /*************************************************************************/
        /// <summary>
        /// Construct the database with the given connection string name.
        /// </summary>
        /// <param name="connectionString">The name of the connection string. The actual connection string is in the ConnectionStrings config section in the web.config. (if bLookup is true)</param>
        /// <param name="bLookup">If true then connectionString is alias to a value in the web.config, otherwise it's the actual connection string</param>
        protected Database(DbProviderFactory factory, IConfig config, string connectionString, bool bLookup)
        {
            _factory            = factory;
            _connectionString   = ConnectionString.Format(config, connectionString, bLookup);
            this.CommandTimeout = config.Get<int>("CommandTimeout", 60);
        }

        /*************************************************************************/
        public int CommandTimeout
        {
            set;
            get;
        }

        /*************************************************************************/
        public RetryPolicy RetryPolicy
        {
            get { return(_retryPolicy); }
        }

        /*************************************************************************/
        public static Database Create(IConfig config = null)
        {
            return(Create("", true, config));
        }

        /*************************************************************************/
        public static Database Create(string connectionString, IConfig config = null)
        {
            return(Create(connectionString, false, config));
        }

       /*************************************************************************/
        public static Database Create(string connectionString, bool bLookup, IConfig config = null)
        {
            if(config == null)
                config = new AppConfig();

            connectionString = ConnectionString.Format(config, connectionString, bLookup);
         
            if(IsIn(connectionString, _aOLEDBProviders))
                return(new OleDbDatabase(config, connectionString, false));

            if(IsIn(connectionString, _aODBCProviders))
                return(new OdbcDatabase(config, connectionString, false));

            return(new SqlDatabase(config, connectionString, false));
        }

        /****************************************************************************/
        public override void Open()
        {          
            try
            {   
                if(!IsOpen)
                {
                    _connection = _factory.CreateConnection();

                    try
                    {
                        _connection.ConnectionString = _connectionString;
                    }
                    catch(Exception ex)
                    {
                        throw new DatabaseException("Invalid connection string format", ex);
                    }

                    _connection.Open();
                }

                base.Open();                
            }
            catch(Exception ex)
            {
               throw ex;
            }
        }

        /****************************************************************************/
        public async override Task OpenAsync()
        {          
            try
            {   
                if(!IsOpen)
                {
                    _connection = _factory.CreateConnection();

                    try
                    {
                        _connection.ConnectionString = _connectionString;
                    }
                    catch(Exception ex)
                    {
                        throw new DatabaseException("Invalid connection string format", ex);
                    }

                    await _connection.OpenAsync();
                }

                await base.OpenAsync();                
            }
            catch(Exception ex)
            {
               throw ex;
            }
        }

        /****************************************************************************/
        public override void Close()
        {
            base.Close();

            if(!IsOpen)
            {
                try
                {
                    Rollback();
                    _connection.Close();
                    _connection.Dispose();
                    _connection = null;
                }
                catch
                {
                }
            }
         }

        /****************************************************************************/
        public DbConnection Connection
        {
            get
            {
                return(_connection);
            }
        }

        /*************************************************************************/
        public DbTransaction Transaction
        {
            get {return(_transaction); }
        }

        /*************************************************************************/
        public void BeginTransaction()
        {
            _transaction = _connection.BeginTransaction();
        }

        /*************************************************************************/
        public void Commit()
        {
            if(_transaction != null)
            {
                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;
            }
        }

        /*************************************************************************/
        public void Rollback()
        {
            if(_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }
        }

        /*************************************************************************/
        public StoredProc CreateStoredProc(string strName)
        {
            return new StoredProc(this, strName);
        }

        /*************************************************************************/
        public StoredProc StoredProc(string strName)
        {
            return new StoredProc(this, strName);
        }

        /****************************************************************************/
        public DbCommand MakeSelectCommand(string strSelect)
        {
            DbCommand objCommand = MakeCommand();

            objCommand.CommandText = strSelect;
            objCommand.CommandType = CommandType.Text;            

            return(objCommand);
        }

        /****************************************************************************/
        public DbCommand MakeCommand(StoredProc sp)
        {
            DbCommand objCommand = MakeCommand();

            objCommand.CommandText    = sp.Name;
            objCommand.CommandType    = CommandType.StoredProcedure;            
            objCommand.CommandTimeout = this.CommandTimeout;            

            return(objCommand);
        }

        /****************************************************************************/
        public DbCommand MakeCommand()
        {
            DbCommand objCommand = _factory.CreateCommand();

            if(_transaction != null)
                objCommand.Transaction = _transaction;

            return(objCommand);
        }

        #region CreateParameter

        /****************************************************************************/
        public DbParameter CreateParameter(string             parameterName, 
                                           DbType             dbType, 
                                           int                size, 
                                           ParameterDirection direction, 
                                           bool               isNullable, 
                                           byte               precision, 
                                           byte               scale, 
                                           string             sourceColumn, 
                                           DataRowVersion     sourceVersion, 
                                           object             value)
        {        
            DbParameter objParameter = _factory.CreateParameter();

            objParameter.ParameterName              = parameterName;
            objParameter.DbType                     = dbType;
            objParameter.Direction                  = direction;
            objParameter.SourceColumnNullMapping    = isNullable;
            objParameter.SourceColumn               = sourceColumn;
            objParameter.SourceVersion              = sourceVersion;
            objParameter.Value                      = value;

            if(size > 0)
                objParameter.Size = size;

            return(objParameter);
        }

        /****************************************************************************/
        public DbParameter CreateParameter(string parameterName, 
                                           DbType dbType)
        {        
            DbParameter objParameter = _factory.CreateParameter();

            objParameter.ParameterName              = parameterName;
            objParameter.DbType                     = dbType;
            objParameter.Direction                  = ParameterDirection.Input;
            objParameter.SourceColumnNullMapping    = true;
            objParameter.SourceVersion              = DataRowVersion.Default;

            return(objParameter);
        }

        /****************************************************************************/
        public DbParameter CreateParameter(string parameterName, 
                                           object data)
        {        
            DbParameter objParameter = _factory.CreateParameter();

            objParameter.ParameterName = parameterName;
            objParameter.Value         = data;

            return(objParameter);
        }

        #endregion

        #region ExecuteSelect 

        /****************************************************************************/
        public DbDataReader ExecuteSelect(string strSelect, CommandBehavior eBehavior = CommandBehavior.Default)
        {
            DbCommand objCommand = MakeSelectCommand(strSelect);

            return(ExecuteSelect(objCommand, eBehavior));
        }

        /************************************************************************/
        public async Task<DbDataReader> ExecuteSelectAsync(string strSelect, CommandBehavior eBehavior = CommandBehavior.Default)
        {
            DbCommand objCommand = MakeSelectCommand(strSelect);

            return await ExecuteSelectAsync(objCommand, eBehavior);
        }

        /****************************************************************************/
        public async Task<DbDataReader> ExecuteSelectAsync(DbCommand cmd, CommandBehavior eBehavior = CommandBehavior.Default)
        {
            return await ExecuteAsync(cmd, async (dbCommand)=>
            {
                return await dbCommand.ExecuteReaderAsync(eBehavior);
            });
        }
        
        /****************************************************************************/
        public DbDataReader ExecuteSelect(DbCommand cmd, CommandBehavior eBehavior = CommandBehavior.Default)
        {
            return Execute<DbDataReader>(cmd, (dbCommand)=>
            {
                return dbCommand.ExecuteReader(eBehavior);
            });
        }

        #endregion

        #region ExecuteDataSet 

        /****************************************************************************/
        public DataSet ExecuteDataSet(string strSelect)
        {
            using(DbCommand cmd = MakeSelectCommand(strSelect))
            {
                return(ExecuteDataSet(cmd));
            }
        }

        /****************************************************************************/
        public DataSet ExecuteDataSet(Operation sp)
        {
            return(ExecuteDataSet(sp.Command));
        }

        /****************************************************************************/
        public DataSet ExecuteDataSet(DbCommand cmd)
        {
            return Execute<DataSet>(cmd, (dbCommand)=>
            {
                DataSet dataSet = null;

                using(DbDataAdapter objAdapter = _factory.CreateDataAdapter())
                {
                    objAdapter.SelectCommand = dbCommand;

                    dataSet = new DataSet("_data");
                    dataSet.EnforceConstraints = false; 

                    objAdapter.Fill(dataSet);
                }

                return dataSet;
            });
        }

        #endregion

        #region ExecuteForXml 

        /****************************************************************************/
        public XmlDocument ExecuteForXml(Operation sp)
        {
             return(ExecuteForXml(sp.Command));
        }

        /****************************************************************************/
        public XmlDocument ExecuteForXml(DbCommand cmd)
        {
            return Execute<XmlDocument>(cmd, (dbCommand)=>
            {
                using(XmlReader objReader = (dbCommand as SqlCommand).ExecuteXmlReader())
                {
                    while(objReader.Read())
                        return XmlDoc.Load(objReader);
                }        
                
                return null;   
            });
        }

        #endregion

        /****************************************************************************/
        public DBRow ExecuteSingleRow(string strSelect) {return(new DBRow(ExecuteDataTable(strSelect)));}
        public DBRow ExecuteSingleRow(DbCommand cmd)    {return(new DBRow(ExecuteDataTable(cmd)));}
        public DBRow ExecuteSingleRow(Operation sp)     {return(new DBRow(ExecuteDataTable(sp)));}

        /****************************************************************************/
        public void ExecuteNonQuery(DbCommand cmd)   {DoTask(cmd);}
        public void ExecuteNonQuery(Operation sp)    {DoTask(sp);}
        public void ExecuteNonQuery(string strSQL)   {DoTask(strSQL);}

        #region ExecuteDataSourceList Methods

        /****************************************************************************/
        public DataSourceList ExecuteDataSourceList(string strSelect)
        {
            return(new DBRowList(ExecuteDataTable(strSelect)));
        }

        /****************************************************************************/
        public DataSourceList ExecuteDataSourceList(Operation sp)
        {
            return(new DBRowList(ExecuteDataTable(sp)));
        }

        /****************************************************************************/
        public DataSourceList ExecuteDataSourceList(DbCommand objCommand)
        {
            return(new DBRowList(ExecuteDataTable(objCommand)));
        }

        #endregion

        #region ExecuteDataTable

        /****************************************************************************/
        public DataTable ExecuteDataTable(string strSelect)
        {
            using(DbCommand objCommand = MakeSelectCommand(strSelect))
            {
                return(ExecuteDataTable(objCommand));
            }
        }

        /****************************************************************************/
        public DataTable ExecuteDataTable(Operation sp)
        {
            return(ExecuteDataTable(sp.Command));
        }

        /****************************************************************************/
        public DataTable ExecuteDataTable(DbCommand cmd)
        {
            return Execute<DataTable>(cmd, (dbCommand)=>
            {
                DataTable dataTable = null;

                using(DbDataAdapter objAdapter = _factory.CreateDataAdapter())
                {
                    objAdapter.SelectCommand = dbCommand;

                    dataTable = new DataTable("_data");

                    objAdapter.Fill(dataTable);
                }

                return(dataTable);
            });
        }

        #endregion

        #region ExecuteList Methods

        /****************************************************************************/
        public IList<T> ExecuteList<T>(string strSelect) where T : new()
        {
            return ExecuteDataTable(strSelect).ToList<T>();
        }

        /****************************************************************************/
        public IList<T> ExecuteList<T>(Operation sp) where T : new()
        {
            return ExecuteDataTable(sp).ToList<T>();
        }

        /****************************************************************************/
        public IList<T> ExecuteList<T>(DbCommand objCommand) where T : new()
        {       
            return ExecuteDataTable(objCommand).ToList<T>();
        }

        #endregion

        #region Dictionary Methods

        /****************************************************************************/
        public Dictionary<string, string> ExecuteDictionary(string strSelect, string idKeyField, string idValueField, IDictionary<string, string> map = null)
        {
            using(DataTable dt = this.ExecuteDataTable(strSelect))
            {
                return(ToDictionary(dt, idKeyField, idValueField, map));
            }
        }

        /****************************************************************************/
        public Dictionary<string, string> ExecuteDictionary(Operation op, string idKeyField, string idValueField, IDictionary<string, string> map = null)
        {
            using(DataTable dt = this.ExecuteDataTable(op))
            {
                return(ToDictionary(dt, idKeyField, idValueField, map));
            }
        }

        /**********************************************************************/
        public async Task<IDictionary<string, object>> ExecuteSingleRecordDictionaryAsync(Operation op, IDictionary<string, string> map = null)
        {
          return await ExecuteSingleRecordDictionaryAsync(op.Command, map);
        }

        /**********************************************************************/
        public async Task< IDictionary<string, object> > ExecuteSingleRecordDictionaryAsync(DbCommand cmd, IDictionary<string, string> map = null)
        {
            return await ExecuteAsync< IDictionary<string, object> >(cmd, async (dbCommand)=>
            {
                using(DbDataReader reader = await this.ExecuteSelectAsync(cmd))
                {
                    IDictionary<string, object> result = await ToSingleRecordDictionaryAsync(reader, map);

                    return result;
                }
            });
        }

        /****************************************************************************/
        public IDictionary<string, object> ExecuteSingleRecordDictionary(Operation op, IDictionary<string, string> map = null)
        {
            return ExecuteSingleRecordDictionary(op.Command, map);
        }

        /****************************************************************************/
        public IDictionary<string, object> ExecuteSingleRecordDictionary(DbCommand cmd, IDictionary<string, string> map = null)
        {
            return Execute< IDictionary<string, object> >(cmd, (dbCommand)=>
            {
                using(DbDataReader reader = this.ExecuteSelect(cmd))
                {
                    IDictionary<string, object> result = ToSingleRecordDictionary(reader, map);

                    return result;
                }
            });
        }

        /****************************************************************************/
        public static async Task< IDictionary<string, object> > ToSingleRecordDictionaryAsync(DbDataReader reader, IDictionary<string, string> map = null)
        {
            var values = new Dictionary<string, object>();

            if(await reader.ReadAsync())
            {
                int nFields = reader.FieldCount;

                for(int i = 0; i < nFields; ++i)
                { 
                    var isNull = await reader.IsDBNullAsync(i);
                    var name = reader.GetName(i);

                    if(map != null && map.ContainsKey(name))
                        name = map[name];

                    values.Add(name, isNull ? null : reader[i]);
                }
            }

            return values;
        }

        /****************************************************************************/
        public static IDictionary<string, object> ToSingleRecordDictionary(DbDataReader reader, IDictionary<string, string> map = null)
        {
            var values = new Dictionary<string, object>();

            if(reader.Read())
            {
                int nFields = reader.FieldCount;

                for(int i = 0; i < nFields; ++i)
                { 
                    bool isNull = reader.IsDBNull(i);
                    string name = reader.GetName(i);

                    if(map != null && map.ContainsKey(name))
                        name = map[name];

                    values.Add(name, isNull ? null : reader[i]);
                }
            }

            return values;
        }

        /****************************************************************************/
        public static Dictionary<string, string> ToDictionary(DataTable dt, string idKeyField, string idValueField, IDictionary<string, string> map = null)
        {
            var dict  = new Dictionary<string, string>(137);
            var aRows = new DBRowList(dt);

            foreach(IDataObjectSource row in aRows)
            {
                string name = row.Get(idKeyField);

                if(map != null && map.ContainsKey(name))
                    name = map[name];

                dict.Add(name, row.Get(idValueField));
            }

            return dict;
        }

        #endregion

        #region DoTask

        /********************************************************************/
        public async Task DoTaskAsync(string strSelect)
        {
            using(DbCommand objCommand = MakeSelectCommand(strSelect))
            {
                await DoTaskAsync(objCommand);              
            }
            
            return;
        }

        /********************************************************************/
        public async Task DoTaskAsync(Operation objProc)
        {
            await DoTaskAsync(objProc.Command);              
        }

        /********************************************************************/
        public async Task DoTaskAsync(DbCommand objCommand)
        {
            if(this.IsOpen)
            {
                objCommand.Connection = this.Connection;

                await Retry.RunAsync( async ()=>
                {
                    await objCommand.ExecuteNonQueryAsync();              
                }, 
                this.RetryPolicy);
            }
            else
            {
                using(Acquire o = await this.AcquireAsync())
                    await DoTaskAsync(objCommand);
            }
            
            return;
        }

        /****************************************************************************/
        public void DoTask(string strSelect)
        {
            using(DbCommand objCommand = MakeSelectCommand(strSelect))
            {
                DoTask(objCommand);              
            }
            
            return;
        }

        /****************************************************************************/
        public void DoTask(Operation objProc)
        {
            DoTask(objProc.Command);              
        }

        /****************************************************************************/
        public void DoTask(DbCommand objCommand)
        {
            if(this.IsOpen)
            {
                objCommand.Connection = this.Connection;

                Retry.Run( ()=>
                {
                    objCommand.ExecuteNonQuery();              
                }, 
                this.RetryPolicy);
            }
            else
            {
                using(Acquire o = new Acquire(this))
                    DoTask(objCommand);
            }
            
            return;
        }

        #endregion

        #region ExecuteXml

        /****************************************************************************/
        public XmlDocument ExecuteXml(string strSelect)
        {
            using(DbCommand cmd = MakeSelectCommand(strSelect))
            {
                return(ExecuteXml(cmd));
            }
        }

        /****************************************************************************/
        public XmlDocument ExecuteXml(Operation objProc)
        {
            return(ExecuteXml(objProc.Command));
        }

        /****************************************************************************/
        public XmlDocument ExecuteXml(DbCommand objCommand)
        {
            if(this.IsOpen)
                return(XmlDoc.LoadXml(ExecuteDataSet(objCommand).GetXml()));

            using(Acquire o = new Acquire(this))
                return(ExecuteXml(objCommand));
        }

        /****************************************************************************/
        public XmlDocument ExecuteXml(Operation objProc, string strDBName, IEnumerable aNames)
        {
            return(ExecuteXml(objProc.Command, strDBName, aNames));
        }

       /****************************************************************************/
        public XmlDocument ExecuteXml(Operation objProc, IEnumerable aNames)
        {
            return(ExecuteXml(objProc.Command, "", aNames));
        }

        /****************************************************************************/
        public XmlDocument ExecuteXml(DbCommand objCommand, string strDBName, IEnumerable aNames)
        {
            if(this.IsOpen)
            {
                using(DataSet dsData = ExecuteDataSet(objCommand))
                {
                    LabelDataSet(dsData, strDBName, aNames);

                    return(XmlDoc.LoadXml(dsData.GetXml()));
                }
            }

            using(Acquire o = new Acquire(this))
                return(ExecuteXml(objCommand, strDBName, aNames));
        }
      
        /****************************************************************************/
        public async Task<string> ExecuteXmlAsync(Operation objProc, string dbName, IList<string> aNames)
        {
            return await ExecuteXmlAsync(objProc.Command, dbName, aNames);
        }

        /****************************************************************************/
        public async Task<string> ExecuteXmlAsync(Operation objProc, IList<string> aNames)
        {
            return await ExecuteXmlAsync(objProc.Command, "", aNames);
        }

        /****************************************************************************/
        public async Task<string> ExecuteXmlAsync(DbCommand cmd, string dbName, IList<string> aNames)
        {
            return await ExecuteAsync<string>(cmd, async (dbCommand)=>
            {
                using(DbDataReader reader = await this.ExecuteSelectAsync(cmd))
                {
                    return await ToXmlAsync(reader, dbName, aNames);
                }
            });
        }

        /****************************************************************************/
        public int ExecuteXml(string strSelect, cXMLWriter writer, string type, bool bAttributes)
        {
            using(DbCommand cmd = MakeSelectCommand(strSelect))
            {
                return(ExecuteXml(cmd, writer, type, bAttributes));
            }
        }

        /****************************************************************************/
        public int ExecuteXml(Operation objProc, cXMLWriter writer, string type, bool bAttributes)
        {
            return(ExecuteXml(objProc.Command, writer, type, bAttributes));
        }

        /****************************************************************************/
        public int ExecuteXml(DbCommand objCommand, cXMLWriter writer, string type, bool bAttributes)
        {
            if(this.IsOpen)
                return(ToXml(ExecuteSelect(objCommand), writer, type, bAttributes));

            using(Acquire o = new Acquire(this))
                return(ExecuteXml(objCommand, writer, type, bAttributes));
        }

        /****************************************************************************/
        public int ExecuteXml(string strSelect, cXMLWriter writer, string type)
        {
            return(ExecuteXml(strSelect, writer, type, false));
        }

        #endregion

        #region ExecuteXPath

        /****************************************************************************/
        public IXPathNavigable ExecuteXPath(Operation objProc, IList<string> aNames)
        {
            return(ExecuteXPath(objProc, "", aNames));
        }

        /****************************************************************************/
        public IXPathNavigable ExecuteXPath(Operation objProc, string dbName, IList<string> aNames)
        {
            try
            {
                return(ExecuteXPath(objProc.Command, dbName, aNames));
            }
            catch(Exception ex)
            {
                throw new StoredProcException(objProc, ex);
            }
        }

        /****************************************************************************/
        public IXPathNavigable ExecuteXPath(DbCommand objCommand, string strDBName, IList<string> aNames)
        {
            if(this.IsOpen)
            {
                using(DbDataReader dbReader = ExecuteSelect(objCommand))
                {
                    using(XmlReader xmlReader = new DbXmlReader(dbReader, strDBName, aNames))
                    { 
                        return(new XPathDocument(xmlReader));
                    }
                }
            }

            using(Acquire o = new Acquire(this))
                return(ExecuteXPath(objCommand, strDBName, aNames));
        }

        /****************************************************************************/
        private static XPathDocument XPathDocFromString(string strXml)
        {
            using(StringReader objReader = new StringReader(strXml))
            {
                return(new XPathDocument(objReader));
            }
        }

        #endregion

        #region Secure String Methods

        /****************************************************************************/
        public SecureString QuerySecureString(DbCommand cmd)
        {
            byte[] aData  = QueryBinary(cmd); // If the data isn't binary then this won't work

            if(aData == null)
                return(null);

            try
            {
                char[] aChars = Encoding.UTF8.GetChars(aData, 0, aData.Length);
                
                try
                {
                    return(ToSecureString(aChars));
                }
                finally
                {
                    aChars.Clear();
                }
            }
            finally
            {
                aData.Clear();
            }
        }

        /****************************************************************************/
        unsafe private static SecureString ToSecureString(char[] aChars)
        {
            SecureString str;

            fixed(char* pChars = aChars)
            {
                str = new SecureString(pChars, aChars.Length);
            }

            str.MakeReadOnly();

            return(str);
        }

        /****************************************************************************/
        public SecureString QuerySecureString(Operation objProc)
        {
            return(QuerySecureString(objProc.Command));
        }

        #endregion

        #region QueryBinary

        /****************************************************************************/
        public byte[] QueryBinary(string strSelect)
        {
            using(DbCommand cmd = MakeSelectCommand(strSelect))
            {
                return(QueryBinary(cmd));
            }
        }

        /****************************************************************************/
        public byte[] QueryBinary(Operation objProc)
        {
            return(QueryBinary(objProc.Command));
        }

        /****************************************************************************/
        public byte[] QueryBinary(DbCommand cmd)
        {
            object objResult = ExecuteScalar(cmd);

            return(objResult as byte[]);
        }

        /****************************************************************************/
        public async Task<byte[]> QueryBinaryAsync(Operation objProc)
        {
            return await QueryBinaryAsync(objProc.Command);
        }

        /****************************************************************************/
        public async Task<byte[]> QueryBinaryAsync(DbCommand cmd)
        {
            object objResult = await ExecuteScalarAsync(cmd);

            return(objResult as byte[]);
        }

        #endregion

        #region ExecuteScalar

        /****************************************************************************/
        public object ExecuteScalar(string strSelect)
        {
            using(DbCommand cmd = MakeSelectCommand(strSelect))
            {
                return(ExecuteScalar(cmd));
            }
        }

        /****************************************************************************/
        public object ExecuteScalar(Operation objProc)
        {
            return(ExecuteScalar(objProc.Command));
        }

        /****************************************************************************/
        public object ExecuteScalar(DbCommand cmd)
        {
            object objReturn =  Execute<object>(cmd, (dbCommand)=>
                                {
                                    return dbCommand.ExecuteScalar();
                                });

            if(objReturn == null)
                throw new NoValueException();

            return(objReturn);
        }

        /****************************************************************************/
        public T ExecuteScalar<T>(string strSelect) where T : struct
        {
            return Utility.Convert<T>(ExecuteScalar(strSelect));
        }

        /****************************************************************************/
        public T ExecuteScalar<T>(DbCommand cmd) where T : struct
        {
            return Utility.Convert<T>(ExecuteScalar(cmd));
        }

        /****************************************************************************/
        public T ExecuteScalar<T>(Operation objProc) where T : struct
        {
            return Utility.Convert<T>(ExecuteScalar(objProc));
        }

        /****************************************************************************/
        public async Task<object> ExecuteScalarAsync(string strSelect) 
        {
            using(DbCommand cmd = MakeSelectCommand(strSelect))
            {
                return await ExecuteScalarAsync(cmd);
            }
        }

        /****************************************************************************/
        public async Task<T> ExecuteScalarAsync<T>(string strSelect) where T : struct
        {
            using(DbCommand cmd = MakeSelectCommand(strSelect))
            {
                return await ExecuteScalarAsync<T>(cmd);
            }
        }

        /****************************************************************************/
        public async Task<object> ExecuteScalarAsync(Operation objProc) 
        {
            return await ExecuteScalarAsync(objProc.Command);
        }

        /****************************************************************************/
        public async Task<T> ExecuteScalarAsync<T>(Operation objProc) where T : struct 
        {
            return await ExecuteScalarAsync<T>(objProc.Command);
        }

        /****************************************************************************/
        public async Task<object> ExecuteScalarAsync(DbCommand cmd) 
        {
            return await ExecuteAsync(cmd, async (dbCommand)=>
            {
                return await dbCommand.ExecuteScalarAsync();
            });
        }

        /****************************************************************************/
        public async Task<T> ExecuteScalarAsync<T>(DbCommand cmd) where T : struct
        {
            return await ExecuteAsync(cmd, async (dbCommand)=>
            {
                object val = await dbCommand.ExecuteScalarAsync();

                return Utility.Convert<T>(val);
            });
        }

        #endregion

        #region Exceptions

        /****************************************************************************/
        /****************************************************************************/
        public class DatabaseException : Exception
        {
            /****************************************************************************/
            public DatabaseException(string msg) : base(msg)
            {
            }

            /****************************************************************************/
            public DatabaseException(string msg, Exception ex) : base(msg, ex)
            {
            }
        }

        /****************************************************************************/
        /****************************************************************************/
        public class StoredProcException : DatabaseException
        {
            /****************************************************************************/
            public StoredProcException(Operation op, Exception ex) : base("Failure executing stored proc: " + op.Name, ex)
            {
            }
        }

        /****************************************************************************/
        /****************************************************************************/
        public class NoValueException : DatabaseException
        {
            /****************************************************************************/
            public NoValueException() : base("Unable to retrieve value from database")
            {
            }
        }

        #endregion

        #region Utility Methods

        /****************************************************************************/
        public static string Encode(string strData)
        {
            return(SubstituteList(strData, _encodeChars, _decodeChars, 8));
        }

        /****************************************************************************/
        public static string Decode(string strData)
        {
            return(SubstituteList(strData, _decodeChars, _encodeChars, 8));
        }

        /****************************************************************************/
        public static async Task<string> ToXmlAsync(DbDataReader reader, string dbName, IList<string> tableNames)
        {
            var writer = new cXMLWriter();

            writer.WriteStartDocument();
            { 
                writer.WriteStartElement(dbName);
                { 
                    int index = -1;

                    do
                    {
                        ++index;

                        int nFields   = -1;
                        var tableName = tableNames[index];

                        while(await reader.ReadAsync())
                        {
                            if(nFields == -1)
                                nFields = reader.FieldCount;

                            writer.WriteStartElement(tableName);
                            { 
                                for(int i = 0; i < nFields; ++i)
                                { 
                                    bool isNull = await reader.IsDBNullAsync(i);

                                    if(!isNull)
                                    { 
                                        string name = reader.GetName(i);
                                        object val  = reader[i];

                                        writer.WriteElementString(name, val.ToString().Trim());
                                    }
                                }
                            }

                            writer.WriteEndElement();
                        }
                    } 
                    while(await reader.NextResultAsync());
                }
                writer.WriteEndElement();
            }
            writer.WriteEndDocument();

            return writer.ToString();
        }

        /****************************************************************************/
        public static int ToXml(DbDataReader data, cXMLWriter writer, string type, bool bAttributes)
        {
            int nFields = data.FieldCount;
            int nRecords = 0;

            if(nFields == 0)
            {
                data.Close();
                data = null;
                throw new DatabaseException("Database.ToXmlNodes: There are no fields in the results");
            }

            while(data.Read())
            {
                if(bAttributes)
                {
                    writer.WriteStartElement(type);
                }
                else
                {
                    writer.WriteStartElement("object");
                    writer.WriteAttributeString("type", type);
                }

                try
                {
                    if(bAttributes)
                    {
                        for(int i = 0; i < nFields; ++i)
                            writer.WriteAttributeString(data.GetName(i).ToLower(), string.Copy(data[i].ToString()).Trim(), true);
                    }
                    else
                    {
                        for(int i = 0; i < nFields; ++i)
                            writer.WriteElementString(data.GetName(i).ToLower(), string.Copy(data[i].ToString()).Trim(), true);
                    }
                }
                catch(Exception)
                {
                }

                ++nRecords;
                writer.WriteEndElement();
            }

            data.Close();
            data = null;

            return(nRecords);
        }

        /****************************************************************************/
        public static string ToString(DbDataReader data)
        {
            int nFields = data.FieldCount;

            if(nFields == 0)
            {
                data.Close();
                data = null;
                throw new DatabaseException("Database::ToString: There are no fields in the results");
            }

            StringBuilder results = new StringBuilder();

            while(data.Read())
            {
                try
                {
                    for(int i = 0; i < nFields; ++i)
                        results.Append(data[i].ToString());
                }
                catch(Exception)
                {
                }
            }

            data.Close();
            data = null;

            return results.ToString();
        }

        /****************************************************************************/
        public static void LabelDataSet(DataSet ds, IEnumerable aNames)
        {        
            LabelDataSet(ds, "", aNames);
        }

        /****************************************************************************/
        public static void LabelDataSet(DataSet ds, string strDataSetName, IEnumerable aNames)
        {        
            if(strDataSetName != "")
                ds.DataSetName = strDataSetName;

            if(aNames != null)
            {
                int iIndex  = 0;
                int nTables = ds.Tables.Count;

                foreach(string strName in aNames)
                {
                    if(iIndex == 0 && strDataSetName == "")
                    { 
                        ds.DataSetName = strDataSetName = strName;
                        continue;
                    }

                    if(nTables <= iIndex)
                        break;

                    ds.Tables[iIndex++].TableName = strName;
                }
            }
        }

        #endregion

        #region Private Methods

        private delegate Task<T> AsyncDelegate<T>(DbCommand objCommand);

        /****************************************************************************/
        private async Task<T> ExecuteAsync<T>(DbCommand cmd, AsyncDelegate<T> fn)
        {
            if(this.IsOpen)
            {
                cmd.Connection = this.Connection;

				T obj = default(T);

                await Retry.RunAsync( async ()=>
                {
                    obj = await fn(cmd);              
                }, 
                this.RetryPolicy);
            
	            return obj;
            }
            else
            {
                using(Acquire o = await this.AcquireAsync())
                    return await ExecuteAsync<T>(cmd, fn);
            }
        }

        private delegate T SyncDelegate<T>(DbCommand objCommand);

        /****************************************************************************/
        private T Execute<T>(DbCommand cmd, SyncDelegate<T> fn)
        {
            if(this.IsOpen)
            {
                cmd.Connection = this.Connection;

				T obj = default(T);

                Retry.Run( ()=>
                {
                    obj = fn(cmd);              
                }, 
                this.RetryPolicy);
            
	            return obj;
            }
            else
            {
                using(Acquire o = this.Acquire)
                    return Execute<T>(cmd, fn);
            }
        }

        /****************************************************************************/
        private static string[] _encodeChars = {"\'",  "\"",  ",",   ".",   ";",   "\r",  "\n",  "<",   ">",   "(",   ")"};
        private static string[] _decodeChars = {"%27", "%22", "%2C", "%2E", "%3B", "%0D", "%0A", "%3C", "%3E", "%28", "%29"};

        /****************************************************************************/
        private static string SubstituteList(string strData, string[] strFind, string[] strReplace, uint nItems)
        {
	        for(uint i = 0; i < nItems; ++i)
                strData = strData.Replace(strFind[i], strReplace[i]);

            return(strData);
        }

        /*************************************************************************/
        private static bool IsIn(string connectionString, IEnumerable aList)
        {
            connectionString = connectionString.ToLower();

            foreach(string strItem in aList)
                if(connectionString.Contains(strItem))
                    return(true);

            return(false);
        }

        /*************************************************************************/
        /*************************************************************************/
        private class DatabaseRetryPolicy : RetryPolicy
        {
            /*************************************************************************/
            public DatabaseRetryPolicy(int iMaxRetries = RetryPolicy.kDefault, int iStartRetryWait = RetryPolicy.kDefault, double dRetryWaitIncrementFactor = RetryPolicy.kDefault)
            {
            }

            /*************************************************************************/
            public override bool ShouldRetry(Exception ex)
            {
                string strMessage = ex.Message.ToLower();

                if(strMessage.Contains("chosen as the deadlock victim"))
                    return true;

                // Azure SQL message
                if(strMessage.Contains("is not currently available"))
                    return true;

                return false;
            }
        }

        #endregion
    }

    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
	/// A connection to a SQL database
	/// </summary>
	public class SqlDatabase : Database
	{
        private StringList m_aMessages = null;

        /****************************************************************************/
        public SqlDatabase(IConfig config) : base(SqlClientFactory.Instance, config)
        {
        }

        /****************************************************************************/
        public SqlDatabase(IConfig config, string connectionString, bool bLookup) : base(SqlClientFactory.Instance, config, ConnectionString.Normalize(connectionString), bLookup)
        {
        }

        /****************************************************************************/
        public DataSet ExecuteDataSet(DbCommand cmd, out IList aMessages)
        {
            if(this.IsOpen)
            {
                m_aMessages = new StringList();

                (this.Connection as SqlConnection).InfoMessage += new SqlInfoMessageEventHandler(Database_InfoMessage);

                DataSet ds = base.ExecuteDataSet(cmd);

                aMessages = m_aMessages;

                return(ds);
            }

            using(this.Acquire)
                return(ExecuteDataSet(cmd, out aMessages));
         }

        /****************************************************************************/
        public DataSet ExecuteDataSet(string strSelect, out IList aMessages)
        {
            using(DbCommand cmd = MakeSelectCommand(strSelect))
            {
                return(ExecuteDataSet(cmd, out aMessages));
            }
        }

        /****************************************************************************/
        void Database_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            m_aMessages.Add(e.Message);
        }        
    }

    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
	/// A connection to an OLE DB database
	/// </summary>
	public class OleDbDatabase : Database
	{
        /****************************************************************************/
        public OleDbDatabase(IConfig config) : base(System.Data.OleDb.OleDbFactory.Instance, config)
        {
        }

        /****************************************************************************/
        public OleDbDatabase(IConfig config, string connectionString, bool bLookup) : base(System.Data.OleDb.OleDbFactory.Instance, config, ConnectionString.Normalize(connectionString), bLookup)
        {
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
	/// A connection to an ODBC database
	/// </summary>
	public class OdbcDatabase : Database
	{
        /****************************************************************************/
        public OdbcDatabase(IConfig config) : base(System.Data.Odbc.OdbcFactory.Instance, config)
        {
        }

        /****************************************************************************/
        public OdbcDatabase(IConfig config, string connectionString, bool bLookup) : base(System.Data.Odbc.OdbcFactory.Instance, config, ConnectionString.Normalize(connectionString), bLookup)
        {
        }
    }
}
