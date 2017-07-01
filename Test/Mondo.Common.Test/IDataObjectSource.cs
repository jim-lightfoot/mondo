using System;
using System.Xml;
using System.Data;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mondo.Common;
using Mondo.Xml;

namespace Mondo.Test
{
    public class TestClass1
    {
        public string Make   {get; set;}
        public string Model  {get; set;}
        public int    Year   {get; set;}
        public string Color  {get; set;}
    }

    public class TestClass2 
    {
        public string  Make       {get; set;}
        public string  Model      {get; set;}
        public int?    Year       {get; set;}
        public string  Color      {get; set;}
        public int?    NumDoors   {get; set;}
    }

    public class TestClass3 
    {
        public string       Make            {get; set;}
        public string       Model           {get; set;}
        public int?         Year            {get; set;}
        public string       Color           {get; set;}
        public int?         NumDoors        {get; set;}
        public DateTime?    MaintenanceDue   {get; set;}
    }

    [TestClass]
    public class IDataObjectSourceTest
    {
        [TestMethod]
        public void Mondo_Common_IDataObjectSource_ToObject()
        {
            string xml = "<Data><Make>Aston Martin</Make><Model>DB9</Model><Year>1965</Year><Color>Blue</Color></Data>";
            XmlDocument xmlDoc = XmlDoc.LoadXml(xml);
            IDataObjectSource ds = DataObjectSource.Create(xmlDoc.RootNode());

            TestClass1 obj = ds.ToObject<TestClass1>();

            Assert.AreEqual("Aston Martin", obj.Make);
            Assert.AreEqual("DB9",          obj.Model);
            Assert.AreEqual(1965,           obj.Year);
            Assert.AreEqual("Blue",         obj.Color);
        }

        [TestMethod]
        public void Mondo_Common_IDataObjectSource_ToObject2()
        {
            string xml = "<Data><Make>Aston Martin</Make><Model>DB9</Model><Year>1965</Year><Color>Blue</Color></Data>";
            XmlDocument xmlDoc = XmlDoc.LoadXml(xml);
            IDataObjectSource ds = DataObjectSource.Create(xmlDoc.RootNode());

            TestClass2 obj = ds.ToObject<TestClass2>();

            Assert.AreEqual("Aston Martin", obj.Make);
            Assert.AreEqual("DB9",          obj.Model);
            Assert.AreEqual(1965,           obj.Year);
            Assert.AreEqual("Blue",         obj.Color);
            Assert.AreEqual(false,          obj.NumDoors.HasValue);
        }

        [TestMethod]
        public void Mondo_Common_IDataObjectSource_ToObject3()
        {
            string xml = "<Data><Make>Aston Martin</Make><MaintenanceDue>2016-08-04T14:00</MaintenanceDue><Model>DB9</Model><Year>1965</Year><Color>Blue</Color></Data>";
            XmlDocument xmlDoc = XmlDoc.LoadXml(xml);
            IDataObjectSource ds = DataObjectSource.Create(xmlDoc.RootNode());

            TestClass3 obj = ds.ToObject<TestClass3>();

            Assert.AreEqual("Aston Martin", obj.Make);
            Assert.AreEqual("DB9",          obj.Model);
            Assert.AreEqual(1965,           obj.Year);
            Assert.AreEqual("Blue",         obj.Color);
            Assert.AreEqual(false,          obj.NumDoors.HasValue);
            Assert.AreEqual(2016,           obj.MaintenanceDue.Value.Year);
            Assert.AreEqual(8,              obj.MaintenanceDue.Value.Month);
            Assert.AreEqual(4,              obj.MaintenanceDue.Value.Day);
            Assert.AreEqual(14,             obj.MaintenanceDue.Value.Hour);
            Assert.AreEqual(0,              obj.MaintenanceDue.Value.Minute);
        }

        [TestMethod]
        public void Mondo_Common_IDataObjectSource_ToList()
        {
            string xml = "<Data><Car><Make>Aston Martin</Make><Model>DB9</Model><Year>1965</Year><Color>Blue</Color></Car><Car><Make>Audi</Make><Model>A4</Model><Year>1973</Year><Color>Green</Color></Car></Data>";
            XmlDocument xmlDoc = XmlDoc.LoadXml(xml);
            DataSourceList ds = DataSourceList.Create(xmlDoc);

            IList<TestClass2> list = ds.ToList<TestClass2>();
            TestClass2 obj = list[0];

            Assert.AreEqual("Aston Martin", obj.Make);
            Assert.AreEqual("DB9",          obj.Model);
            Assert.AreEqual(1965,           obj.Year);
            Assert.AreEqual("Blue",         obj.Color);
            Assert.AreEqual(false,          obj.NumDoors.HasValue);

            obj = list[1];

            Assert.AreEqual("Audi", obj.Make);
            Assert.AreEqual("A4",          obj.Model);
            Assert.AreEqual(1973,           obj.Year);
            Assert.AreEqual("Green",         obj.Color);
            Assert.AreEqual(false,          obj.NumDoors.HasValue);
        }

        [TestMethod]
        public void Mondo_Common_IDataObjectSource_Dynamic()
        {
            string xml = "<Data><Make>Aston Martin</Make><Model>DB9</Model><Year>1965</Year><Color>Blue</Color></Data>";
            XmlDocument xmlDoc = XmlDoc.LoadXml(xml);
            dynamic data = DataObjectSource.Create(xmlDoc.RootNode());

            Assert.AreEqual("Aston Martin", data.Make);
            Assert.AreEqual("DB9",          data.Model);
            Assert.AreEqual(1965,           data.Year);
            Assert.AreEqual("Blue",         data.Color);
        }

        [TestMethod]
        public void Mondo_Common_IDataObjectSource_Dynamic2()
        {
            string xml = "<Data><Car><Make>Aston Martin</Make><Model>DB9</Model><Year>1965</Year><Color>Blue</Color></Car></Data>";
            XmlDocument xmlDoc = XmlDoc.LoadXml(xml);
            dynamic data = DataObjectSource.Create(xmlDoc.RootNode());

            Assert.AreEqual("Aston Martin", data.Car.Make);
            Assert.AreEqual("DB9",          data.Car.Model);
            Assert.AreEqual(1965,           data.Car.Year);
            Assert.AreEqual("Blue",         data.Car.Color);
        }

        [TestMethod]
        public void Mondo_Common_IDataObjectSource_DynamicDataTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("City", typeof(string));

            DataRow row = dt.LoadDataRow(new string[] {"bob", "Seattle"}, LoadOption.Upsert);

            dynamic data = DataObjectSource.Create(dt);

            Assert.AreEqual("bob", data.Name);
            Assert.AreEqual("Seattle", data.City);
        }
    }
}

