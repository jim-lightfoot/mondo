using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MondoTest
{
    [TestClass]
    public class Globalization
    {
        [TestMethod]
        public void Mondo_Globalization_IsValidTimeZone()
        {
            Assert.IsTrue(Mondo.Globalization.Globalization.IsValidTimeZone("Pacific Standard Time"));
            Assert.IsTrue(Mondo.Globalization.Globalization.IsValidTimeZone("W. Europe Standard Time"));
            Assert.IsTrue(Mondo.Globalization.Globalization.IsValidTimeZone("India Standard Time"));
            Assert.IsTrue(Mondo.Globalization.Globalization.IsValidTimeZone("Kaliningrad Standard Time"));
            Assert.IsTrue(Mondo.Globalization.Globalization.IsValidTimeZone("Tasmania Standard Time"));
            Assert.IsTrue(Mondo.Globalization.Globalization.IsValidTimeZone("Ulaanbaatar Standard Time"));
            Assert.IsTrue(Mondo.Globalization.Globalization.IsValidTimeZone("Turkey Standard Time"));
            Assert.IsFalse(Mondo.Globalization.Globalization.IsValidTimeZone("Pavvcific Standard Time"));
        }

        [TestMethod]
        public void Mondo_Globalization_ToLocalTime()
        {
            DateTime dtCurrent = new DateTime(2015, 1, 1);

            Assert.AreEqual("bob", Mondo.Globalization.Globalization.ToLocalTime("bob", "Pacific Standard Time"));
            Assert.AreEqual("fred", Mondo.Globalization.Globalization.ToLocalTime("fred", "Kaliningrad Standard Time"));

            Assert.AreEqual(dtCurrent.AddHours(-8), Mondo.Globalization.Globalization.ToLocalTime(dtCurrent, "Pacific Standard Time"));
            Assert.AreEqual(dtCurrent.AddHours(3), Mondo.Globalization.Globalization.ToLocalTime(dtCurrent, "Russian Standard Time"));
            Assert.AreEqual(dtCurrent.AddHours(5.5), Mondo.Globalization.Globalization.ToLocalTime(dtCurrent, "India Standard Time"));
            Assert.AreEqual(dtCurrent.AddHours(2), Mondo.Globalization.Globalization.ToLocalTime(dtCurrent, "Turkey Standard Time"));
        }

        [TestMethod]
        public void Mondo_Globalization_ToUniversalTime()
        {
            DateTime dtCurrent = new DateTime(2015, 1, 1);

            Assert.AreEqual(dtCurrent.AddHours(8), Mondo.Globalization.Globalization.ToUniversalTime(dtCurrent, "Pacific Standard Time"));
            Assert.AreEqual(dtCurrent.AddHours(-3), Mondo.Globalization.Globalization.ToUniversalTime(dtCurrent, "Russian Standard Time"));
            Assert.AreEqual(dtCurrent.AddHours(-5.5), Mondo.Globalization.Globalization.ToUniversalTime(dtCurrent, "India Standard Time"));
            Assert.AreEqual(dtCurrent.AddHours(-2), Mondo.Globalization.Globalization.ToUniversalTime(dtCurrent, "Turkey Standard Time"));
        }
    }
}
