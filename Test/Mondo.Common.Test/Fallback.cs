using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mondo.Common.Test
{
    [TestClass]
    public class Fallback_Test
    {
        /****************************************************************************/
        [TestMethod]
        public void Mondo_Common_Fallback_Run()
        {
            var return1 = Fallback.Run<string, string>(new string[] {"Broccoli", "Spinach", "Potatoes" }, 
                                         (item)=>
            {
                return item;  
            });

            Assert.AreEqual("Broccoli", return1);

            var return2 = Fallback.Run<string, string>(new string[] {"Broccoli", "Spinach", "Potatoes" }, 
                                         (item)=>
            {
                if(item == "Broccoli")
                    throw new Exception("Nope, don't like Broccoli");

                return item;  
            });

            Assert.AreEqual("Spinach", return2);

            var return3 = Fallback.Run<string, string>(new string[] {"Broccoli", "Spinach", "Potatoes" }, 
                                         (item)=>
            {
                if(item == "Broccoli")
                    throw new Exception("Nope, don't like Broccoli");

                if(item == "Spinach")
                    throw new Exception("Nope, don't like Spinach");

                return item;  
            });

            Assert.AreEqual("Potatoes", return3);

            try
            {
                var return4 = Fallback.Run<string, string>(new string[] { "Broccoli", "Spinach", "Potatoes" },
                                             (item) =>
                {
                    if (item == "Broccoli")
                        throw new Exception("Nope, don't like Broccoli");

                    if (item == "Spinach")
                        throw new Exception("Nope, don't like Spinach");

                    if (item == "Potatoes")
                        throw new Exception("Nope, don't like Potatoes");

                    return item;
                });

                Assert.Fail("Expected exception!");
            }
            catch (Exception ex4)
            {
                Assert.AreEqual("Nope, don't like Potatoes", ex4.Message);
            }
        }
    }
}
