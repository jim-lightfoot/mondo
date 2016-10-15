/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: ThreadLock.cs										    */
/*        Class(es): ThreadLock										        */
/*          Purpose: Provide a lock to access data in a thread-safe manner  */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 28 Jun 2009                                            */
/*                                                                          */
/*   Copyright (c) 2009-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Specialized;
using Mondo.Common;

namespace Mondo.Threading
{
    /****************************************************************************/
    /****************************************************************************/
    public sealed class ThreadLock
    {
        private readonly ReaderWriterLockSlim m_objLock      = new ReaderWriterLockSlim();
        private readonly ThreadLock.ReadLock  m_objReadLock;
        private readonly ThreadLock.WriteLock m_objWriteLock;
        private          int m_iTimeout;

        /****************************************************************************/
        public ThreadLock() : this(2000)
        {
        }

        /****************************************************************************/
        public ThreadLock(int iTimeout)
        {
            m_iTimeout = iTimeout;

            m_objReadLock  = new ReadLock(this);
            m_objWriteLock = new WriteLock(this);
        }

        /****************************************************************************/
        public ThreadLock.ReadLock  Read    {get{return(m_objReadLock);}}
        public ThreadLock.WriteLock Write   {get{return(m_objWriteLock);}}

        /****************************************************************************/
        /****************************************************************************/
        public class LockTimeOut : Exception
        {
            public LockTimeOut() : base("Timed out while waiting to lock variable.")
            {
            }
        }

        /****************************************************************************/
        /****************************************************************************/
        public class ReadLock : Openable
        {
            private readonly ThreadLock m_objLock;

            /****************************************************************************/
            public ReadLock(ThreadLock objLock)
            {
                m_objLock = objLock;
            }

            /****************************************************************************/
            public override void Open()
            {
                if(!IsOpen)
                    m_objLock.EnterReadLock();

                base.Open();
            }

            /****************************************************************************/
            public void OpenUpgradeable()
            {
                if(!IsOpen)
                    m_objLock.EnterUpgradeableReadLock();

                base.Open();
            }

            /****************************************************************************/
            public override void Close()
            {
                base.Close();

                if(!IsOpen)
                    m_objLock.ExitReadLock();
            }

            /****************************************************************************/
            public void CloseUpgradeable()
            {
                base.Close();

                if(!IsOpen)
                    m_objLock.ExitUpgradeableReadLock();
            }

            /****************************************************************************/
            public AcquireUpgradeableLock AcquireUpgradeable
            {
                get
                {
                    return(new AcquireUpgradeableLock(this));
                }
            }

            /****************************************************************************/
            /****************************************************************************/
            public class AcquireUpgradeableLock : IDisposable
            {
                private ReadLock m_objLock = null;

                /****************************************************************************/
                public AcquireUpgradeableLock(ReadLock objLock)
                {
                    m_objLock = objLock;
                    m_objLock.OpenUpgradeable();
                }

                /****************************************************************************/
                public void Dispose()
                {
                    m_objLock.CloseUpgradeable();
                }
            }
        }

        /****************************************************************************/
        /****************************************************************************/
        public class WriteLock : Openable
        {
            private readonly ThreadLock m_objLock;

            /****************************************************************************/
            public WriteLock(ThreadLock objLock)
            {
                m_objLock = objLock;
            }

            /****************************************************************************/
            public override void Open()
            {
                m_objLock.EnterWriteLock();
            }

            /****************************************************************************/
            public override void Close()
            {
                m_objLock.ExitWriteLock();
            }
       }

        #region Private Methods

        /****************************************************************************/
        private void EnterReadLock()
        {
            if(!m_objLock.TryEnterReadLock(m_iTimeout))
                throw new LockTimeOut();
        }

        /****************************************************************************/
        private void EnterUpgradeableReadLock()
        {
            if(!m_objLock.TryEnterUpgradeableReadLock(m_iTimeout))
                throw new LockTimeOut();
        }

        /****************************************************************************/
        private void ExitReadLock()
        {
            m_objLock.ExitReadLock();
        }

        /****************************************************************************/
        private void ExitUpgradeableReadLock()
        {
            m_objLock.ExitUpgradeableReadLock();
        }

        /****************************************************************************/
        private void EnterWriteLock()
        {
            if(!m_objLock.TryEnterWriteLock(m_iTimeout))
                throw new LockTimeOut();
        }

        /****************************************************************************/
        private void ExitWriteLock()
        {
            m_objLock.ExitWriteLock();
        }

        #endregion
    }

    /****************************************************************************/
    /****************************************************************************/
     public class ThreadVar<T>
    {
        protected T                   m_objVar  = default(T);
        protected readonly ThreadLock m_objLock = new ThreadLock(10000);

        /****************************************************************************/
        public ThreadVar()         
        {
        }

        /****************************************************************************/
        public T Value             
        {
            get
            {
                using(m_objLock.Read.Acquire)
                {
                    return(m_objVar);
                }
            }
             
            set
            {
                using(m_objLock.Write.Acquire)
                {
                    m_objVar = value;
                }
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class ThreadCounter : ThreadVar<int>
    {
        /****************************************************************************/
        public ThreadCounter()         
        {
        }

        /****************************************************************************/
        public static implicit operator int(ThreadCounter objCounter)            
        {
            return(objCounter.Value);
        }

        /****************************************************************************/
        public static ThreadCounter operator++(ThreadCounter objCounter)            
        {
            using(objCounter.m_objLock.Write.Acquire)
            {
                ++objCounter.m_objVar;
            }

            return(objCounter);
        }

        /****************************************************************************/
        public static ThreadCounter operator--(ThreadCounter objCounter)            
        {
            using(objCounter.m_objLock.Write.Acquire)
            {
                --objCounter.m_objVar;
            }

            return(objCounter);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public sealed class ThreadFlag : ThreadVar<bool>
    {
        /****************************************************************************/
        public ThreadFlag()         
        {
             Value = false;
        }

        /****************************************************************************/
        public static implicit operator bool(ThreadFlag objFlag)            
        {
            return(objFlag.Value);
        }

        /****************************************************************************/
        public static bool operator true(ThreadFlag objFlag)
        {
            return(objFlag.Value);
        }

        /****************************************************************************/
        public static bool operator false(ThreadFlag objFlag)
        {
            return(!objFlag.Value);
        }

        /****************************************************************************/
        public bool Set()
        {
            using(m_objLock.Read.AcquireUpgradeable)
            {
                if(m_objVar)
                    return(false);

                using(m_objLock.Write.Acquire)
                {
                    m_objVar = true;
                    return(true);
                }
            }
        }
    }
}
