/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: Model.cs					    		                */
/*        Class(es): Model				         		                    */
/*          Purpose: Generic data item                                      */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 28 Sep 2003                                            */
/*                                                                          */
/*   Copyright (c) 2003-2007 - Tenth Generation Software, LLC               */
/*                          All rights reserved                             */
/*                                                                          */
/****************************************************************************/

using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public class TextModel : Model
    {
        private string m_strData = "";

        /****************************************************************************/
        public TextModel()
        {
        }

        /****************************************************************************/
        public TextModel(string strData)
        {
            m_strData = strData;
        }

        /****************************************************************************/
        public string Data
        {
            get {return(m_strData);}
            set {Modify(ref m_strData, value);}
        }

        /****************************************************************************/
        public override string ToString()
        {
            return(m_strData);
        }
    }
}
