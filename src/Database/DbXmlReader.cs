/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Database							                */
/*             File: DbXmlReader.cs										    */
/*        Class(es): DbXmlReader										    */
/*          Purpose: An XmlReader that reads from an DbDataReader           */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 5 Feb 2016                                             */
/*                                                                          */
/*   Copyright (c) 2016 - Jim Lightfoot, All rights reserved                */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Mondo.Common;

namespace Mondo.Database
{
    /****************************************************************************/
    /****************************************************************************/
    public partial class DbXmlReader : XmlReader
    {
        private readonly IDataReader    _reader;
        private readonly Stack<string>  _tableNames     = new Stack<string>();
        private readonly StringList     _columnNames    = new StringList();
        private readonly string         _rootName;
        private int                     _currentColumn  = 0;
        private string                  _currentValue   = "";
        private State                   _state          = State.Initial;

        /****************************************************************************/
        private enum State
        {
            Initial,
            EnterRoot,
            InRoot,
            ExitRoot,
            EnterRow,
            ExitRow,
            EnterColumn,
            ExitColumn,
            EnterValue,
            Done
        }

        /****************************************************************************/
        public DbXmlReader(IDataReader reader, string rootName, IList<string> aTableNames)
        {
            _reader = reader;
            _rootName = rootName;

            int nTables = aTableNames.Count;

            for(int i = nTables-1; i >=0; --i)
                _tableNames.Push(aTableNames[i]);
        }

        /****************************************************************************/
        public override bool Read()
        {  
            switch(_state)
            {
                case State.Initial:
                    ReadColumns();
                    return(SetState(State.EnterRoot));

                case State.EnterRoot:
                case State.ExitRow:
                { 
                    if(_reader.Read())
                    {
                        _currentColumn = 0;
                        return(SetState(State.EnterRow));
                    }

                    if(!_reader.NextResult())
                        return(SetState(State.ExitRoot));

                    ReadColumns();
                    _tableNames.Pop(); // ??? Do something to handle not enough names

                    SetState(State.EnterRoot);
                    return(Read());
                }

                case State.EnterRow:
                case State.ExitColumn:
                {
                    _currentValue = "";

                    while(_currentValue == "")
                    {
                        if(_currentColumn >= _columnNames.Count)
                            return(SetState(State.ExitRow));

                        _currentValue = GetValue(_currentColumn++);
                    }

                    return(SetState(State.EnterColumn));
                }

                case State.EnterColumn:
                    return(SetState(State.EnterValue));

                case State.EnterValue:
                    return(SetState(State.ExitColumn));

                case State.ExitRoot:
                    return(SetState(State.Done));

                default:
                    return(true);
            }
        }

        /****************************************************************************/
        public override int Depth
        {
            get 
            { 
                switch(_state)
                {
                    case State.EnterRow:
                    case State.ExitRow:
                        return(1);

                    case State.EnterColumn:
                    case State.ExitColumn:
                        return(2);

                    case State.EnterValue:
                        return(3);

                    default:
                        return(0);
                }   
            }
        }

        /****************************************************************************/
        public override string LocalName
        {
            get 
            { 
                if(_state == State.EnterRoot)
                    return(_rootName);

                if(_state == State.EnterColumn)
                    return(_columnNames[_currentColumn-1]);

                return(_tableNames.Peek()); 
            }
        }

        /****************************************************************************/
        private static readonly Dictionary<State, XmlNodeType> s_NodeTypes = 
            new Dictionary<State,XmlNodeType>
            {
                {State.EnterRoot,   XmlNodeType.Element},
                {State.EnterRow,    XmlNodeType.Element},
                {State.EnterColumn, XmlNodeType.Element},
                {State.ExitRoot,    XmlNodeType.EndElement},
                {State.ExitRow,     XmlNodeType.EndElement},
                {State.ExitColumn,  XmlNodeType.EndElement},
                {State.EnterValue,  XmlNodeType.Text},
                {State.Done,        XmlNodeType.Document},
                {State.Initial,     XmlNodeType.Document}
            };

        /****************************************************************************/
        public override XmlNodeType NodeType
        {
            get 
            { 
                return(s_NodeTypes[_state]);
            }
        }

        /****************************************************************************/
        public override ReadState ReadState
        {
            get 
            { 
                switch(_state)
                {
                    case State.Initial:
                        return(ReadState.Initial); 

                    case State.Done:
                        return(ReadState.EndOfFile); 

                    default:
                        return(ReadState.Interactive);
                }   
            }
        }

        /****************************************************************************/
        public override string Value
        {
            get 
            { 
                return(_currentValue);
            }
        }

        /****************************************************************************/
        public override bool EOF
        {
            get { return(_state == State.Done); }
        }

        /****************************************************************************/
        public override bool IsEmptyElement
        {
            get { return(_state == State.EnterValue); }
        }

        #region Private Methods

        /****************************************************************************/
        private void ReadColumns()
        {
            int nColumns = _reader.FieldCount;

            _columnNames.Clear();

            for(int i = 0; i < nColumns; ++i)
                _columnNames.Add(_reader.GetName(i));

            return;
        }

        /****************************************************************************/
        private bool SetState(State state)
        {  
            _state = state;

            return(state != State.Done);
        }

        /****************************************************************************/
        private string GetValue(int column)
        {
            object val = _reader.GetValue(column);

            if(val is DBNull)
                return("");

            if(val is DateTime)
                return(((DateTime)val).ToString("s"));

            if(val is bool)
                return((bool)val ? "true" : "false");

            if(val is string)
                return(val as string);

            return(val.ToString());
        }

        #endregion

        #region Irrelevant

        /****************************************************************************/
        public override string BaseURI
        {
            get { return(""); }
        }

        /****************************************************************************/
        public override bool MoveToElement()
        {
            return(true);
        }

        /****************************************************************************/
        public override int AttributeCount
        {
            get { return(0); }
        }

        /****************************************************************************/
        public override string NamespaceURI
        {
            get { return(""); }
        }

        /****************************************************************************/
        public override XmlNameTable NameTable
        {
            get { return(new NameTable()); }
        }

        /****************************************************************************/
        public override string Prefix
        {
            get { return(""); } 
        }

        /****************************************************************************/
        public override bool HasAttributes
        {
            get
            {
                return(false);
            }
        }

        /****************************************************************************/
        public override string GetAttribute(int i)
        {
            return("");
        }

        /****************************************************************************/
        public override string GetAttribute(string name)
        {
            return("");
        }

        /****************************************************************************/
        public override string GetAttribute(string name, string namespaceURI)
        {
            return("");
        }

        /****************************************************************************/
        public override string LookupNamespace(string prefix)
        {
            return("");
        }

        /****************************************************************************/
        public override bool MoveToAttribute(string name)
        {
            return false;
        }

        /****************************************************************************/
        public override bool MoveToAttribute(string name, string ns)
        {
            return false;
        }

        /****************************************************************************/
        public override bool MoveToFirstAttribute()
        {
            return false;
        }

        /****************************************************************************/
        public override bool MoveToNextAttribute()
        {
            return false;
        }


        /****************************************************************************/
        public override bool ReadAttributeValue()
        {
            return false;
        }

        /****************************************************************************/
        public override void ResolveEntity()
        {
        }

        #endregion
    }
}
