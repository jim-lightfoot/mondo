using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using Mondo.Common;

namespace Mondo.Test
{
    [TestClass]
    public class LogTest
    {
        [TestMethod]
        public void Mondo_Common_ApplicationLog_WriteError()
        {
            List<string> errors = new List<string>();
            ApplicationLog log = new ApplicationLog();

            log.Register(new TestLog(errors));

            log.WriteError(new Exception("Bob's hair is on fire"));

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Bob's hair is on fire", errors[0]);
        }

        [TestMethod]
        public void Mondo_Common_Log_WriteError()
        {
            List<string> errors = new List<string>();

            Log.Register(new TestLog(errors));

            Log.WriteError(new Exception("Bob's hair is on fire"));

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Bob's hair is on fire", errors[0]);
        }

        /*************************************************************************/
        /*************************************************************************/
        internal class TestLog : ILog
        {
            private readonly List<string> _errors;

            /*************************************************************************/
            internal TestLog(List<string> errors)
            {
                _errors = errors;
            }

            /*************************************************************************/
            public void WriteError(Exception ex, Dictionary<string, string> properties = null)
            {
                _errors.Add(ex.Message);
            }

            /*************************************************************************/
            public void WriteEvent(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
            {
            }

            /*************************************************************************/
            public void WriteMetric(string metricName, double value, Dictionary<string, string> properties = null)
            {
            }

            /*************************************************************************/
            public void WriteTrace(string message, Log.SeverityLevel level, Dictionary<string, string> properties = null)
            {
            }

            /*************************************************************************/
            public void WriteRequest(string name, DateTime startTime, TimeSpan duration, string responseCode, bool success)
            {
            }
        }
    }
}
