/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: EventLog.cs										    */
/*        Class(es): EventLog										        */
/*          Purpose: Writes to the event log                                */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 19 Sep 2001                                            */
/*                                                                          */
/*   Copyright (c) 2001-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Specialized;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public interface IEventLog
    {
        void WriteEntry(string strMessage, EventLogEntryType eType);
    }

    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Writes to the event log
    /// </summary>
    public sealed class EventLog : IEventLog, ILog
    {
        private System.Diagnostics.EventLog m_objEventLog;

        /****************************************************************************/
        public EventLog(System.Diagnostics.EventLog objEventLog = null)
        {
            m_objEventLog = objEventLog;
        }

        /****************************************************************************/
        public static string DefaultAppName
        {
            get {return(Config.Get("ExceptionEventLogSource", "Application")); }
        }

        /****************************************************************************/
        public static void Error(string strAppName, string strMessage)
        {
            WriteEntry(strAppName, strMessage, EventLogEntryType.Error);
        }  

        /****************************************************************************/
        public static void Error(string strMessage)
        {
             Error(DefaultAppName, strMessage);
        }  

        /****************************************************************************/
        public static void Warning(string strAppName, string strMessage)
        {
            WriteEntry(strAppName, strMessage, EventLogEntryType.Warning);
        }  

        /****************************************************************************/
        public static void Warning(string strMessage)
        {
             Warning(DefaultAppName, strMessage);
        }  

       /****************************************************************************/
        public static void Note(string strAppName, string strMessage)
        {
            WriteEntry(strAppName, strMessage, EventLogEntryType.Information);
        }  

        /****************************************************************************/
        public static void Note(string strMessage)
        {
            Note(DefaultAppName, strMessage);
        }  

        /****************************************************************************/
        public static void NoteTime(string strMessage)
        {
            Note(DefaultAppName, strMessage + " at " + DateTime.Now.ToString("HH:mm:ss:fff"));
        }  

        /****************************************************************************/
        public void WriteEntry(string strMessage, EventLogEntryType eType)
        {
            try
            {
                if(m_objEventLog != null)
                    m_objEventLog.WriteEntry(strMessage, eType);
            }
            catch
            {
                // Oops!
            }
        }

        /****************************************************************************/
        private static void WriteEntry(string strAppName, string strMessage, EventLogEntryType eType)
        {
            try
            {
                string strLogName = Config.Get("ExceptionEventLogName", "Application");

                try
                {
                    if(!System.Diagnostics.EventLog.SourceExists(strAppName))
                        System.Diagnostics.EventLog.CreateEventSource(strAppName, strLogName);
                }
                catch(Exception ex)
                {
                    cDebug.Capture(ex);
                    strLogName = "Application";
                    strAppName = "";
                }

                System.Diagnostics.EventLog objLog = new System.Diagnostics.EventLog(strLogName);

                if(strAppName != "")
                    objLog.Source = strAppName;
      
                objLog.WriteEntry(strMessage, eType);
            }
            catch(Exception ex)
            {
                cDebug.Capture(ex);
            }
        }

        #region ILog Methods

        /****************************************************************************/
        public void WriteError(Exception ex, System.Collections.Generic.Dictionary<string, string> properties = null)
        {
            EventLog.Error(ex.Message);
        }

        /****************************************************************************/
        public void WriteEvent(string eventName, System.Collections.Generic.Dictionary<string, string> properties = null, System.Collections.Generic.Dictionary<string, double> metrics = null)
        {
            EventLog.Note(eventName);
        }

        /****************************************************************************/
        public void WriteMetric(string metricName, double value, System.Collections.Generic.Dictionary<string, string> properties = null)
        {
        }

        /****************************************************************************/
        public void WriteTrace(string message, Log.SeverityLevel level, System.Collections.Generic.Dictionary<string, string> properties = null)
        {
        }

        /****************************************************************************/
        public void WriteRequest(string name, DateTime startTime, TimeSpan duration, string responseCode, bool success)
        {
        }

        #endregion
    }
}
