using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mondo.Common;
using Mondo.Xml;

namespace Mondo.Test
{
    /****************************************************************************/
    /****************************************************************************/
    [TestClass]
    public class CollectionExtensions
    {
        /****************************************************************************/
        [TestMethod]
        public void Mondo_Common_CollectionExtensions_ToXml_1()
        {
          NameValueCollection collection = new NameValueCollection();

          collection.Add("Fred", "Bob");

          XmlDocument xmlResult = collection.ToXml();
          XmlNode xmlNode = xmlResult.SelectSingleNode("//Data/Fred");

          Assert.AreEqual("Bob", xmlNode.InnerText);
        }

        /****************************************************************************/
        [TestMethod]
        public void Mondo_Common_CollectionExtensions_ToXml_2()
        {
          NameValueCollection collection = new NameValueCollection();

          collection.Add("Vehicle[Name]", "Chevy");

          XmlDocument xmlResult = collection.ToXml();
          XmlNode xmlNode = xmlResult.SelectSingleNode("//Data/Vehicle/Name");

          Assert.AreEqual("Chevy", xmlNode.InnerText);
        }

        /****************************************************************************/
        [TestMethod]
        public void Mondo_Common_CollectionExtensions_ToXml_3()
        {
          NameValueCollection collection = new NameValueCollection();

          collection.Add("Vehicles[0]", "Chevy");

          XmlDocument xmlResult = collection.ToXml();
          XmlNode xmlNode = xmlResult.SelectSingleNode("//Data/Vehicles/Vehicle[1]");

          Assert.AreEqual("Chevy", xmlNode.InnerText);
        }

        /****************************************************************************/
        [TestMethod]
        public void Mondo_Common_CollectionExtensions_ToXml_4()
        {
          NameValueCollection collection = new NameValueCollection();

          collection.Add("Vehicles[0][Exterior][Color]", "Black");

          XmlDocument xmlResult = collection.ToXml();
          XmlNode xmlNode = xmlResult.SelectSingleNode("//Data/Vehicles/Vehicle[1]/Exterior/Color");

          Assert.AreEqual("Black", xmlNode.InnerText);
        }

        /****************************************************************************/
        [TestMethod]
        public void Mondo_Common_CollectionExtensions_ToXml_5()
        {
          NameValueCollection collection = new NameValueCollection();

          collection.Add("Vehicles[0][Make]", "Chevy");
          collection.Add("Vehicles[0][Exterior][Lights][0][Type]", "FrontHeadLight");
          collection.Add("Vehicles[0][Exterior][Lights][1][Type]", "TailLight");
          collection.Add("Vehicles[0][Exterior][Lights][2][Type]", "RearTurnSignal");
          collection.Add("Vehicles[1][Make]", "Pontiac");
          collection.Add("Vehicles[1][Exterior][Lights][0][Type]", "FrontTurnSignal");
          collection.Add("Vehicles[1][Exterior][Lights][1][Type]", "RunningBoard");
          collection.Add("Vehicles[1][Exterior][Lights][2][Type]", "Roof");

          XmlNode xmlResult = collection.ToXml();

          Assert.AreEqual("Chevy",              xmlResult.GetChildText("//Data/Vehicles/Vehicle[1]/Make"));
          Assert.AreEqual("FrontHeadLight",     xmlResult.GetChildText("//Data/Vehicles/Vehicle[1]/Exterior/Lights/Light[1]/Type"));
          Assert.AreEqual("TailLight",          xmlResult.GetChildText("//Data/Vehicles/Vehicle[1]/Exterior/Lights/Light[2]/Type"));
          Assert.AreEqual("RearTurnSignal",     xmlResult.GetChildText("//Data/Vehicles/Vehicle[1]/Exterior/Lights/Light[3]/Type"));

          Assert.AreEqual("Pontiac",            xmlResult.GetChildText("//Data/Vehicles/Vehicle[2]/Make"));
          Assert.AreEqual("FrontTurnSignal",    xmlResult.GetChildText("//Data/Vehicles/Vehicle[2]/Exterior/Lights/Light[1]/Type"));
          Assert.AreEqual("RunningBoard",       xmlResult.GetChildText("//Data/Vehicles/Vehicle[2]/Exterior/Lights/Light[2]/Type"));
          Assert.AreEqual("Roof",               xmlResult.GetChildText("//Data/Vehicles/Vehicle[2]/Exterior/Lights/Light[3]/Type"));
        }

        /****************************************************************************/
        [TestMethod]
        public void Mondo_Common_CollectionExtensions_ToXml_6()
        {
          NameValueCollection collection = new NameValueCollection();

          collection.Add("Vehicles[0][Exterior][Color][]", "Black");

          XmlDocument xmlResult = collection.ToXml();
          XmlNode xmlNode = xmlResult.SelectSingleNode("//Data/Vehicles/Vehicle[1]/Exterior/Color");

          Assert.AreEqual("Black", xmlNode.InnerText);
        }

        /****************************************************************************/
        [TestMethod]
        public void Mondo_Common_CollectionExtensions_Pack_IDictionary()
        {
            Dictionary<int, int> values = new Dictionary<int, int> { {1000, 9000}, {1001, 9001}, {1002, 9002}, {1003, 9003} };

            Assert.AreEqual("1000,9000;1001,9001;1002,9002;1003,9003", values.Pack<int, int>());

            Dictionary<int, int> values2 = new Dictionary<int, int> { {1000, 9000}, {1001, 9001}, {1002, 9002}, {1003, 9003} };

            Assert.AreEqual("1000/9000|1001/9001|1002/9002|1003/9003", values2.Pack<int, int>("/", "|"));

            Dictionary<int, int> values3 = new Dictionary<int, int> { {1000, 9000}, {1001, 9001}, {1002, 9002}, {1003, 9003} };

            Assert.AreEqual("1000,9001;1001,9002;1002,9003;1003,9004", values3.Pack<int, int>(",", ";", v=> { return (v+1).ToString(); }));
        }

    }
}
