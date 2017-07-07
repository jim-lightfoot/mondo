using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mondo.Rest;

namespace Mondo.Rest.Test
{
    [TestClass]
    public class WebService_Test
    {
        [TestMethod]
        public async Task Mondo_Rest_WebService_Get()
        {
            var svc    = new WebService("http://bnb.data.bl.uk/doc/data");
            var result = await svc.Get("/BNB.json");

            Assert.AreEqual("linked-data-api", result["format"].ToString());
        }
    }
}
