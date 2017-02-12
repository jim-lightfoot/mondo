using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mondo.Common.Test
{
    [TestClass]
    public class Mondo_Common_Query_Test
    {
        [TestMethod]
        public void Mondo_Common_Query_Select()
        {
            Query query = new Query();

            Assert.AreEqual("", query.ToString());

            query = new Query();

            query.SelectFrom("Cars");

            Assert.AreEqual("SELECT*FROM[Cars]", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.Select(new string[] {"Make", "Model", "Year"})
                 .From("Cars");

            Assert.AreEqual("SELECT[Make],[Model],[Year]FROM[Cars]", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.Select()
                 .Top(100)
                 .Columns(new string[] {"c.Make", "c.Model", "c.Year"})
                 .From("Cars c");

            Assert.AreEqual("SELECTTOP100c.[Make],c.[Model],c.[Year]FROM[Cars]c", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.Select()
                 .Top("@top")
                 .Columns(new string[] {"Make", "Model", "Year"})
                 .From("Cars");

            Assert.AreEqual("SELECTTOP(@top)[Make],[Model],[Year]FROM[Cars]", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.Select()
                 .Top("@top")
                 .All()
                 .From("Cars");

            Assert.AreEqual("SELECTTOP(@top)*FROM[Cars]", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Make")
                 .IsEqualTo("Chevy");

            Assert.AreEqual("SELECT*FROM[Cars]WHERE([Make]='Chevy')", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Make")
                 .IsEqualTo("@make");

            Assert.AreEqual("SELECT*FROM[Cars]WHERE([Make]=@make)", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Year")
                 .IsEqualTo(1969);

            Assert.AreEqual("SELECT*FROM[Cars]WHERE([Year]=1969)", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Displacement")
                 .IsEqualTo(7.5);

            Assert.AreEqual("SELECT*FROM[Cars]WHERE([Displacement]=7.5)", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Displacement")
                 .IsGreaterThan(7.5);

            Assert.AreEqual("SELECT*FROM[Cars]WHERE([Displacement]>7.5)", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Displacement")
                 .IsLessThan(7.5);

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Bought")
                 .IsLessThan(DateTime.Parse("2017-01-01T12:00"));

            Assert.AreEqual("SELECT*FROM[Cars]WHERE([Bought]<'2017-01-01T12:00:00')", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Displacement")
                 .IsGreaterThanOrEqualTo(7.5);

            Assert.AreEqual("SELECT*FROM[Cars]WHERE([Displacement]>=7.5)", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Displacement")
                 .IsLessThanOrEqualTo(7.5);

            Assert.AreEqual("SELECT*FROM[Cars]WHERE([Displacement]<=7.5)", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Make")
                 .IsIn(new string[] {"Chevy", "Pontiac", "Audi"});

            Assert.AreEqual("SELECT*FROM[Cars]WHERE([Make]in('Chevy','Pontiac','Audi'))", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Make")
                 .IsEqualTo("Chevy")
                 .And("Model")
                 .IsEqualTo("Malibu");

            Assert.AreEqual("SELECT*FROM[Cars]WHERE([Make]='Chevy')AND([Model]='Malibu')", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Make")
                 .IsEqualTo("Chevy")
                 .Or("Model")
                 .IsEqualTo("Malibu");

            Assert.AreEqual("SELECT*FROM[Cars]WHERE([Make]='Chevy')OR([Model]='Malibu')", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Make")
                 .IsEqualTo("Chevy")
                 .AndWhere("Model")
                 .IsEqualTo("Malibu")
                 .Or("Model")
                 .IsEqualTo("Camaro")
                 .End();

            Assert.AreEqual("SELECT*FROM[Cars]WHERE([Make]='Chevy')AND(([Model]='Malibu')OR([Model]='Camaro'))", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Make")
                 .IsEqualTo("Chevy")
                 .AndWhere("Model")
                 .IsEqualTo("Malibu")
                 .Or("Model")
                 .IsEqualTo("Camaro")
                 .End()
                 .OrderBy("Model");

            Assert.AreEqual("SELECT*FROM[Cars]WHERE([Make]='Chevy')AND(([Model]='Malibu')OR([Model]='Camaro'))ORDERBY[Model]ASC", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Make")
                 .IsEqualTo("Chevy")
                 .AndWhere("Model")
                 .IsEqualTo("Malibu")
                 .Or("Model")
                 .IsEqualTo("Camaro")
                 .End()
                 .OrderBy(new string[] {"Make", "Model"});

            Assert.AreEqual("SELECT*FROM[Cars]WHERE([Make]='Chevy')AND(([Model]='Malibu')OR([Model]='Camaro'))ORDERBY[Make]ASC,[Model]ASC", query.ToString().Replace(" ", "").Replace("\r\n", ""));
        }

        [TestMethod]
        public void Mondo_Common_Query_Joins()
        {
            Query query = new Query();

            Assert.AreEqual("", query.ToString());

            query = new Query();

            query.SelectFrom("Cars c")
            .InnerJoin("Owners o")
            .On("o.CarID")
            .IsEqualTo("c.CarID");
            
            Assert.AreEqual("SELECT*FROM[Cars]cINNERJOIN[Owners]oONo.[CarID]=c.[CarID]", query.ToString().Replace(" ", "").Replace("\r\n", ""));
        }

        [TestMethod]
        public void Mondo_Common_Query_Update()
        {
            Query query = new Query();

            query.Update("dbo.Cars");

            Assert.AreEqual("UPDATE[dbo].[Cars]", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.Update("dbo.Cars")
                 .Set(new Dictionary<string, object> { 
                                                        {"Make",  "Chevy"},
                                                        {"Model", "Malibu"},
                                                        {"Year",  1969}
                                                     });

            Assert.AreEqual("UPDATE[dbo].[Cars]SET[Make]='Chevy',[Model]='Malibu',[Year]=1969", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.Update("dbo.Cars")
                 .Set(new Dictionary<string, object> { 
                                                        {"Make",  "Chevy"},
                                                        {"Model", "Malibu"},
                                                        {"Year",  1969}
                                                     })
                .Where("Owner")
                .IsEqualTo("Jim");

            Assert.AreEqual("UPDATE[dbo].[Cars]SET[Make]='Chevy',[Model]='Malibu',[Year]=1969WHERE([Owner]='Jim')", query.ToString().Replace(" ", "").Replace("\r\n", ""));

        }

        [TestMethod]
        public void Mondo_Common_Query_Insert()
        {
            Query query = new Query();

            query.InsertInto("dbo.Cars");

            Assert.AreEqual("INSERTINTO[dbo].[Cars]", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.InsertInto("dbo.Cars")
                 .Columns(new string[] {"Make", "Model", "Year"});

            Assert.AreEqual("INSERTINTO[dbo].[Cars]([Make],[Model],[Year])", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.InsertInto("dbo.Cars")
                 .Columns(new string[] {"Make", "Model", "Year"})
                 .Values(new object[] {"Chevy", "Malibu", 1969});

            Assert.AreEqual("INSERTINTO[dbo].[Cars]([Make],[Model],[Year])VALUES('Chevy','Malibu',1969)", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.InsertInto("dbo.Cars")
                 .Columns(new string[] {"Make", "Model", "Year"})
                 .Select(new string[] {"Make", "Model", "Year"})
                 .From("MyCars");

            Assert.AreEqual("INSERTINTO[dbo].[Cars]([Make],[Model],[Year])SELECT[Make],[Model],[Year]FROM[MyCars]", query.ToString().Replace(" ", "").Replace("\r\n", ""));
        }

        [TestMethod]
        public void Mondo_Common_Query_Delete()
        {
            Query query = new Query();

            Assert.AreEqual("", query.ToString());

            query = new Query();

            query.DeleteFrom("Cars");

            Assert.AreEqual("DELETEFROM[Cars]", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.DeleteFrom("Cars") 
                 .Where("Make")
                 .IsEqualTo("Chevy");

            Assert.AreEqual("DELETEFROM[Cars]WHERE([Make]='Chevy')", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.DeleteFrom("Cars") 
                 .Where("Make")
                 .IsEqualTo("@make");

            Assert.AreEqual("DELETEFROM[Cars]WHERE([Make]=@make)", query.ToString().Replace(" ", "").Replace("\r\n", ""));
        }
    }
}
