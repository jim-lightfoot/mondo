/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Database							                */
/*             File: StoredProc.cs										    */
/*        Class(es): Operation, StoredProc, TextCommand                     */
/*          Purpose: Call a stored proc or a parameterized SQL text command */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 7 Sep 2001                                             */
/*                                                                          */
/*   Copyright (c) 2001-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection;
using System.Resources;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.XPath;
using Mondo.Common;
using Mondo.Xml;

namespace Mondo.Database
{
    /****************************************************************************/
    /****************************************************************************/
    public class Operation : IDisposable 
    {
        protected DbCommand m_objCommand = null;
        protected Mondo.Database.Database m_objDatabase = null;
        private bool m_bDisposeDB = false;
        private string m_strName;

        /****************************************************************************/
        protected Operation(Mondo.Database.Database db, string strName)
        {
            m_objDatabase = db;
            m_strName = strName;
        }

        /****************************************************************************/
        protected Operation(string strName, IConfig config) : this(Database.Create(config), strName)
        {
            m_bDisposeDB  = true;
        }

        /****************************************************************************/
        public virtual Mondo.Database.Database  Database    {get{return(m_objDatabase);} protected set{m_objDatabase = value;}}
        public DbParameterCollection            Parameters  {get{return(m_objCommand.Parameters);}}
        public string                           Name        {get{return(m_strName);}}
        public int                              Timeout     {get{return(m_objCommand.CommandTimeout);} set{m_objCommand.CommandTimeout = value;}}
        protected bool                          SetDispose  {set{m_bDisposeDB = value;}}

        /****************************************************************************/
        public DbCommand Command         
        {
            get { return(m_objCommand); }
         }

        /****************************************************************************/
        public DbParameter AddParam(string strName, string strValue)
        {
            return(AddParam(strName, strValue, false));
        }

        /****************************************************************************/
        public DbParameter AddParam(string strName, string strValue, bool bEncode)
        {
            strValue = strValue.Normalized();

            if(bEncode)
                strValue = Database.Encode(strValue);

            return(AddInputParam(strName, DbType.String, strValue.Length*2, strValue));
        }

        /****************************************************************************/
        public DbParameter AddTextParam(string strName, string strValue)
        {
            strValue = strValue.Trim();

            return(AddInputParam(strName, DbType.String, strValue.Length*2, strValue));
        }

        /****************************************************************************/
        private DbParameter AddInputParam(string strName, DbType iType, int iLength, object objValue)
        {
            string      strSource = strName.Remove(0, 1);
            DbParameter objParam  = m_objDatabase.CreateParameter(strName,
                                                                  iType,
                                                                  iLength,
                                                                  ParameterDirection.Input,
                                                                  false,
                                                                  0, 0,
                                                                  strSource,
                                                                  DataRowVersion.Default,
                                                                  objValue);

            m_objCommand.Parameters.Add(objParam);
            return(objParam);
        }

        /****************************************************************************/
        public DbParameter AddNullParam(string strName, DbType iType)
        {
            return(AddInputParam(strName, iType, 4, DBNull.Value));
        }

        /****************************************************************************/
        public DbParameter AddParam(string strName, Guid guidValue)
        {
            return(AddInputParam(strName, DbType.Guid, 16, guidValue));
        }

        /****************************************************************************/
        public DbParameter AddParam(string strName, byte[] pBytes, uint uLength)
        {
            return(AddInputParam(strName, DbType.Binary, (int)uLength, pBytes));
        }

        /****************************************************************************/
        public DbParameter AddParam(string strName, uint uValue)
        {
            return(AddParam(strName, uValue.ToString()));
        }

        /****************************************************************************/
        public DbParameter AddParam(string strName, int iValue)
        {
            return(AddInputParam(strName, DbType.Int32, 4, iValue));
        }        
        
        /****************************************************************************/
        public DbParameter AddParam(string strName, long iValue)
        {
            return(AddInputParam(strName, DbType.Int64, 8, iValue));
        }        
        
        /****************************************************************************/
        public DbParameter AddParam(string strName, bool bValue)
        {
            return(AddInputParam(strName, DbType.Boolean, 1, bValue ? 1 : 0));
        }        
        
        /****************************************************************************/
        public DbParameter AddParam(string strName, DateTime dtValue)
        {
            return(AddInputParam(strName, DbType.DateTime, 8, dtValue));
        }        
        
        /****************************************************************************/
        public DbParameter AddParam(string strName, Double dValue)
        {
            return(AddInputParam(strName, DbType.Double, 8, dValue));
        }        
        
        /****************************************************************************/
        public DbParameter AddParam(string strName, Decimal dValue)
        {
            return(AddInputParam(strName, DbType.Decimal, 8, dValue));
        }        
        
        /****************************************************************************/
        public DbParameter AddParam(string strName, byte[] aData)
        {
            return(AddInputParam(strName, DbType.Binary, aData.Length, aData));
        }        
        
        /****************************************************************************/
        public DbParameter AddParam(string strName, XmlNode xmlData)
        {
            string strXml = xmlData.OuterXml;
                
            return(AddInputParam(strName, DbType.Xml, strXml.Length, strXml));
        }        

        /****************************************************************************/
        private void AddOutParam(SqlCommand objStoredProc)
        {
            objStoredProc.Parameters.Add(m_objDatabase.CreateParameter("@strOutput",
                                                                        DbType.String,
                                                                        512,
                                                                        ParameterDirection.Output,
                                                                        false,
                                                                        0, 0,
                                                                        "strOutput",
                                                                        DataRowVersion.Default,
                                                                        null));

            objStoredProc.UpdatedRowSource = UpdateRowSource.OutputParameters;
        }

        #region IDisposable Members

        public virtual void Dispose()
        {
            m_objCommand.Dispose();

            if(m_bDisposeDB && m_objDatabase != null)
                m_objDatabase.Dispose();
        }

        #endregion
    }

    /****************************************************************************/
    /****************************************************************************/
    public class StoredProc : Operation 
    {
        protected string m_strProcName;

        /****************************************************************************/
        public StoredProc(Mondo.Database.Database db, string strProcName) : base(db, strProcName)
        {
            m_strProcName = strProcName;
            m_objCommand  = m_objDatabase.MakeCommand(this); 
        }

        /****************************************************************************/
        public StoredProc(string strProcName, IConfig config = null) : base(strProcName, config)
        {
            m_strProcName = strProcName;
            m_objCommand  = m_objDatabase.MakeCommand(this); 
        }

        /****************************************************************************/
        public override Database Database
        {
            get
            {
                return(base.Database);
            }

            protected set
            {
                base.Database = value;
                m_objCommand  = m_objDatabase.MakeCommand(this); 
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class TextCommand : Operation 
    {
        /****************************************************************************/
        public TextCommand(Mondo.Database.Database db, string strSQL) : base(db, strSQL)
        {
            m_objCommand  = m_objDatabase.MakeSelectCommand(strSQL); 
        }
    }
}

