using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using Mondo.Common;

namespace Mondo.Test
{
    [TestClass]
    public class ConfigTest
    {
       [TestMethod]
        public void Config_GetAttribute()
        {
           IConfig config = new AppConfig();

            Assert.AreEqual(Guid.Parse("AF5B98A1-196D-4773-ADE1-1817A8D8BBDF"), config.Get<Guid>("guid"));
            Assert.AreEqual("Bob",                      config.Get("name"));
            Assert.AreEqual(42,                         config.Get<int>("age"));
            Assert.AreEqual(42,                         config.Get<short>("age"));
            Assert.AreEqual(42,                         config.Get<long>("age"));
            Assert.AreEqual(true,                       config.Get<bool>("male"));
            Assert.AreEqual(true,                       config.Get<bool>("male2"));
            Assert.AreEqual(true,                       config.Get<bool>("male3"));
            Assert.AreEqual(true,                       config.Get<bool>("male4"));
            Assert.AreEqual(true,                       config.Get<bool>("male"));
            Assert.AreEqual(999,                        config.Get<int>("job_no", 999));
            Assert.AreEqual(679856895,                  config.Get<long>("id"));
            Assert.AreEqual(new DateTime(1961, 09, 11), config.Get<DateTime>("born"));
            Assert.AreEqual(3.2M,                       config.Get<decimal>("rank"));
            Assert.AreEqual(3.2d,                       config.Get<double>("rank"));
        }
    }
}
