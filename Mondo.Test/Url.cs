using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mondo.Common;

namespace Mondo.Test
{
    [TestClass]
    public class Url_Test
    {
        [TestMethod]
        public void Url_Combine()
        {
            Assert.AreEqual("http://blob/fred",                   Url.Combine("http://blob", "fred"));
            Assert.AreEqual("http://blob/fred/john",              Url.Combine("http://blob", "fred", "/john/"));
            Assert.AreEqual("http://blob/fred/john",              Url.Combine("http://blob/", "/fred/", "john/"));
            Assert.AreEqual("http://blob/fred/john/wilma",        Url.Combine("http://blob/", "/fred/", "john/", "wilma"));
            Assert.AreEqual("http://blob/fred/john/wilma/barney", Url.Combine("http://blob//", "/fred/", "john//", "wilma ", "//barney/"));
            Assert.AreEqual("http://blob/fred/john/wilma/barney", Url.Combine("http://blob//", "/fred/", "john//", "", "wilma", "//barney/"));
            Assert.AreEqual("http://blob/fred/john/wilma/barney", Url.Combine("http://blob//", "/fred/", "john//", "", " wilma", "  ", "//barney/"));
        }
    }
}
