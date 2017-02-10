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

        /****************************************************************************/
        public Query()
        {
        }

        /****************************************************************************/
        protected StringBuilder StringBuilder
        {
            get { return _sb; }
        }

        /****************************************************************************/
        public virtual Query Select(IEnumerable<string> columns)
        {
          var list = new StringList(columns);

          _sb.AppendLine("SELECT " + list.Pack(", "));

          return this;
        }

        /****************************************************************************/
        public virtual Query From(string tableName)
        {
          _sb.AppendLine("FROM   " + tableName);

          return this;
        }

        /****************************************************************************/
        public virtual Query SelectFrom(string tableName)
        {
          _sb.AppendLine("SELECT *");
          _sb.AppendLine("FROM   " + tableName);

          return this;
        }

        /****************************************************************************/
        public virtual Query Where(string columnName)
        {
          _sb.Append("WHERE (" + columnName);

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
            var list = new StringList();

            foreach(object val in values)
                list.Add(FormatValue(val));

            _sb.Append(" in (");
            _sb.Append(list.Pack(", "));
            _sb.AppendLine("))");

            return this;
        }

        /****************************************************************************/
        public virtual Query AndWhere(string columnName)
        {
          _sb.Append("AND ((" + columnName);

          return this;
        }

        /****************************************************************************/
        public virtual Query OrWhere(string columnName)
        {
          _sb.Append("OR ((" + columnName);

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
          _sb.Append("AND (" + columnName);

          return this;
        }

        /****************************************************************************/
        public virtual Query Or(string columnName)
        {
          _sb.Append("OR (" + columnName);

          return this;
        }

        /****************************************************************************/
        public override string ToString()
        {
            return _sb.ToString();
        }

        /****************************************************************************/
        protected virtual Query Expression(string sOperator, object value)
        {
            _sb.Append(" " + sOperator + " ");
            _sb.Append(FormatValue(value));
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

        /****************************************************************************/
        public virtual Query Raw(string text)
        {
            _sb.AppendLine(text);

            return this;
        }

        /****************************************************************************/
        public virtual Query OrderBy(IEnumerable<string> columnNames)
        {
            var list = new StringList(columnNames);

            _sb.Append("ORDER BY ");
            _sb.AppendLine(list.Pack("{0} ASC", ", "));

            return this;
        }

        /****************************************************************************/
        public virtual Query OrderBy(string columnName, bool asc = true)
        {
            _sb.AppendLine("ORDER BY " + columnName + (asc ? " ASC" : "DESC"));

            return this;
        }
    }
}
