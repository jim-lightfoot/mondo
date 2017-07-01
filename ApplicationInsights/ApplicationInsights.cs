/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  						    */
/*                                                                          */
/*           Module: Mondo.ApplicationInsights				    */
/*             File: ApplicationInsights.cs				    */
/*        Class(es): ApplicationInsights				    */
/*          Purpose: A thin wrapper around Microsoft's Application Insights */
/*                    API that implements the ILog interface from           */
/*                      Mondo.Common                                        */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 22 Mar 2015                                            */
/*                                                                          */
/*   Copyright (c) 2015-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;

using Mondo.Common;

namespace Mondo.ApplicationInsights
{
    /***********************************************************************************
     * Put this in your app's startup code, i.e. Global.asax:                 
     *                                                                       
     *   IConfig config      = new Mondo.Common.AppConfig();                         
     *   ILog    appInsights = new Mondo.ApplicationInsights.ApplicationInsights(config);   
     *                                                                        
     *   Mondo.Common.Log.Register(appInsights);                       
     *                                                                        
    /*************************************************************************/
    public class ApplicationInsights : ILog
    {
        private readonly TelemetryClient _client;

        /*************************************************************************/
        public ApplicationInsights(IConfig config) : this(config.Get("ApplicationInsights.InstrumentationKey"))
        {
        }

        /*************************************************************************/
        public ApplicationInsights(string instrumentationKey)
        {
            // Create an Application Insights client
            _client = new TelemetryClient();

            if(instrumentationKey != "")
            { 
                _client.Context.InstrumentationKey = instrumentationKey;

                // Set intrumentation key for unhandled exceptions, request logging, etc
                Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = instrumentationKey;
            }
        }

        /*************************************************************************/
        public void WriteError(Exception ex, Dictionary<string, string> properties = null)
        {
            _client.TrackException(ex, properties);
        }

        /*************************************************************************/
        public void WriteEvent(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
        {
            _client.TrackEvent(eventName, properties, metrics);
        }

        /*************************************************************************/
        public void WriteMetric(string metricName, double value, Dictionary<string, string> properties = null)
        {
            _client.TrackMetric(metricName, value, properties);
        }

        /*************************************************************************/
        public void WriteTrace(string message, Log.SeverityLevel level, Dictionary<string, string> properties = null)
        {
            _client.TrackTrace(message, (Microsoft.ApplicationInsights.DataContracts.SeverityLevel)level, properties);
        }

        /*************************************************************************/
        public void WriteRequest(string name, DateTime startTime, TimeSpan duration, string responseCode, bool success)
        {
            _client.TrackRequest(name, startTime, duration, responseCode, success);
        }
    }}
