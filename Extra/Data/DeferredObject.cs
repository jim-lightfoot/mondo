/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: DeferredObject.cs										*/
/*        Class(es): DeferredObject										    */
/*          Purpose: Defers loading or creation of an object until it's     */
/*                      accessed                                            */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 23 Apr 2012                                            */
/*                                                                          */
/*   Copyright (c) 2012 - Tenth Generation Software, LLC                    */
/*                              All rights reserved                         */
/*                                                                          */
/****************************************************************************/

using System;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
	public abstract class DeferredObject<T> 
	{
        private T m_objValue = default(T);
        private bool m_bLoaded = false;

		/****************************************************************************/
        public DeferredObject()
		{
		}

        /****************************************************************************/
        public T Value
        {
            get
            {
                if(!m_bLoaded)
                {
                    m_objValue = Load();
                    m_bLoaded = true;
                }

                return(m_objValue);
            }
        }

        /****************************************************************************/
        public bool IsLoaded
        {
            get {return(m_bLoaded);}
        }

        /****************************************************************************/
        public abstract T Load();
    }
}
