/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Globalization							        */
/*             File: IGlobalization.cs							 		    */
/*        Class(es): IGlobalization, GlobalizationObject                    */
/*          Purpose: Interface and base class for globalization             */
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
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Globalization;

using Mondo.Common;
using Mondo.Xml;

namespace Mondo.Globalization
{
    /*********************************************************************/
    /*********************************************************************/
    public interface IGlobalization
    {
        bool                    IsMetric        {get;}
        TimeZoneInfo            TimeZone        {get;}
        CultureInfo             Culture         {get;}
        DateTimeFormatInfo      DateTimeFormat  {get;}
        NumberFormatInfo        NumberFormat    {get;}
        RegionInfo              Region          {get;}
    }

    /*********************************************************************/
    /*********************************************************************/
    public static class Globalization
    {
        private static readonly TimeZoneList s_aTimeZones = new TimeZoneList();

        /*************************************************************************/
        static Globalization()
        {
            ReadOnlyCollection<TimeZoneInfo> aTimeZones = TimeZoneInfo.GetSystemTimeZones();

            foreach(TimeZoneInfo objTimeZone in aTimeZones)
                s_aTimeZones.Add(objTimeZone.Id, objTimeZone);
                            
            s_aTimeZones.Sort(new TimeZoneComparer());
        }

        /*************************************************************************/
        /*************************************************************************/
        public class TimeZoneList : OrderedDictionary<string, TimeZoneInfo>
        {
            /*************************************************************************/
            public TimeZoneList() : base(137)
            {
            }

            /****************************************************************************/
            public XmlDocument Xml
            {
                get
                {
                    cXMLWriter objWriter = new cXMLWriter();

                    using(objWriter.Acquire)
                    {
                        using(new XmlElementWriter(objWriter, "Zoomla"))
                        {
                            using(new XmlElementWriter(objWriter, "TimeZones"))
                            {
                                foreach(TimeZoneInfo tz in Mondo.Globalization.Globalization.TimeZones)
                                {
                                    using(new XmlElementWriter(objWriter, "TimeZone"))
                                    {
                                        double dOffset = tz.BaseUtcOffset.TotalMinutes;

                                        objWriter.WriteAttributeString("id", tz.Id);
                                        objWriter.WriteAttributeString("utc_offset", (int)Math.Floor(dOffset));

                                        objWriter.WriteElementCDATA("Name",   tz.DisplayName);
                                        objWriter.WriteElementString("Offset", tz.BaseUtcOffset);
                                    }
                                }
                            }
                        }
                    }

                    return(objWriter.Xml);
                }
            }
        }

        /*************************************************************************/
        public static TimeZoneList TimeZones 
        {
            get {return(s_aTimeZones);}
        }

        /*************************************************************************/
        public static bool IsValidTimeZone(string idTimeZone)
        {
            return(s_aTimeZones.ContainsKey(idTimeZone));
        }

        /****************************************************************************/
        public static string ToLocalTime(string strValue, string strTimeZone)
        {
            DateTime dtValue = DateTime.MinValue;

            if(!DateTime.TryParse(strValue, out dtValue))
                return(strValue);

            dtValue = ToLocalTime(dtValue, strTimeZone);

            return(dtValue.ToString("s"));
        }

        /****************************************************************************/
        public static DateTime ToLocalTime(DateTime dtValue, string strTimeZone)
        {
            dtValue = DateTime.SpecifyKind(dtValue, DateTimeKind.Unspecified);

            return(TimeZoneInfo.ConvertTimeFromUtc(dtValue, GetTimeZone(strTimeZone)));
        }

        /****************************************************************************/
        public static DateTime ToUniversalTime(DateTime dtValue, string strTimeZone)
        {
            dtValue = DateTime.SpecifyKind(dtValue, DateTimeKind.Unspecified);

            return(TimeZoneInfo.ConvertTimeToUtc(dtValue, GetTimeZone(strTimeZone)));
        }

        /****************************************************************************/
        public static string ToUniversalTime(string strValue, string strTimeZone)
        {
            DateTime dtValue = DateTime.MinValue;

            if(!DateTime.TryParse(strValue, out dtValue))
                return(strValue);

            dtValue = ToUniversalTime(dtValue, strTimeZone);

            return(dtValue.ToString("s"));
        }

        /****************************************************************************/
        private static TimeZoneInfo GetTimeZone(string strTimeZone)
        {
            if(strTimeZone == "" || !Globalization.TimeZones.ContainsKey(strTimeZone))
                strTimeZone = "Pacific Standard Time";

            return(Globalization.TimeZones[strTimeZone]);
        }

        /*************************************************************************/
        /*************************************************************************/
        private class TimeZoneComparer : IComparer<TimeZoneInfo>
        {
            internal TimeZoneComparer()
            {
            }

            /*************************************************************************/
            public int Compare(TimeZoneInfo x, TimeZoneInfo y)
            {
                return(x.DisplayName.CompareTo(y.DisplayName));
            }
        }
    }

    /*********************************************************************/
    /*********************************************************************/
    public class GlobalizationObject : IGlobalization
    {
        private bool?                   m_bMetric           = null;
        private TimeZoneInfo            m_objTimeZone       = null;
        private CultureInfo             m_objCulture        = null;
        private DateTimeFormatInfo      m_objDateTimeFormat = null;
        private NumberFormatInfo        m_objNumberFormat   = null;
        private RegionInfo              m_objRegion         = null;

        /*********************************************************************/
        public virtual bool IsMetric        
        {
            get {return(m_bMetric == null ? this.Region.IsMetric : m_bMetric.Value);}
            set {m_bMetric = value;}
        }

        /*********************************************************************/
        public virtual TimeZoneInfo TimeZone
        {
            get
            {
                if(m_objTimeZone != null)
                    return(m_objTimeZone);

                return(TimeZoneInfo.Local);
            }

            set
            {
                m_objTimeZone = value;
            }
        }

        /*********************************************************************/
        public virtual CultureInfo Culture
        {
            get
            {
                if(m_objCulture != null)
                    return(m_objCulture);

                return(CultureInfo.CurrentUICulture);
            }

            set
            {
                m_objCulture = value;
            }
        }

        /*********************************************************************/
        public virtual DateTimeFormatInfo DateTimeFormat
        {
            get
            {
                if(m_objDateTimeFormat != null)
                    return(m_objDateTimeFormat);

                return(this.Culture.DateTimeFormat);
            }

            set
            {
                m_objDateTimeFormat = value;
            }
        }

        /*********************************************************************/
        public virtual NumberFormatInfo NumberFormat
        {
            get
            {
                if(m_objNumberFormat != null)
                    return(m_objNumberFormat);

                return(this.Culture.NumberFormat);
            }

            set
            {
                m_objNumberFormat = value;
            }
        }

        /*********************************************************************/
        public virtual RegionInfo Region
        {
            get
            {
                if(m_objRegion != null)
                    return(m_objRegion);

                return(RegionInfo.CurrentRegion);
            }

            set
            {
                m_objRegion = value;
            }
        }
    }

}
