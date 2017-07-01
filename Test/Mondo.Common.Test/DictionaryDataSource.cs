using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mondo.Common;

namespace Mondo.Common.Test
{
    [TestClass]
    public class Mondo_Common_Test
    {
        [TestMethod]
        public void Mondo_Common_DictionaryDataSource()
        {
            var dict = new Dictionary<string, object>();

            dict.Add("Name",  "fred");
            dict.Add("Age",   42);
            dict.Add("City",  "Bedrock");

            var src = DataObjectSource.Create(dict);
            dynamic dynValue = src;

            string strName = dynValue.Name;
            int   iAge     = dynValue.Age;
            string strCity = dynValue.City;

            Assert.AreEqual("fred",    strName);
            Assert.AreEqual(42,        iAge);
            Assert.AreEqual("Bedrock", strCity);

        }
    }
}
