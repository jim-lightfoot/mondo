/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: Acquire.cs										        */
/*        Class(es): Acquire										        */
/*          Purpose: A utility class that opens a Openable object           */
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

namespace Mondo.Common
{
	/*************************************************************************/
    /*************************************************************************/
    /// <summary>
	/// A utility class that opens an Openable object
	///   Must be used in conjunction with "using" block statement, i.e.
	///   
	///     DerivedOpeneable objMyObject = new DerivedOpeneable();
	///     
	///     using(Acquire open = new Acquire(objMyObject))
	///     {
	///        // ... do something here
	///     }
	///     
	/// </summary>
	public class Acquire : IDisposable
	{
        protected IOpenable m_objOpenable;

        /*************************************************************************/
        public Acquire(IOpenable objOpenable)
        {
            m_objOpenable = objOpenable;
            m_objOpenable.Open();
        }

        /*************************************************************************/
        protected Acquire()
        {
        }

        /*************************************************************************/
        public void Dispose()
        {
            m_objOpenable.Close();
            m_objOpenable = null;
        }
	}
}
