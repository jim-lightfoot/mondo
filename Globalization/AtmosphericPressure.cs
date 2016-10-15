/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Globalization							        */
/*             File: AtmosphericPressure.cs									*/
/*        Class(es): AtmosphericPressure									*/
/*          Purpose: Conversion and formatting of AtmosphericPressure units */
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
    public class AtmosphericPressure : Measurement
    {
        /****************************************************************************/
        public AtmosphericPressure(double dValue)
        {
            // Store as InHg32 (Inches of Mercury at 32 F)
            this.Value = dValue;
        }

        /****************************************************************************/
        public AtmosphericPressure(decimal dValue)
        {
            this.Value = (double)dValue;
        }

        /****************************************************************************/
        public double KiloPascals
        {
            get {return(this.Value * 3.38639);}
            set {this.Value = value / 3.38639;}
        }

        /****************************************************************************/
        public double Millibars
        {
            get {return(this.KiloPascals * 10d);}
            set {this.KiloPascals = value / 10d;}
        }

        /****************************************************************************/
        public double HectoPascals
        {
            get {return(this.Millibars);}
            set {this.Millibars = value;}
        }

        /****************************************************************************/
        public static double ToKiloPascals(double dValue)
        {
            return(dValue * 3.38639);
        }

        /****************************************************************************/
        public static double ToHectoPascals(double dValue)
        {
            return(ToKiloPascals(dValue) * 10d);
        }

        /****************************************************************************/
        public double InchesOfMercury
        {
            get {return(this.Value);}
            set {this.Value = value;}
        }

        /****************************************************************************/
        public string ToString(bool bStandard)
        {
            return(ToString(bStandard, 1, true));
        }

        /****************************************************************************/
        public string ToString(bool bStandard, int nDecimalPlaces, bool bLong)
        {
            string strUOM = bLong ? (" " + Symbol(bStandard)) : "";
            double dValue = bStandard ? this.InchesOfMercury : this.HectoPascals;

            return(string.Format("{0:f" + nDecimalPlaces.ToString() + "}{1}", dValue, strUOM));
        }

        /****************************************************************************/
        public static string Symbol(bool bStandard)
        {
            return(bStandard ? Resource.AtmospericPressure_InchesMercury : Resource.AtmospericPressure_HectoPascals);
        }
    }
}
