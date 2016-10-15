/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Globalization							        */
/*             File: Temperature.cs										    */
/*        Class(es): Temperature									        */
/*          Purpose: Conversion and formatting of temperature               */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 12 Jun 2009                                            */
/*                                                                          */
/*   Copyright (c) 2009 - Tenth Generation Software                         */
/*                          All rights reserved                             */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Mondo.Globalization
{
    /****************************************************************************/
    /****************************************************************************/
    public class Temperature : Measurement
    {
        /****************************************************************************/
        public Temperature(double dValue)
        {
            this.Value = dValue;
        }

        /****************************************************************************/
        public Temperature(decimal dValue)
        {
            this.Value = (double)dValue;
        }

        /****************************************************************************/
        public double Fahrenheit
        {
            get {return(this.Value);}
            set {this.Value = value;}
        }

        /****************************************************************************/
        public double Celsius
        {
            set {this.Value = ToFahrenheit(value);}
            get {return(ToCelsius(this.Value));}
        }

        /****************************************************************************/
        public static double ToCelsius(double dValue)
        {
            return(((dValue - 32.0) * 5.0) / 9.0);
        }

        /****************************************************************************/
        public static double ToFahrenheit(double dValue)
        {
            return(((dValue * 9.0) / 5.0) + 32.0);
        }

        /****************************************************************************/
        public string ToString(bool bStandard)
        {
            return(ToString(bStandard, 1, true));
        }

        /****************************************************************************/
        public string ToString(bool bStandard, int nDecimalPlaces, bool bLong)
        {
            string strUOM = bLong ? Symbol(bStandard) : Resource.Temperature_Symbol;
            double dValue = bStandard ? this.Fahrenheit : this.Celsius;

            return(string.Format("{0:f" + nDecimalPlaces.ToString() + "}{1}", dValue, strUOM));
        }

        /****************************************************************************/
        public static string Symbol(bool bStandard)
        {
            return(bStandard ? Resource.Temperature_Fahrenheit : Resource.Temperature_Celsius);
        }
    }
}
