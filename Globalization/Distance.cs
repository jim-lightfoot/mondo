/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Globalization							        */
/*             File: Distance.cs										    */
/*        Class(es): Distance									            */
/*          Purpose: Conversion and formatting of Distance units            */
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
    public class Distance : Measurement
    {
        /****************************************************************************/
        public Distance(double dValue)
        {
            // Store as inches
            this.Value = dValue;
        }

        /****************************************************************************/
        public Distance(decimal dValue)
        {
            this.Value = (double)dValue;
        }

        /****************************************************************************/
        public enum Size
        {
            VerySmall,   // in/mm
            Small,       // in/cm
            Medium,      // ft/m
            Large        // mi/km
        }

        /****************************************************************************/
        public double Millimeters
        {
            get {return(this.Centimeters * 10.0);}
            set {this.Centimeters = value/10.0;}
        }

        /****************************************************************************/
        public double Centimeters
        {
            get {return(this.Meters * 10.0);}
            set {this.Meters = value/10.0;}
        }

        /****************************************************************************/
        public double Meters
        {
            get {return(this.Value * 0.3048);}
            set {this.Value = value / 0.3048;}
        }

        /****************************************************************************/
        public double Kilometers
        {
            get {return(this.Meters / 1000.0);}
            set {this.Meters  = value * 1000.0;}
        }

        /****************************************************************************/
        public double Inches
        {
            get {return(this.Value);}
            set {this.Value = value;}
        }

        /****************************************************************************/
        public double Feet
        {
            get {return(this.Inches / 12.0);}
            set {this.Inches = value * 12.0;}
        }

        /****************************************************************************/
        public double Miles
        {
            get {return(this.Feet / 5280.0);}
            set {this.Feet = value * 5280.0;}
        }

        /****************************************************************************/
        public string ToString(bool bStandard, Distance.Size eSize)
        {
            return(ToString(bStandard, 1, true, eSize));
        }

        /****************************************************************************/
        public string ToString(bool bStandard, int nDecimalPlaces, bool bLong, Distance.Size eSize)
        {
            string strUOM = bLong ? (" " + Symbol(bStandard, eSize)) : "";
            double dValue = 0.0;

            switch(eSize)
            {
                case Size.VerySmall: dValue = bStandard ? this.Inches : this.Millimeters;   break;
                case Size.Small:     dValue = bStandard ? this.Inches : this.Centimeters;   break;
                case Size.Medium:    dValue = bStandard ? this.Feet   : this.Meters;        break;
                default:             dValue = bStandard ? this.Miles  : this.Kilometers;    break;
            }

            return(string.Format("{0:f" + nDecimalPlaces.ToString() + "}{1}", dValue, strUOM));
        }

        /****************************************************************************/
        public static string Symbol(bool bStandard, Distance.Size eSize)
        {
            switch(eSize)
            {
                case Size.VerySmall: return(bStandard ? Resource.Distance_Inches : Resource.Distance_Millimeters);
                case Size.Small:     return(bStandard ? Resource.Distance_Inches : Resource.Distance_Centimeters);
                case Size.Medium:    return(bStandard ? Resource.Distance_Feet   : Resource.Distance_Meters);
                default:             return(bStandard ? Resource.Distance_Miles  : Resource.Distance_Kilometers);
            }
        }
    }
}
