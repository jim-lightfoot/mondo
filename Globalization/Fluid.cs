/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Globalization							        */
/*             File: Fluid.cs										        */
/*        Class(es): Fluid									                */
/*          Purpose: Conversion and formatting of Fluid units               */
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
    public class Fluid : Measurement
    {
        /****************************************************************************/
        public Fluid(double dValue)
        {
            // Store as gallons
            this.Value = dValue;
        }

         /****************************************************************************/
        public Fluid(decimal dValue)
        {
            this.Value = (double)dValue;
        }

        /****************************************************************************/
        public enum Size
        {
            Small,       // oz/ml
            Large        // gal/l
        }

        /****************************************************************************/
        public double Milliliters
        {
            get {return(this.Liters * 1000.0);}
            set {this.Liters = value/1000.0;}
        }

        /****************************************************************************/
        public double Liters
        {
            get {return(this.Value * 3.7854);}
            set {this.Value = value / 3.7854;}
        }

        /****************************************************************************/
        public static double ToLiters(double dValue)
        {
            return(dValue * 3.7854);
        }

        /****************************************************************************/
        public double Ounces
        {
            get {return(this.Gallons * 128d);}
            set {this.Gallons = value / 128d;}
        }

        /****************************************************************************/
        public double Gallons
        {
            get {return(this.Value);}
            set {this.Value = value;}
        }

        /****************************************************************************/
        public string ToString(bool bStandard, Fluid.Size eSize)
        {
            return(ToString(bStandard, 1, true, eSize));
        }

        /****************************************************************************/
        public string ToString(bool bStandard, int nDecimalPlaces, bool bLong, Fluid.Size eSize)
        {
            string strUOM = bLong ? (" " + Symbol(bStandard, eSize)) : "";
            double dValue = 0.0;

            if(eSize == Size.Small)
                dValue = bStandard ? this.Ounces : this.Milliliters;
            else
                dValue = bStandard ? this.Gallons : this.Liters;      

            return(string.Format("{0:f" + nDecimalPlaces.ToString() + "}{1}", dValue, strUOM));
        }

        /****************************************************************************/
        public static string Symbol(bool bStandard, Fluid.Size eSize)
        {
            if(eSize == Size.Small)
                return(bStandard ? Resource.Fluid_Ounces : Resource.Fluid_Milliliters);

            return(bStandard ? Resource.Fluid_Gallons : Resource.Fluid_Liters);
        }
    }
}
