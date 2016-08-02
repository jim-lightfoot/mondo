/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: Openable.cs										    */
/*        Class(es): Openable										        */
/*          Purpose: An abtract class for any object that can be opened     */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 12 Sep 2001                                            */
/*                                                                          */
/*   Copyright (c) 2001-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Threading.Tasks;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
	/// <summary>
	/// An interface for any object that can be opened
	/// </summary>
	public interface IOpenable 
	{
        void Open();
        Task OpenAsync();
        void Close();
        bool IsOpen      {get;}
    }

    /****************************************************************************/
    /****************************************************************************/
	/// <summary>
	/// An abtract class for any object that can be opened
	/// </summary>
	public abstract class Openable : IOpenable, IDisposable
	{
        private int m_iOpen = 0;

		/****************************************************************************/
        public Openable()
		{
		}

        /****************************************************************************/
        public virtual void Open()
        {
            ++m_iOpen;
        }

        /****************************************************************************/
        public virtual async Task OpenAsync()
        {
            Open();
        }

        /****************************************************************************/
        public virtual void Close()
        {
            if(m_iOpen > 0)
                --m_iOpen;
        }

        /****************************************************************************/
        public bool IsOpen
        {
            get
            {
                return(m_iOpen > 0);
            }
        }

        /****************************************************************************/
        protected int OpenCount
        {
            get
            {
                return(m_iOpen);
            }
        }

        /****************************************************************************/
        public Acquire Acquire
        {
            get
            {
                return(new Acquire(this));
            }
        }

        /****************************************************************************/
        public async Task<Acquire> AcquireAsync()
        {
            AsyncAcquire acquire = new AsyncAcquire(this);

            return await acquire.Open();
        }
        
	    /*************************************************************************/
        /*************************************************************************/
        private class AsyncAcquire : Acquire
	    {
            /*************************************************************************/
            public AsyncAcquire(IOpenable objOpenable)
            {
                m_objOpenable = objOpenable;
            }

            /*************************************************************************/
            public async Task<Acquire> Open()
            {
                await m_objOpenable.OpenAsync();

                return this;
            }
	    }

        #region IDisposable Members

        /****************************************************************************/
        public virtual void Dispose()
        {
            if(this.IsOpen)
                Close();
        }

        #endregion
    }
}
