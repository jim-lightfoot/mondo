using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mondo.Common;

namespace Mondo.Test
{
    [TestClass]
    public class DiskFile
    {
        /****************************************************************************/
        [TestMethod]
        public async Task DiskFile_LoadFileAsync()
        {
            string path = "D:\\Development\\Projects\\Libraries\\Mondo\\Common\\config.cs";

            string text = await Mondo.Common.DiskFile.LoadFileAsync(path);

            Assert.IsTrue(text.Length > 100);
            Assert.IsTrue(text.Contains("Get(string strAttrName, bool bRequired = false)"));
        }

        /****************************************************************************/
        [TestMethod]
        public async Task DiskFile_LoadFileStreamAsync()
        {
            string path = "D:\\Development\\Projects\\Libraries\\Mondo\\Common\\config.cs";

            MemoryStream strm = await Mondo.Common.DiskFile.LoadFileStreamAsync(path);

            var text = Utility.StreamToString(strm);

            Assert.IsTrue(text.Length > 100);
            Assert.IsTrue(text.Contains("Get(string strAttrName, bool bRequired = false)"));
        }
    }
}
