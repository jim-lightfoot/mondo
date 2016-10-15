/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: IdObject.cs										    */
/*        Class(es): IdObject										        */
/*          Purpose: A collection of keyed objects                          */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 12 Sep 2001                                            */
/*                                                                          */
/*   Copyright (c) 2011-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using Mondo.Common;

namespace Mondo.Threading
{
    /****************************************************************************/
    /****************************************************************************/
    public class IdObject : IDictionaryEntry<string>
    {
        private readonly ThreadVar<string> m_idObject = new ThreadVar<string>();

        /****************************************************************************/
        public IdObject(string strId)
        {
            m_idObject.Value = strId;
        }

        /****************************************************************************/
        protected IdObject()
        {
        }

        /****************************************************************************/
        public string Id       {get{return(m_idObject.Value);}}

        /****************************************************************************/
        protected void SetId(string strId)
        {
            m_idObject.Value = strId;
        }

        /****************************************************************************/
        public virtual string Key(int i)
        {
            return(this.Id);
        }
    }
        
    /****************************************************************************/
    /****************************************************************************/
    public class UniquelyIdentifiable : IdObject
    {
        /****************************************************************************/
        public UniquelyIdentifiable()
        {
            SetId(Guid.NewGuid().ToString().ToUpper());
        }

        /****************************************************************************/
        public override bool Equals(object obj)
        {
            if(obj == null)
                return(this == null);

            if(obj is UniquelyIdentifiable)
                return((obj as UniquelyIdentifiable).Id == this.Id);

            return(obj.Normalized().ToUpper() == this.Id);
        }

        /****************************************************************************/
        public override int GetHashCode()
        {
            return(this.Id.GetHashCode());
        }
    }
}
