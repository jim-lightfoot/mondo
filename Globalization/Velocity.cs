/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Globalization							        */
/*             File: Velocity.cs										    */
/*        Class(es): Velocity									            */
/*          Purpose: Conversion and formatting of Velocity units            */
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
    public class Velocity : Measurement
    {
        /****************************************************************************/
        public Velocity(double dValue)
        {
            this.MPH = dValue;
        }

        /****************************************************************************/
        public Velocity(decimal dValue)
        {
            this.MPH = (double)dValue;
        }

        /****************************************************************************/
        public double KPH
        {
            get {return(ToKPH(this.Value));}
            set {this.Value = ToMPH(value);}
        }

        /****************************************************************************/
        public double MPH
        {
            get {return(this.Value);}
            set {this.Value = value;}
        }

        /****************************************************************************/
        public static double ToKPH(double dValue)
        {
            return(dValue * 1.609344);
        }

        /****************************************************************************/
        public static double ToMPH(double dValue)
        {
            return(dValue / 1.609344);
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
            double dValue = bStandard ? this.MPH : this.KPH;

            return(string.Format("{0:f" + nDecimalPlaces.ToString() + "}{1}", dValue, strUOM));
        }

        /****************************************************************************/
        public static string Symbol(bool bStandard)
        {
            return(bStandard ? Resource.Velocity_MPH : Resource.Velocity_KPH);
        }
    }
}
