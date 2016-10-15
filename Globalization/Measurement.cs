/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Globalization							        */
/*             File: Measurement.cs										    */
/*        Class(es): Measurement									        */
/*          Purpose: Conversion and formatting of units of measure          */
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
    /* ??? TODO: Add:
     * 
     *  1. Area (acre/hectare) 
     *  2. Weight
     *  3. Volume
     *  4. Latitude/Longitude
     *  5. Direction
     *  6. Voltage
     *  7. Solar Radiation
     *  8. Water content (i.e. soil moisture)
     * /

    /****************************************************************************/
    /****************************************************************************/
    public class Measurement
    {
        private double m_dValue = 0.0;

        /****************************************************************************/
        public Measurement() 
        {
        }

        /****************************************************************************/
        public double Value
        {
            get {return(m_dValue);}
            set {m_dValue = value;}
        }

        /*****************************************************************************/
        public static double GetFuelLevel(double dGallons, IGlobalization objGlobalization)
        {
            if(!objGlobalization.IsMetric)
                return(dGallons);

            return(Fluid.ToLiters(dGallons));
        }

        /*****************************************************************************/
        public static double GetTemperature(double dFahrenheit, IGlobalization objGlobalization)
        {
            if(!objGlobalization.IsMetric)
                return(dFahrenheit);

            return(Temperature.ToCelsius(dFahrenheit));
        }

        /*****************************************************************************/
        public static double GetAtmosphericPressure(double dInches, IGlobalization objGlobalization)
        {
            if(!objGlobalization.IsMetric)
                return(dInches);

            return(AtmosphericPressure.ToHectoPascals(dInches));
        }

        /*****************************************************************************/
        public static double GetWindSpeed(double dMPH, IGlobalization objGlobalization)
        {
            if(!objGlobalization.IsMetric)
                return(dMPH);

            return(Velocity.ToKPH(dMPH));
        }

        /*****************************************************************************/
        public static double GetRainfall(double dValue, IGlobalization objGlobalization)
        {
            if(!objGlobalization.IsMetric)
                return(dValue);

            Distance objDistance = new Distance(dValue);

            return(objDistance.Millimeters);
        }

        /*****************************************************************************/
        public static double GetRainRate(double dValue, IGlobalization objGlobalization)
        {
            return(GetRainfall(dValue, objGlobalization));
        }
    }
}
