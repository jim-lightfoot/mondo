/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Globalization							        */
/*             File: ITextFormatter.cs							 		    */
/*        Class(es): ITextFormatter, TextFormatter                          */
/*          Purpose: Interface and base class for formatting text           */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 29 Nov 2015                                            */
/*                                                                          */
/*   Copyright (c) 2015 - Jim Lightfoot, All rights reserved                */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Globalization;
using Mondo.Common;
using Mondo.Xml;

namespace Mondo.Globalization
{
    /****************************************************************************/
    /****************************************************************************/
    public class NumberFormatter : DigitFormatter
    {
        protected readonly NumberFormatInfo m_objFormat;
        
        /****************************************************************************/
        public NumberFormatter()
        {
            m_objFormat = CultureInfo.CurrentCulture.NumberFormat;
        }
        
        /****************************************************************************/
        public NumberFormatter(NumberFormatInfo formatInfo)
        {
            m_objFormat = formatInfo;
        }
        
        /****************************************************************************/
        public override string Format(string strText)
        {   
            return(Utility.ToDecimal(strText).ToString());
        }
        
        /****************************************************************************/
        public override bool Validate(string strText)
        {
            if(strText.Trim() == "")
                return(true);
                
            decimal dResult = 0M;
            
            return(decimal.TryParse(strText, out dResult));
        }
        
        /****************************************************************************/
        public override bool CanInsertChar(string strText, ref char chValue, int iIndex)
        {
            if(base.CanInsertChar(strText, ref chValue, iIndex))
                return(true);
                
            string strValue = chValue.ToString();
            
            if(strValue == m_objFormat.NegativeSign)
                return(true);
            
            if(strValue == m_objFormat.NumberDecimalSeparator)
                return(true);
                           
            if(strValue == m_objFormat.NumberGroupSeparator)
                return(true);
                
            return(false);
        }
    }
    
    /****************************************************************************/
    /****************************************************************************/
    public class CurrencyFormatter : NumberFormatter
    {       
        /****************************************************************************/
        public CurrencyFormatter()
        {
        }
        
        /****************************************************************************/
        public CurrencyFormatter(NumberFormatInfo formatInfo) : base(formatInfo)
        {
        }
        
        /****************************************************************************/
        public override string Format(string strText)
        {   
            return(Utility.ToCurrency(strText).ToString("c"));
        }
        
        /****************************************************************************/
        public override bool Validate(string strText)
        {
            if(strText.Trim() == "")
                return(true);
                
            decimal dResult = 0M;
            
            return(decimal.TryParse(strText, System.Globalization.NumberStyles.Currency, m_objFormat, out dResult));
        }
        
        /****************************************************************************/
        public override bool CanInsertChar(string strText, ref char chValue, int iIndex)
        {
            if(char.IsDigit(chValue))
                return(true);
                
            string strValue = chValue.ToString();
            
            if(strValue == m_objFormat.NegativeSign)
                return(true);
            
            if(strValue == m_objFormat.CurrencyDecimalSeparator)
                return(true);
            
            if(strValue == m_objFormat.CurrencyGroupSeparator)
                return(true);
                           
            if(strValue == m_objFormat.CurrencySymbol)
                return(true);
                
            return(false);
        }
    }
        
    /****************************************************************************/
    /****************************************************************************/
    public class PercentFormatter : NumberFormatter
    {       
        /****************************************************************************/
        public PercentFormatter()
        {
        }
        
        /****************************************************************************/
        public PercentFormatter(NumberFormatInfo formatInfo) : base(formatInfo)
        {
        }
        
        /****************************************************************************/
        public override string Format(string strText)
        {   
            float fValue = Utility.ToFloat(strText.Replace(m_objFormat.PercentSymbol, ""));
            
            return(string.Format("{0:p}", fValue/100f));
        }
        
        /****************************************************************************/
        public override bool Validate(string strText)
        {
            if(strText.Trim() == "")
                return(true);
                
            float fResult = 0f;
            
            return(float.TryParse(strText.Replace(m_objFormat.PercentSymbol, ""), out fResult));
        }
        
        /****************************************************************************/
        public override bool CanInsertChar(string strText, ref char chValue, int iIndex)
        {
            if(char.IsDigit(chValue))
                return(true);
                
            string strValue = chValue.ToString();
            
            if(strValue == m_objFormat.NegativeSign)
                return(true);
            
            if(strValue == m_objFormat.PercentDecimalSeparator)
                return(true);
            
            if(strValue == m_objFormat.PercentGroupSeparator)
                return(true);
                           
            if(strValue == m_objFormat.PercentSymbol)
                return(true);
                
            return(false);
        }
    }
}
