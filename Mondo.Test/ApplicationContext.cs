using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using Mondo.Common;

namespace Mondo.Test
{
    [TestClass]
    public class ApplicationContextTest
    {
        [TestMethod]
        public void ApplicationContext_WriteError()
        {
            List<string> errors1 = new List<string>();
            List<string> errors2 = new List<string>();
            TestAppContext context1 = new TestAppContext(errors1);
            TestAppContext context2 = new TestAppContext(errors2);

            context1.Log.WriteError(new Exception("Bob's hair is on fire"));
            context2.Log.WriteError(new Exception("John's hair is on fire"));

            Assert.AreEqual(1, errors1.Count);
            Assert.AreEqual("Bob's hair is on fire", errors1[0]);

            Assert.AreEqual(1, errors2.Count);
            Assert.AreEqual("John's hair is on fire", errors2[0]);
        }

        /*************************************************************************/
        /*************************************************************************/
        internal class TestAppContext : ApplicationContext
        {
            /*************************************************************************/
            internal TestAppContext(List<string> errors) : base(true, false)
            {
                var log = new ApplicationLog();

                log.Register(new TestLog(errors));

                this.Log = log;
            }

            public override IEncryptor Encryptor { get { return null;} }
            public override void       Validate(ILog log = null) { }
            public override string     MapPath(string path) { return ""; }
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
