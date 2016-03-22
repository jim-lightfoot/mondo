/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: OrderedDictionary.cs									*/
/*        Class(es): OrderedDictionary										*/
/*          Purpose: An ordered collection of keyed objects                 */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 12 Sep 2006                                            */
/*                                                                          */
/*   Copyright (c) 2001 - 2007 - Jim Lightfoot, All rights reserved         */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using System.ComponentModel;

namespace Mondo.Common
{   
    /****************************************************************************/
    /****************************************************************************/
    public interface IKeyHolder<K> 
    {
        bool ContainsKey(K objKey);
        int IndexOfKey(K objKey);
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface IIdObject<T> 
    {
        T Id {get;}
    }

    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
	/// An ordered collection of keyed objects
	/// </summary>
	public class OrderedDictionary<K, V> : IList, IDictionary, IKeyHolder<K>
	{
        private List<V>          m_ObjectList;
        private Dictionary<K, V> m_ObjectDict;

        /****************************************************************************/
        public OrderedDictionary(int iInitialSize)
        {
            m_ObjectList = new List<V>();
            m_ObjectDict = new Dictionary<K, V>(iInitialSize);
        }

        /****************************************************************************/
        public int Count 
        {
            get
            {
                return(m_ObjectList.Count);
            }
        }

        /****************************************************************************/
        public void Sort()
        {
            m_ObjectList.Sort();
        }

        /****************************************************************************/
        public void Sort(IComparer<V> objComparer)
        {
            m_ObjectList.Sort(objComparer);
        }

        /****************************************************************************/
        public bool ContainsKey(K objKey)
        {
            return(m_ObjectDict.ContainsKey(objKey));
        }

        /****************************************************************************/
        public int IndexOfKey(K objKey)
        {
            return(m_ObjectList.IndexOf(m_ObjectDict[objKey]));
        }

        /****************************************************************************/
        public int IndexOf(V objValue)
        {
            return(m_ObjectList.IndexOf(objValue));
        }

        /****************************************************************************/
        public V ItemBefore(V objItem)
        {
            int iIndex = m_ObjectList.IndexOf(objItem);

            return(m_ObjectList[iIndex-1]);
        }

        /****************************************************************************/
        public V ItemAfter(V objItem)
        {
            int iIndex = m_ObjectList.IndexOf(objItem);

            return(m_ObjectList[iIndex+1]);
        }

        /****************************************************************************/
        public void Switch(V objFirst, V objSecond)
        {
            int iFirst  = m_ObjectList.IndexOf(objFirst);
            int iSecond = m_ObjectList.IndexOf(objSecond);

            if(iFirst > iSecond)
            {
                int iTemp = iFirst;
                V objTemp = objFirst;

                iFirst = iSecond;
                iSecond = iTemp;

                objFirst = objSecond;
                objSecond = objTemp;
            }

            m_ObjectList.Remove(objSecond);
            m_ObjectList.Remove(objFirst);

            m_ObjectList.Insert(iFirst,  objSecond);
            m_ObjectList.Insert(iSecond, objFirst);

            return;
        }

        /****************************************************************************/
        public virtual void Add(K objKey, V objAdd)
        {
            m_ObjectList.Add(objAdd);
            m_ObjectDict.Add(objKey, objAdd);
        }

        /****************************************************************************/
        public virtual bool Remove(K Key)
        {
            V objValue = m_ObjectDict[Key];

            if(objValue != null)
            {
                if(m_ObjectList.Remove(objValue))
                    return(m_ObjectDict.Remove(Key));
            }

            return(false);
        }

        /****************************************************************************/
        public virtual void Replace(int iIndex, K objKey, V objAdd)
        {
            m_ObjectList[iIndex] = objAdd;
            m_ObjectDict.Remove(objKey);
            m_ObjectDict.Add(objKey, objAdd);
        }

        /****************************************************************************/
        public virtual void Insert(int iIndex, K objKey, V objAdd)
        {
            m_ObjectList.Insert(iIndex, objAdd);
            m_ObjectDict.Add(objKey, objAdd);
        }

        /****************************************************************************/
        public V GetValueAt(int iIndex)
        {
            return(m_ObjectList[iIndex]);
        }

        /****************************************************************************/
        public virtual void Clear()
        {
            m_ObjectList.Clear();
            m_ObjectDict.Clear();
        }

        /****************************************************************************/
        public virtual V this [K Key] 
        {
            get
            {             
                return(m_ObjectDict[Key]);
            }

            set
            {
                int iIndex = this.IndexOfKey(Key);

                Remove(Key);
                Insert(iIndex, Key, value);
            }
        }

        /****************************************************************************/
        public IEnumerator GetEnumerator() 
        {
            return(GetArrayEnumerator());
        }

        /****************************************************************************/
        public IEnumerator GetKeysEnumerator() 
        {
            return(m_ObjectDict.Keys.GetEnumerator());
        }

        /****************************************************************************/
        public IDictionaryEnumerator GetDictionaryEnumerator()
        {
            return(this.m_ObjectDict.GetEnumerator());
        }

        /****************************************************************************/
        public IEnumerator GetArrayEnumerator()
        {
            return(this.m_ObjectList.GetEnumerator());
        }

        /****************************************************************************/
        public IEnumerable<V> GetArrayEnumerable()
        {
            return(this.m_ObjectList);
        }

        /****************************************************************************/
        public IEnumerator<V> GetArrayGenericEnumerator()
        {
            return(this.m_ObjectList.GetEnumerator());
        }

        #region ICollection Members

        /****************************************************************************/
        public void CopyTo(Array array, int index)
        {
            // do nothing
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

        #region IList Members

        int IList.Add(object value)
        {
            throw new NotSupportedException();
        }

        void IList.Clear()
        {
            m_ObjectList.Clear();
            m_ObjectDict.Clear();
        }

        bool IList.Contains(object value)
        {
            if(value is V)
                return(m_ObjectList.Contains((V)value));

            return(false);
        }

        int IList.IndexOf(object value)
        {
            if(value is V)
                return(m_ObjectList.IndexOf((V)value));

            return(-1);
        }

        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        bool IList.IsFixedSize
        {
            get { return(false); }
        }

        bool IList.IsReadOnly
        {
            get { return(false); }
        }

        void IList.Remove(object value)
        {
        }

        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        object IList.this[int index]
        {
            get
            {
                return(m_ObjectList[index]);
            }

            set
            {
            throw new NotSupportedException();
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return(GetArrayEnumerator());
        }

        #endregion

        #region IDictionary Members

        public void Add(object key, object value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Contains(object key)
        {
            if(key is K)
                return(this.m_ObjectDict.ContainsKey((K)key));

            if(typeof(K) == typeof(string))
                return(this.m_ObjectDict.ContainsKey((K)Utility.ConvertType(key, typeof(string))));

            return(false);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return(m_ObjectDict.GetEnumerator());
        }

        public bool IsFixedSize
        {
            get { return(false); }
        }

        public bool IsReadOnly
        {
            get { return(false); }
        }

        public ICollection Keys
        {
            get { return(m_ObjectDict.Keys); }
        }

        public void Remove(object key)
        {
            if(key is K)
                this.Remove((K)key);
        
            if(key is IIdObject<K>)
                this.Remove((key as IIdObject<K>).Id);

            throw new ArgumentException();
        }

        public ICollection Values
        {
            get { return(m_ObjectDict.Values); }
        }

        public ICollection<V> ValueList
        {
            get { return(m_ObjectList); }
        }

       public object this[object key]
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        #endregion
    }

    /****************************************************************************/
    /****************************************************************************/
    public class Table<T> : OrderedDictionary<T, T>
    {
        public Table() : base(17)
        {
        }

        /****************************************************************************/
        public void Add(T objValue)
        {
            Add(objValue, objValue);
        }

        /****************************************************************************/
        public void Insert(int iIndex, T objValue)
        {
            Insert(iIndex, objValue, objValue);
        }

        /****************************************************************************/
        public bool Contains(T objValue)
        {
            return(ContainsKey(objValue));
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class StringTable : Table<string>
    {
        public StringTable()
        {
        }

        /****************************************************************************/
        public static StringTable FromList(string strList, string strSeparator)
        {
            StringTable objTable = new StringTable();
            string[]    aStrings = strList.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries);

            foreach(string strValue in aStrings)
                objTable.Add(strValue);

            return(objTable);
        }
    }
}
