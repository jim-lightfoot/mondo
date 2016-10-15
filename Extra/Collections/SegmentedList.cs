/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: SegmentedList.cs										*/
/*        Class(es): SegmentedList										    */
/*          Purpose: Multiple lists grouped together to act as one          */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 8 Jan 2010                                             */
/*                                                                          */
/*   Copyright (c) 2010 - Tenth Generation Software - All rights reserved   */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public class SegmentedList<T> : IList<T>, IList
    {
        private readonly List< IList<T> > m_aSegments = new List<IList<T>>();

        /****************************************************************************/
        public SegmentedList()
        {
        }

        /****************************************************************************/
        public void AddSegment(IList<T> aSegment)
        {
            m_aSegments.Add(aSegment);
        }

        #region IList<T> Members

        /****************************************************************************/
        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        /****************************************************************************/
        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        /****************************************************************************/
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        /****************************************************************************/
        public T this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<T> Members

        /****************************************************************************/
        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        /****************************************************************************/
        public void Clear()
        {
            m_aSegments.Clear();
        }

        /****************************************************************************/
        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        /****************************************************************************/
        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /****************************************************************************/
        public int Count
        {
            get 
            {
                int iCount = 0;

                foreach(IList<T> aSegment in m_aSegments)
                    iCount += aSegment.Count;

                return(iCount);           
            }
        }

        /****************************************************************************/
        public bool IsReadOnly
        {
            get { return(false); }
        }

        /****************************************************************************/
        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<T> Members

        /****************************************************************************/
        public IEnumerator<T> GetEnumerator()
        {
            return(new MyEnumerator(this));
        }

        #endregion

        #region IEnumerable Members

        /****************************************************************************/
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return(new MyEnumerator(this));
        }

        #endregion

        /****************************************************************************/
        private class MyEnumerator : IEnumerator<T>
        {
            private SegmentedList<T> m_aList;
            private int              m_iCount   = 0;
            private int              m_iSegmentIndex   = -1;
            private int              m_iIndex   = -1;
            private IList<T>         m_aSegment = null;

            /****************************************************************************/
            internal MyEnumerator(SegmentedList<T> aList)
            {
                m_aList = aList;
                m_iCount = aList.Count;

                Reset();
            }

            #region IEnumerator<T> Members

            /****************************************************************************/
            public T Current
            {
                get { return(m_aSegment[m_iIndex]); }
            }

            #endregion

            #region IDisposable Members

            /****************************************************************************/
            public void Dispose()
            {
            }

            #endregion

            #region IEnumerator Members

            /****************************************************************************/
            object System.Collections.IEnumerator.Current
            {
                get { return(m_aSegment[m_iIndex]); }
            }

            /****************************************************************************/
            public bool MoveNext()
            {
                if(m_aSegment == null)
                    return(false);

                while(true)
                {
                    if(++m_iIndex < m_aSegment.Count)
                        break;

                    if(++m_iSegmentIndex >= m_aList.m_aSegments.Count)
                        return(false);

                    m_iIndex = -1;
                    m_aSegment = m_aList.m_aSegments[m_iSegmentIndex];
                }

                return(true);
            }

            /****************************************************************************/
            public void Reset()
            {
                m_iSegmentIndex = 0;
                m_iIndex        = 0;

                if(m_aList.m_aSegments.Count > 0)
                    m_aSegment = m_aList.m_aSegments[0];
                else
                    m_aSegment = null;
            }

            #endregion
        }

        #region IList Members

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public bool IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        object IList.this[int index]
        {
            get
            {
                int nSoFar = 0;

                foreach(IList<T> aSegment in m_aSegments)
                {
                    if((index - nSoFar) < aSegment.Count)
                        return(aSegment[index - nSoFar]);

                    nSoFar += aSegment.Count;
                }

                throw new Exception();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
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
    }
}
