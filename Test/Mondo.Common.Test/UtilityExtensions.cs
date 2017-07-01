using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mondo.Common;

namespace Mondo.Common.Test
{
    /// <summary>
    /// Summary description for UtilityExtensions
    /// </summary>
    [TestClass]
    public class UtilityExtensions
    {
        [TestMethod]
        public void Mondo_Common_UtilityExtensions_Daydex()
        {
            Assert.AreEqual(40177, (new DateTime(2010, 1,   1)).Daydex());
            Assert.AreEqual(41944, (new DateTime(2014, 11,  3)).Daydex());
            Assert.AreEqual(42174, (new DateTime(2015,  6, 21)).Daydex());
            Assert.AreEqual(42427, (new DateTime(2016,  2, 29)).Daydex());
        }

        [TestMethod]
        public void Mondo_Common_UtilityExtensions_FromDaydex()
        {
            Assert.AreEqual(new DateTime(2010, 1,   1), 40177.FromDaydex());
            Assert.AreEqual(new DateTime(2014, 11,  3), 41944.FromDaydex());
            Assert.AreEqual(new DateTime(2015,  6, 21), 42174.FromDaydex());
            Assert.AreEqual(new DateTime(2016,  2, 29), 42427.FromDaydex());
        }
    }
}
