using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mondo.Common.Test
{
    [TestClass]
    public class Mondo_Common_Query_Test
    {
        [TestMethod]
        public void Mondo_Common_Query_Test1()
        {
            Query query = new Query();

            Assert.AreEqual("", query.ToString());

            query = new Query();

            query.SelectFrom("Cars");

            Assert.AreEqual("SELECT*FROMCars", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.Select(new string[] {"Make", "Model", "Year"})
                 .From("Cars");

            Assert.AreEqual("SELECTMake,Model,YearFROMCars", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Make")
                 .IsEqualTo("Chevy");

            Assert.AreEqual("SELECT*FROMCarsWHERE(Make='Chevy')", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Make")
                 .IsEqualTo("@make");

            Assert.AreEqual("SELECT*FROMCarsWHERE(Make=@make)", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Year")
                 .IsEqualTo(1969);

            Assert.AreEqual("SELECT*FROMCarsWHERE(Year=1969)", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Displacement")
                 .IsEqualTo(7.5);

            Assert.AreEqual("SELECT*FROMCarsWHERE(Displacement=7.5)", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Displacement")
                 .IsGreaterThan(7.5);

            Assert.AreEqual("SELECT*FROMCarsWHERE(Displacement>7.5)", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Displacement")
                 .IsLessThan(7.5);

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Bought")
                 .IsLessThan(DateTime.Parse("2017-01-01T12:00"));

            Assert.AreEqual("SELECT*FROMCarsWHERE(Bought<'2017-01-01T12:00:00')", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Displacement")
                 .IsGreaterThanOrEqualTo(7.5);

            Assert.AreEqual("SELECT*FROMCarsWHERE(Displacement>=7.5)", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Displacement")
                 .IsLessThanOrEqualTo(7.5);

            Assert.AreEqual("SELECT*FROMCarsWHERE(Displacement<=7.5)", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Make")
                 .IsIn(new string[] {"Chevy", "Pontiac", "Audi"});

            Assert.AreEqual("SELECT*FROMCarsWHERE(Makein('Chevy','Pontiac','Audi'))", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Make")
                 .IsEqualTo("Chevy")
                 .And("Model")
                 .IsEqualTo("Malibu");

            Assert.AreEqual("SELECT*FROMCarsWHERE(Make='Chevy')AND(Model='Malibu')", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Make")
                 .IsEqualTo("Chevy")
                 .Or("Model")
                 .IsEqualTo("Malibu");

            Assert.AreEqual("SELECT*FROMCarsWHERE(Make='Chevy')OR(Model='Malibu')", query.ToString().Replace(" ", "").Replace("\r\n", ""));

            query = new Query();

            query.SelectFrom("Cars") 
                 .Where("Make")
                 .IsEqualTo("Chevy")
                 .AndWhere("Model")
                 .IsEqualTo("Malibu")
                 .Or("Model")
                 .IsEqualTo("Camaro")
                 .End();

            Assert.AreEqual("SELECT*FROMCarsWHERE(Make='Chevy')AND((Model='Malibu')OR(Model='Camaro'))", query.ToString().Replace(" ", "").Replace("\r\n", ""));

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

            Assert.AreEqual("SELECT*FROMCarsWHERE(Make='Chevy')AND((Model='Malibu')OR(Model='Camaro'))ORDERBYModelASC", query.ToString().Replace(" ", "").Replace("\r\n", ""));

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

            Assert.AreEqual("SELECT*FROMCarsWHERE(Make='Chevy')AND((Model='Malibu')OR(Model='Camaro'))ORDERBYMakeASC,ModelASC", query.ToString().Replace(" ", "").Replace("\r\n", ""));
        }
    }
}
