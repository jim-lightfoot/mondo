/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: KeyedDictionary.cs									    */
/*        Class(es): KeyedDictionary								        */
/*          Purpose: A collection of keyed objects                          */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 12 Sep 2001                                            */
/*                                                                          */
/*   Copyright (c) 2001-2011 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public interface IDictionaryEntry<K>
    {
        /****************************************************************************/
        K Key(int i);
    }

    /****************************************************************************/
    /****************************************************************************/
	public class KeyedDictionary<K, V> : IList where V : IDictionaryEntry<K>
	{
        private   Mondo.Common.OrderedDictionary<K, V> m_ObjectDict;
        private   int               m_iKeyIndex = 0;
        protected bool              m_bReadOnly = false;

        /****************************************************************************/
        public KeyedDictionary(int iInitialSize)
        {
            m_ObjectDict = new Mondo.Common.OrderedDictionary<K, V>(iInitialSize);
        }

        /****************************************************************************/
        public int Count 
        {
            get
            {
                return(m_ObjectDict.Count);
            }
        }

        /****************************************************************************/
        public int KeyIndex    
        {
            get{return(m_iKeyIndex);}
            set{m_iKeyIndex = value;}
        }

        /****************************************************************************/
        public void Sort()
        {
            m_ObjectDict.Sort();
        }

        /****************************************************************************/
        public void Sort(IComparer<V> objComparer)
        {
            m_ObjectDict.Sort(objComparer);
        }

        /****************************************************************************/
        public bool ContainsKey(K objKey)
        {
            return(m_ObjectDict.ContainsKey(objKey));
        }

        /****************************************************************************/
        public int IndexOfKey(K objKey)
        {
            int i = 0;

            foreach(IDictionaryEntry<K> objItem in this.m_ObjectDict)
            {
                if(objItem.Key(KeyIndex).Equals(objKey))
                    return(i);

                ++i;
            }

            throw new ArgumentOutOfRangeException();
        }

        /****************************************************************************/
        public int IndexOf(V objValue)
        {
            return(m_ObjectDict.IndexOf(objValue));
        }

        /****************************************************************************/
        public V LastItem
        {
            get
            {
                return(m_ObjectDict.GetValueAt(m_ObjectDict.Count-1));
            }
        }

        /****************************************************************************/
        public V ItemBefore(V objItem)
        {
            return(m_ObjectDict.ItemBefore(objItem));
        }

        /****************************************************************************/
        public V ItemAfter(V objItem)
        {
            return(m_ObjectDict.ItemAfter(objItem));
        }

        /****************************************************************************/
        public void Switch(V objFirst, V objSecond)
        {
            if(m_bReadOnly)
                throw new NotSupportedException();

            m_ObjectDict.Switch(objFirst, objSecond);

            return;
        }

        /****************************************************************************/
        public virtual void Add(V objAdd)
        {
            if(m_bReadOnly)
                throw new NotSupportedException();

            m_ObjectDict.Add(objAdd.Key(this.KeyIndex), objAdd);
        }

        /****************************************************************************/
        public virtual void Remove(K Key)
        {
            if(m_bReadOnly)
                throw new NotSupportedException();

            m_ObjectDict.Remove(Key);
        }

        /****************************************************************************/
        public virtual void RemoveObject(V obj)
        {
            Remove(obj.Key(this.KeyIndex));
        }

        /****************************************************************************/
        public virtual void RemoveAt(int iIndex)
        {
            if(m_bReadOnly)
                throw new NotSupportedException();

            V objRemove = GetValueAt(iIndex);

            if(objRemove != null)
                Remove(objRemove.Key(this.KeyIndex));
        }

        /****************************************************************************/
        public virtual void ReplaceAt(int iIndex, V objAdd)
        {
            if(m_bReadOnly)
                throw new NotSupportedException();

            RemoveAt(iIndex);
            Insert(iIndex, objAdd);
        }

        /****************************************************************************/
        public virtual void Insert(int iIndex, V objAdd)
        {
            if(m_bReadOnly)
                throw new NotSupportedException();

            m_ObjectDict.Insert(iIndex, objAdd.Key(m_iKeyIndex), objAdd);
        }

        /****************************************************************************/
        public V GetValueAt(int iIndex)
        {
            return(m_ObjectDict.GetValueAt(iIndex));
        }

        /****************************************************************************/
        public virtual void Clear()
        {
            if(m_bReadOnly)
                throw new NotSupportedException();

            m_ObjectDict.Clear();
        }

        /****************************************************************************/
        public V this [K Key] 
        {
            get
            {             
                return(m_ObjectDict[Key]);
            }
        }

        /****************************************************************************/
        public virtual IEnumerator GetEnumerator() 
        {
            return(m_ObjectDict.GetEnumerator());
        }

        /****************************************************************************/
        public IDictionaryEnumerator GetDictionaryEnumerator()
        {
            return(m_ObjectDict.GetDictionaryEnumerator());
        }

        /****************************************************************************/
        public IEnumerator GetArrayEnumerator()
        {
            return(this.m_ObjectDict.GetArrayEnumerator());
        }

        /****************************************************************************/
        public IEnumerator GetKeysEnumerator() 
        {
            return(m_ObjectDict.GetKeysEnumerator());
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

        /****************************************************************************/
        public int Add(object value)
        {
            if(value is V)
                Add((V)value);

            return(-1);
        }

        /****************************************************************************/
        public bool Contains(object value)
        {
            if(value is V)
                return(m_ObjectDict.Contains(value));

            return(false);
        }

        /****************************************************************************/
        public int IndexOf(object value)
        {
            if(value is V)
                return(m_ObjectDict.IndexOf((V)value));

            return(-1);
        }

        /****************************************************************************/
        public void Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        /****************************************************************************/
        public bool IsFixedSize
        {
            get { return(m_ObjectDict.IsFixedSize); }
        }

        /****************************************************************************/
        public bool IsReadOnly
        {
            get { return(m_ObjectDict.IsReadOnly); }
        }

        /****************************************************************************/
        public void Remove(object value)
        {
            if(value is V)
                RemoveObject((V)value);
            if(value is K)
                Remove((K)value);
            else
                throw new ArgumentException();
        }

        /****************************************************************************/
        public object this[int index]
        {
            get
            {
                return(m_ObjectDict.GetValueAt(index));
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface IIdentifiable : IDictionaryEntry<Guid>
    {
        Guid Id {get;}
    }

    /****************************************************************************/
    /****************************************************************************/
	public class IdDictionary<V> : KeyedDictionary<Guid, V> where V : IIdentifiable
	{
        /****************************************************************************/
        public IdDictionary(int iInitialSize) : base(iInitialSize)
        {
        }

        /****************************************************************************/
        public new V this [int iIndex] 
        {
            get
            {             
                return(GetValueAt(iIndex));
            }
        }
    }
     
    /****************************************************************************/
    /****************************************************************************/
    public abstract class ObjectDictionary<K, V> : KeyedDictionary<K, V> where V : IDictionaryEntry<K>
    {
        /****************************************************************************/
        public ObjectDictionary(DataSourceList aObjects) : base(127)
        {
            Load(aObjects);
        }

        /****************************************************************************/
        public ObjectDictionary() : base(127)
        {
        }

        /****************************************************************************/
        public void Load(DataSourceList aObjects)
        {
            foreach(IDataObjectSource objRow in aObjects)
                this.Add(CreateObject(objRow));
        }

        /****************************************************************************/
        public abstract V CreateObject(IDataObjectSource objRow);
    }
}
