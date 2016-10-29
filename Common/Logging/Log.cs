/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: Log.cs								                    */
/*        Class(es): Log, ILog, ApplicationLog                              */
/*          Purpose: Log errors and other info                              */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 11 Apr 2014                                            */
/*                                                                          */
/*   Copyright (c) 2014-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mondo.Common
{
    /*************************************************************************/
    /*************************************************************************/
    // Generic interface for logging
    public interface ILog
    {
        void WriteError(Exception ex, Dictionary<string, string> properties = null);
        void WriteEvent(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null);
        void WriteMetric(string metricName, double value, Dictionary<string, string> properties = null);
        void WriteTrace(string message, Log.SeverityLevel level, Dictionary<string, string> properties = null);
        void WriteRequest(string name, DateTime startTime, TimeSpan duration, string responseCode, bool success);
    }

    /*************************************************************************/
    /*************************************************************************/
    public class ApplicationLog : ILog
    {
        private readonly List<LogEntry> _aLogs    = new List<LogEntry>();
        private ExceptionCache          _objCache = new ExceptionCache();

        /*************************************************************************/
        public void Register(ILog log, bool fallbackOnly = false, bool fallbackAsError = false)
        {
            _aLogs.Add(new LogEntry {Log = log, FallbackOnly = fallbackOnly, FallbackAsError = fallbackAsError});
        }

        /*************************************************************************/
        public void ClearRegistered()
        {
            _aLogs.Clear();
        }

        /*************************************************************************/
        public void WriteError(Exception ex, Dictionary<string, string> properties = null)
        {
            // Don't need to log these
            if(ex is ThreadAbortException)
                return;

            // Wierd asp.net thing
            if(ex.Message.IndexOf("get_aspx_ver.aspx") != -1) 
                return;

            // If it's added to the cache we can send messages otherwise it's already 
            //    in there and we don't want to send it again
            if(_objCache.Add(ref ex))
            {
                WriteTelemetry(log => 
                { 
                    log.WriteError(ex, properties); 
                });
            }
        }

        /*************************************************************************/
        public void WriteEvent(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
        {
            WriteTelemetry(log =>
            { 
                log.WriteEvent(eventName, properties, metrics); 
            });
        }

        /*************************************************************************/
        public void WriteMetric(string metricName, double value, Dictionary<string, string> properties = null)
        {
            WriteTelemetry(log => 
            { 
                log.WriteMetric(metricName, value, properties); 
            });
        }

        /*************************************************************************/
        public void WriteTrace(string message, Log.SeverityLevel level, Dictionary<string, string> properties = null)
        {
            WriteTelemetry(log => 
            { 
                log.WriteTrace(message, level, properties); 
            });
        }

        /*************************************************************************/
        public void WriteRequest(string name, DateTime startTime, TimeSpan duration, string responseCode, bool success)
        {
            WriteTelemetry(log => 
            { 
                log.WriteRequest(name, startTime, duration, responseCode, success); 
            });
        }

        #region Private Methods

        /*************************************************************************/
        private void WriteTelemetry(WriteTask fnWrite)
        {
            // Write telemetry in background thread
          #if DEBUG
          #else
            Task.Run(() =>
          #endif
            {
                var nLoggers = _aLogs.Count;

                for (var i = 0; i < nLoggers; ++i)
                {
                    var logger = _aLogs[i];

                    // Only write primary telemetry to non-fallback loggers
                    if (!logger.FallbackOnly)
                    {
                        try
                        {
                            fnWrite(logger.Log);
                        }
                        catch (Exception ex)
                        {
                            // Ok that didn't work, write to a fallback log
                            FallbackTelemetry(fnWrite, i + 1, ex);
                        }
                    }
                }
            }
          #if DEBUG
          #else
            );
          #endif
        }

        /*************************************************************************/
        private void FallbackTelemetry(WriteTask fnWrite, int start, Exception excep)
        {
            var nLoggers = _aLogs.Count;

            // Go through all the loggers after the last one
            for (var j = start; j < nLoggers; ++j)
            {
                var fallBackLogger = _aLogs[j];

                // Find the first fallback logger
                if (fallBackLogger.FallbackOnly)
                {
                    try
                    {
                        // Log the exception from the previously failed log
                        if (fallBackLogger.FallbackAsError)
                        {
                            try
                            {
                                fallBackLogger.Log.WriteError(excep);
                            }
                            catch
                            {
                                // This is bad
                            }
                        }
                        else // or write the original telemetry
                            fnWrite(fallBackLogger.Log);
                    }
                    catch (Exception ex)
                    {
                        // The fallback logger failed, fallback to the next
                        FallbackTelemetry(fnWrite, start + 1, ex);
                    }

                    break;
                }
            }
        }

        /*************************************************************************/
        private struct LogEntry
        {
            internal bool FallbackAsError;
            internal bool FallbackOnly;
            internal ILog Log;
        }

        private delegate void WriteTask(ILog log);

        #endregion
    }

    /*************************************************************************/
    /*************************************************************************/
    public static class Log
    {
        private static readonly ApplicationLog _log = new ApplicationLog();

        /*************************************************************************/
        public static void Register(ILog log, bool fallbackOnly = false, bool fallbackAsError = false)
        {
            _log.Register(log, fallbackOnly, fallbackAsError);
        }

        /*************************************************************************/
        /*************************************************************************/
        public enum SeverityLevel
        {
            Verbose     = 0,
            Information = 1,
            Warning     = 2,
            Error       = 3,
            Critical    = 4
        }

        /*************************************************************************/
        public static void ClearRegistered()
        {
            _log.ClearRegistered();
        }

        /*************************************************************************/
        public static void WriteError(Exception ex, Dictionary<string, string> properties = null)
        {
            _log.WriteError(ex, properties);
        }

        /*************************************************************************/
        public static void WriteEvent(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
        {
            _log.WriteEvent(eventName, properties, metrics);
        }

        /*************************************************************************/
        public static void WriteMetric(string metricName, double value, Dictionary<string, string> properties = null)
        {
            _log.WriteMetric(metricName, value, properties);
        }

        /*************************************************************************/
        public static void WriteTrace(string message, Log.SeverityLevel level, Dictionary<string, string> properties = null)
        {
            _log.WriteTrace(message, level, properties);
        }

        /*************************************************************************/
        public static void WriteRequest(string name, DateTime startTime, TimeSpan duration, string responseCode, bool success)
        {
            _log.WriteRequest(name, startTime, duration, responseCode, success);
        }
    }
}