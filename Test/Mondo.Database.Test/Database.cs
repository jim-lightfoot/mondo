using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Xml;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mondo.Common;
using Mondo.Xml;
using Mondo.Database;

namespace Mondo.Database.Test
{
    [TestClass]
    public class Mondo_Database_Test
    {
        private const string _connectionString = "Data Source=OFFICEDESKTOP;Initial Catalog=MondoTest;Integrated Security=SSPI;";

        [TestMethod]
        public void Mondo_Database_ExecuteXml()
        {
            string xml = "";

            using(Database db = Database.Create(_connectionString,  false))
            {
                using(StoredProc sp = new StoredProc(db, "dbo.GetCustomerList"))
                {
                    using(db.Acquire)
                    { 
                        using(DbDataReader reader = db.ExecuteSelect(sp.Command))
                        {
                            xml = Database.ToXmlAsync(reader, "Database", new List<string> {"Customer"} ).Result;
                        }
                    }
                }
            }

            XmlDocument doc = XmlDoc.LoadXml(xml);

            Assert.AreEqual(4, doc.SelectNodes("//Customer").Count);
            Assert.AreEqual("John", doc.SelectNodes("//Customer")[0].GetChildText("Name"));
            Assert.AreEqual(27, doc.SelectNodes("//Customer")[0].GetChildText<int>("Age"));
            Assert.AreEqual("Barney", doc.SelectNodes("//Customer")[3].GetChildText("Name"));
            Assert.AreEqual(42, doc.SelectNodes("//Customer")[3].GetChildText<int>("Age"));
        }

        [TestMethod]
        public void Mondo_Database_ExecuteXml_2()
        {
            string xml = "";

            using(Database db = Database.Create(_connectionString,  false))
            {
                using(StoredProc sp = new StoredProc(db, "dbo.GetCustomers"))
                {
                    using(db.Acquire)
                    { 
                        using(DbDataReader reader = db.ExecuteSelect(sp.Command))
                        {
                            xml = Database.ToXmlAsync(reader, "Database", new List<string> {"Customer", "Promotion", "PromotionChannel"} ).Result;
                        }
                    }
                }
            }

            XmlDocument doc = XmlDoc.LoadXml(xml);

            Assert.AreEqual(4, doc.SelectNodes("//Customer").Count);
            Assert.AreEqual("John", doc.SelectNodes("//Customer")[0].GetChildText("Name"));
            Assert.AreEqual(27, doc.SelectNodes("//Customer")[0].GetChildText<int>("Age"));
            Assert.AreEqual("Barney", doc.SelectNodes("//Customer")[3].GetChildText("Name"));
            Assert.AreEqual(42, doc.SelectNodes("//Customer")[3].GetChildText<int>("Age"));

            Assert.AreEqual(3, doc.SelectNodes("//Promotion").Count);
            Assert.AreEqual("Buy  2S7s and Get a Free TV", doc.SelectNodes("//Promotion")[0].GetChildText("Name"));

            Assert.AreEqual(3, doc.SelectNodes("//PromotionChannel").Count);
            Assert.AreEqual(3, doc.SelectNodes("//PromotionChannel")[2].GetChildText<int>("PromotionID"));
        }

        [TestMethod]
        public void Mondo_Database_ExecuteXml_3()
        {
            string xml = "";

            using(Database db = Database.Create(_connectionString,  false))
            {
                using(StoredProc sp = new StoredProc(db, "dbo.GetCustomers"))
                {
                    xml = db.ExecuteXmlAsync(sp, "Database", new List<string> {"Customer", "Promotion", "PromotionChannel"} ).Result;
                }
            }

            XmlDocument doc = XmlDoc.LoadXml(xml);

            Assert.AreEqual(4, doc.SelectNodes("//Customer").Count);
            Assert.AreEqual("John", doc.SelectNodes("//Customer")[0].GetChildText("Name"));
            Assert.AreEqual(27, doc.SelectNodes("//Customer")[0].GetChildText<int>("Age"));
            Assert.AreEqual("Barney", doc.SelectNodes("//Customer")[3].GetChildText("Name"));
            Assert.AreEqual(42, doc.SelectNodes("//Customer")[3].GetChildText<int>("Age"));

            Assert.AreEqual(3, doc.SelectNodes("//Promotion").Count);
            Assert.AreEqual("Buy  2S7s and Get a Free TV", doc.SelectNodes("//Promotion")[0].GetChildText("Name"));

            Assert.AreEqual(3, doc.SelectNodes("//PromotionChannel").Count);
            Assert.AreEqual(3, doc.SelectNodes("//PromotionChannel")[2].GetChildText<int>("PromotionID"));
        }

        [TestMethod]
        public void Mondo_Database_ExecuteXml_4()
        {
            string xml = "";
            DateTime dtStart = DateTime.Now;
            XmlDocument syncDoc = null;

            using(Database db = Database.Create(_connectionString,  false))
            {
                using(StoredProc sp = new StoredProc(db, "dbo.GetCustomers"))
                {
                    using(db.Acquire)
                    { 
                        syncDoc = db.ExecuteXml(sp, "Zoomla", new List<string> {"Customer", "Promotion", "PromotionChannel"} );
                    }
                }
            }

            DateTime dtMiddle = DateTime.Now;


            using(Database db = Database.Create(_connectionString,  false))
            {
                using(StoredProc sp = new StoredProc(db, "dbo.GetCustomers"))
                {
                    xml = db.ExecuteXmlAsync(sp, "Database", new List<string> {"Customer", "Promotion", "PromotionChannel"} ).Result;
                }
            }

            XmlDocument asyncDoc = XmlDoc.LoadXml(xml);

            DateTime dtEnd = DateTime.Now;

            TimeSpan syncDuration  = dtMiddle - dtStart;
            TimeSpan asyncDuration = dtEnd - dtMiddle;

            Assert.IsTrue(syncDuration.Ticks > asyncDuration.Ticks);
        }

        [TestMethod]
        public async Task Mondo_Database_ExecuteSingleRecordDictionary()
        {
            IDictionary<string, object> dict = null;

            using(Database db = Database.Create(_connectionString,  false))
            {
                using(StoredProc sp = new StoredProc(db, "dbo.GetCustomerList"))
                {
                    dict = await db.ExecuteSingleRecordDictionaryAsync(sp);
                }
            }

            Assert.AreEqual("John", dict["Name"]);
            Assert.AreEqual(27,     dict["Age"]);
        }

        [TestMethod]
        public async Task Mondo_Database_ExecuteQuery()
        {
            var query = new Query();

            query.SelectFrom("dbo.Customer");

            var dict = await ExecuteQueryResult(query);

            Assert.AreEqual("John", dict["Name"]);
            Assert.AreEqual(27,     dict["Age"]);

            /*******************************************************************************/
            query = new Query();

            query.SelectFrom("dbo.Customer")
                 .Where("Name")
                 .IsEqualTo("Fred");

            dict = await ExecuteQueryResult(query);

            Assert.AreEqual("Fred", dict["Name"]);
            Assert.AreEqual(44,     dict["Age"]);

            /*******************************************************************************/
            query = new Query();

            query.InsertInto("dbo.Customer")
                 .Columns(new string[] {"Name", "Age"})
                 .Values(new object[] {"Dino", 5});

            await ExecuteQuery(query);

            query.SelectFrom("dbo.Customer")
                 .Where("Name")
                 .IsEqualTo("Dino");

            dict = await ExecuteQueryResult(query);

            Assert.AreEqual("Dino", dict["Name"]);
            Assert.AreEqual(5,     dict["Age"]);

            await ExecuteQuery("DELETE FROM dbo.Customer WHERE Name = 'Dino'");

            /*******************************************************************************/
            query = new Query();

            query.Update("dbo.Customer")
                 .Set(new Dictionary<string, object> { 
                                                        {"Age", 45}
                                                     })
                .Where("Name")
                .IsEqualTo("Fred");

            await ExecuteQuery(query);

            query.SelectFrom("dbo.Customer")
                 .Where("Name")
                 .IsEqualTo("Fred");

            dict = await ExecuteQueryResult(query);

            Assert.AreEqual("Fred", dict["Name"]);
            Assert.AreEqual(45,     dict["Age"]);

            // Set it back
            query.Update("dbo.Customer")
                 .Set(new Dictionary<string, object> { 
                                                        {"Age", 44}
                                                     })
                .Where("Name")
                .IsEqualTo("Fred");

            await ExecuteQuery(query);
        }

        /*******************************************************************************/
        private async Task<IDictionary<string, object>> ExecuteQueryResult(Query query)
        {
            using(Database db = Database.Create(_connectionString,  false))
            {
                using(TextCommand sp = new TextCommand(db, query.ToString()))
                {
                    return await db.ExecuteSingleRecordDictionaryAsync(sp);
                }
            }
        }

        /*******************************************************************************/
        private async Task ExecuteQuery(Query query)
        {
            await ExecuteQuery(query.ToString());
        }

        /*******************************************************************************/
        private async Task ExecuteQuery(string text)
        {
            using(Database db = Database.Create(_connectionString, false))
            {
                using(TextCommand sp = new TextCommand(db, text))
                {
                    await db.DoTaskAsync(sp);
                }
            }
        }
    }
}
