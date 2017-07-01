using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using Mondo.Xml;

namespace Mondo.Test
{
    [TestClass]
    public class XmlExtensions
    {
        private XmlDocument TestData
        {
            get
            {
                return(XmlDoc.LoadXml("<Test><Row1 name='Bob' age='42' id='679856895' born='1961-09-11' male='true' guid='AF5B98A1-196D-4773-ADE1-1817A8D8BBDF'> " +
                                      "Smith" + 
                                      "<Column1 name='Maggie' age='30' id='679856899' born='1981-11-09' male='0' rank='3.2' guid='AF5B98A1-196D-4773-ADE1-1817A8D8BBDD'>Jones</Column1>" + 
                                      "</Row1></Test>"));
            }
        }

        [TestMethod]
        public void Mondo_Common_XmlExtensions_GetAttribute()
        {
            XmlDocument xmlTest = TestData;

            Assert.AreEqual("Bob",     xmlTest.SelectSingleNode("//Row1").GetAttribute("name"));
            Assert.AreEqual(42,        xmlTest.SelectSingleNode("//Row1").GetAttribute<int>("age"));
            Assert.AreEqual(42,        xmlTest.SelectSingleNode("//Row1").GetAttribute<short>("age"));
            Assert.AreEqual(42,        xmlTest.SelectSingleNode("//Row1").GetAttribute<long>("age"));
            Assert.AreEqual(true,        xmlTest.SelectSingleNode("//Row1").GetAttribute<bool>("male"));
            Assert.AreEqual(999,        xmlTest.SelectSingleNode("//Row1").GetAttribute<int>("job_no", 999));
            Assert.AreEqual(679856895, xmlTest.SelectSingleNode("//Row1").GetAttribute<long>("id"));
            Assert.AreEqual(new DateTime(1961, 09, 11), xmlTest.SelectSingleNode("//Row1").GetAttribute<DateTime>("born"));
            Assert.AreEqual(Guid.Parse("AF5B98A1-196D-4773-ADE1-1817A8D8BBDF"), xmlTest.SelectSingleNode("//Row1").GetAttribute<Guid>("guid"));
        }

        public void Mondo_Common_XmlExtensions_GetChildText()
        {
            XmlDocument xmlTest = TestData;

            Assert.AreEqual("Jones", xmlTest.SelectSingleNode("//Row1").GetChildText("Column1"));
        }

        [TestMethod]
        public void Mondo_Common_XmlExtensions_GetChildAttribute()
        {
            XmlDocument xmlTest = TestData;

            Assert.AreEqual("Maggie",     xmlTest.SelectSingleNode("//Row1").GetChildAttribute("Column1", "name"));
            Assert.AreEqual(30,        xmlTest.SelectSingleNode("//Row1").GetChildAttribute<int>("Column1", "age"));
            Assert.AreEqual(3.2M,        xmlTest.SelectSingleNode("//Row1").GetChildAttribute<decimal>("Column1", "rank"));
            Assert.AreEqual(3.2d,        xmlTest.SelectSingleNode("//Row1").GetChildAttribute<double>("Column1", "rank"));
            Assert.AreEqual(679856899, xmlTest.SelectSingleNode("//Row1").GetChildAttribute<long>("Column1", "id"));
            Assert.AreEqual(new DateTime(1981, 11, 09), xmlTest.SelectSingleNode("//Row1").GetChildAttribute<DateTime>("Column1", "born"));
            Assert.AreEqual(Guid.Parse("AF5B98A1-196D-4773-ADE1-1817A8D8BBDD"), xmlTest.SelectSingleNode("//Row1").GetChildAttribute<Guid>("Column1", "guid"));
        }

        [TestMethod]
        public void Mondo_Common_XmlExtensions_ToJSON()
        {
           XmlDocument xmlTest1 = XmlDoc.LoadXml("<Test> " +
                                                 "  <Manufacturer>Chevy</Manufacturer>" + 
                                                 "  <Model>Camaro</Model>" + 
                                                 "</Test>");

            Assert.AreEqual("{\"Test\":{\"Manufacturer\":\"Chevy\",\"Model\":\"Camaro\"}}", xmlTest1.ToJSON().Replace(" ", "").Replace("\r", "").Replace("\n", ""));

           XmlDocument xmlTest2 = XmlDoc.LoadXml("<Test> " +
                                                 "  <List name=\"Vehicles\">" + 
                                                 "    <Vehicle>" + 
                                                 "      <Manufacturer>Chevy</Manufacturer>" + 
                                                 "      <Model>Camaro</Model>" + 
                                                 "    </Vehicle>" + 
                                                 "    <Vehicle>" + 
                                                 "      <Manufacturer>Pontiac</Manufacturer>" + 
                                                 "      <Model>Firebird</Model>" + 
                                                 "    </Vehicle>" + 
                                                 "  </List>" + 
                                                 "</Test>");

            Assert.AreEqual("{\"Test\":{\"Vehicles\":[{\"Manufacturer\":\"Chevy\",\"Model\":\"Camaro\"},{\"Manufacturer\":\"Pontiac\",\"Model\":\"Firebird\"}]}}", xmlTest2.ToJSON().Replace(" ", "").Replace("\r", "").Replace("\n", ""));

           XmlDocument xmlTest3 = XmlDoc.LoadXml("<Test> " +
                                                 "  <List name=\"Models\">" + 
                                                 "    <Model>Camaro</Model>" + 
                                                 "    <Model>Malibu</Model>" + 
                                                 "    <Model>Corvette</Model>" + 
                                                 "  </List>" + 
                                                 "</Test>");

            Assert.AreEqual("{\"Test\":{\"Models\":[\"Camaro\",\"Malibu\",\"Corvette\"]}}", xmlTest3.ToJSON().Replace(" ", "").Replace("\r", "").Replace("\n", ""));
        }
    }
}
