/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: ITextFormatter.cs							 		    */
/*        Class(es): ITextFormatter, TextFormatter                          */
/*          Purpose: Interface and base class for formatting text           */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 29 Nov 2015                                            */
/*                                                                          */
/*   Copyright (c) 2015-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Globalization;
using Mondo.Common;
using Mondo.Xml;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public interface ITextFormatter
    {
        string  Format(string strText);
        bool    Validate(string strText);
        bool    CanInsertChar(string strText, ref char chValue, int iIndex);
    }
    
    /****************************************************************************/
    /****************************************************************************/
    public class TextFormatter : ITextFormatter
    {
        /****************************************************************************/
        public virtual string Format(string strText)
        {   
            return(strText);
        }
        
        /****************************************************************************/
        public virtual bool Validate(string strText)
        {
            return(true);
        }
        
        /****************************************************************************/
        public virtual bool CanInsertChar(string strText, ref char chValue, int iIndex)
        {
            return(true);
        }
    }
    
    /****************************************************************************/
    /****************************************************************************/
    public class DigitFormatter : TextFormatter
    {
        /****************************************************************************/
        public DigitFormatter()
        {
        }
        
        /****************************************************************************/
        public override bool CanInsertChar(string strText, ref char chValue, int iIndex)
        {
            return(char.IsDigit(chValue));
        }
    }
}
