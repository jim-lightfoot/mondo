/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  	                                                */
/*                                                                          */
/*      Namespace: Mondo.Common	                                            */
/*           File: Query.cs                                                 */
/*      Class(es): Query                                                    */
/*        Purpose: Class to create a SQL query                              */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 9 Feb 2017                                             */
/*                                                                          */
/*   Copyright (c) 2017 - Jim Lightfoot, All rights reserved                */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public class Query
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private bool     _inSelect = false;
        private bool     _inOn     = false;

        /****************************************************************************/
        public Query()
        {
        }

        /****************************************************************************/
        protected StringBuilder StringBuilder
        {
            get { return _sb; }
        }

        #region Select

        /****************************************************************************/
        public virtual Query Select(IEnumerable<string> columns = null)
        {
            var list = new StringList(columns);

            _sb.Append("SELECT ");

            if(columns != null)
            { 
              _sb.AppendLine(columns.Pack(", ", (obj)=>
              {
                  return FormatColumnName(obj.ToString());
              }));
            }

            _inSelect = true;

            return this;
        }

        /****************************************************************************/
        public virtual Query All()
        {
          _sb.Append(" * ");

          return this;
        }

        /****************************************************************************/
        public virtual Query From(string tableName)
        {
            _sb.AppendLine("FROM   " + FormatTableName(tableName));

            return this;
        }

        /****************************************************************************/
        public virtual Query Top(int val)
        {
            _sb.AppendFormat(" TOP {0} ", val);

            return this;
        }

        /****************************************************************************/
        public virtual Query Top(string var)
        {
            _sb.AppendFormat(" TOP ({0}) ", var);

            return this;
        }

        /****************************************************************************/
        public virtual Query SelectFrom(string tableName)
        {
            _sb.AppendLine("SELECT *");
            _sb.AppendLine("FROM   " + FormatTableName(tableName));

            return this;
        }

        #endregion

        #region Delete

        /****************************************************************************/
        public virtual Query DeleteFrom(string tableName)
        {
            _sb.AppendLine("DELETE FROM " + FormatTableName(tableName));

            return this;
        }

        #endregion

        #region Update

        /****************************************************************************/
        public virtual Query Update(string tableName)
        {
            _sb.AppendLine("UPDATE " + FormatTableName(tableName));

            return this;
        }

        /****************************************************************************/
        public virtual Query Set(IDictionary<string, object> values)
        {
            _sb.AppendLine("SET ");

            bool first = true;

            foreach(string columnName in values.Keys)
            {
                if(!first)
                    _sb.Append(", ");

                first = false;

                _sb.AppendFormat("[{0}] = {1}", columnName, FormatValue(values[columnName]));
            }

            return this;
        }

        #endregion

        #region Insert

        /****************************************************************************/
        public virtual Query InsertInto(string tableName)
        {
            _sb.AppendLine("INSERT INTO " + FormatTableName(tableName));

            return this;
        }

        /****************************************************************************/
        public virtual Query Columns(IEnumerable<string> columnNames)
        {
            var list = new StringList(columnNames);

            if(!_inSelect)
                _sb.AppendLine("(");

            _sb.AppendLine(columnNames.Pack(", ", (obj)=>
            {
                return FormatColumnName(obj.ToString());
            }));

            if(!_inSelect)
                _sb.AppendLine(")");

            return this;
        }

        /****************************************************************************/
        public virtual Query Values(IEnumerable<object> values)
        {
            _sb.AppendLine("VALUES");
            _sb.AppendLine("(");
            _sb.Append(values.Pack(",\r\n", (obj)=>
            {
                return FormatValue(obj);
            }));
            _sb.AppendLine(")");

            return this;
        }

        #endregion

        /****************************************************************************/
        public virtual Query Union()
        {
            _sb.AppendLine("UNION");

            return this;
        }


        #region Joins

        /****************************************************************************/
        public virtual Query InnerJoin(string tableName)
        {
            _sb.AppendLine("INNER JOIN " + FormatTableName(tableName));

            return this;
        }

        /****************************************************************************/
        public virtual Query LeftOuterJoin(string tableName)
        {
            _sb.AppendLine("LEFT OUTER JOIN " + FormatTableName(tableName));

            return this;
        }

        /****************************************************************************/
        public virtual Query RightOuterJoin(string tableName)
        {
            _sb.AppendLine("RIGHT OUTER JOIN " + FormatTableName(tableName));

            return this;
        }

        /****************************************************************************/
        public virtual Query FullOuterJoin(string tableName)
        {
            _sb.AppendLine("FULL OUTER JOIN " + FormatTableName(tableName));

            return this;
        }

        /****************************************************************************/
        public virtual Query On(string columnName)
        {
            _sb.AppendLine("ON " + FormatColumnName(columnName));
            _inOn = true;

            return this;
        }

        #endregion

        #region Where

        /****************************************************************************/
        public virtual Query Where(string columnName)
        {
          _sb.Append("WHERE (" + FormatColumnName(columnName));
          _inOn = false;

          return this;
        }

        /****************************************************************************/
        public virtual Query IsEqualTo(object value)
        {
            return Expression("=", value);
        }

        /****************************************************************************/
        public virtual Query IsNotEqualTo(object value)
        {
            return Expression("!=", value);
        }

        /****************************************************************************/
        public virtual Query IsLike(object value)
        {
            return Expression("LIKE", value);
        }

        /****************************************************************************/
        public virtual Query IsGreaterThan(object value)
        {
            return Expression(">", value);
        }

        /****************************************************************************/
        public virtual Query IsLessThan(object value)
        {
            return Expression("<", value);
        }

        /****************************************************************************/
        public virtual Query IsLessThanOrEqualTo(object value)
        {
            return Expression("<=", value);
        }

        /****************************************************************************/
        public virtual Query IsGreaterThanOrEqualTo(object value)
        {
            return Expression(">=", value);
        }

        /****************************************************************************/
        public virtual Query IsIn(IEnumerable<object> values)
        {
            _sb.Append(" in (");
            _sb.Append(values.Pack(", ", (s)=>
            {
                return FormatValue(s);
            }));

            _sb.AppendLine("))");

            return this;
        }

        /****************************************************************************/
        public virtual Query AndWhere(string columnName)
        {
          _sb.Append("AND ((" + FormatColumnName(columnName));

          return this;
        }

        /****************************************************************************/
        public virtual Query OrWhere(string columnName)
        {
          _sb.Append("OR ((" + FormatColumnName(columnName));

          return this;
        }

        /****************************************************************************/
        public virtual Query End()
        {
            _sb.AppendLine(")");
            return this;
        }

        /****************************************************************************/
        public virtual Query And(string columnName)
        {
          _sb.Append("AND (" + FormatColumnName(columnName));

          return this;
        }

        /****************************************************************************/
        public virtual Query Or(string columnName)
        {
          _sb.Append("OR (" + FormatColumnName(columnName));

          return this;
        }

        #endregion

        /****************************************************************************/
        public override string ToString()
        {
            return _sb.ToString();
        }

        /****************************************************************************/
        public virtual Query Raw(string text)
        {
            _sb.AppendLine(text);

            return this;
        }

        /****************************************************************************/
        public virtual Query OrderBy(IEnumerable<string> columnNames, bool asc = true)
        {
            _sb.Append("ORDER BY ");
            _sb.AppendLine(columnNames.Pack(", ", (columnName)=>
            {
                return FormatColumnName(columnName.ToString()) + (asc ? " ASC" : "DESC");
            }));

            return this;
        }

        /****************************************************************************/
        public virtual Query OrderBy(string columnName, bool asc = true)
        {
            _sb.AppendLine("ORDER BY " + FormatColumnName(columnName) + (asc ? " ASC" : "DESC"));

            return this;
        }

        #region Helper Methods

        /****************************************************************************/
        protected virtual string FormatTableName(string tableName)
        {
            var parts1 = new StringList(tableName, " ", true);
            var parts2 = new StringList(parts1[0], ".", true);
            var result = parts2.Pack("[{0}]", ".");

            if(parts1.Count == 2)
              result += " " + parts1[1];

            return result;
        }

        /****************************************************************************/
        protected virtual string FormatColumnName(string columnName)
        {
            if(!columnName.Contains("."))
                return string.Format("[{0}]", columnName);

            return string.Format("{0}.[{1}]", columnName.StripAfterLast(".", true), columnName.StripUpTo("."));
        }

        /****************************************************************************/
        protected virtual Query Expression(string sOperator, object value)
        {
            _sb.Append(" " + sOperator + " ");

            if(_inOn && value.ToString().IndexOf("'") == 0)
                _sb.Append(value);
            else if(_inOn)
                _sb.Append(FormatColumnName(value.ToString()));
             else
                _sb.Append(FormatValue(value));

            if(!_inOn)
                _sb.AppendLine(")");

            return this;
        }

        /****************************************************************************/
        protected virtual string Quote(string value)
        {
            return "'" + value.Replace("'", "''") + "'";
        }

        /****************************************************************************/
        protected virtual string FormatValue(object value)
        {
            if(value is DateTime)
                return Quote(((DateTime)value).ToString("s"));

            var sVal = value.ToString();

            if(value is string)
            {
                if(sVal.StartsWith("@"))
                    return sVal;

                return Quote(sVal);
            }

            if(value is int || value is uint || value is long || value is short || value is ushort || value is ulong || value is double || value is float || value is decimal)
                return sVal;

            return Quote(sVal);
        }

        #endregion

    }
}
