using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mondo.Web;

namespace Mondo.Web.Test
{
    [TestClass]
    public class HtmlToXmlTest
    {
        [TestMethod]
        public void Mondo_Web_HtmlToXml_ProcessAttributes()
        {
            HtmlToXml.Element obj = new HtmlToXml.Element(null, "div");

            obj.ProcessAttributes("class='test'");

            Assert.AreEqual(1, obj.Attributes.Count);
            Assert.AreEqual("class", obj.Attributes[0].Name);
            Assert.AreEqual("test", obj.Attributes[0].Value);

        }

        public void Mondo_Web_HtmlToXml_Process()
        {
            string input = "<p><html><p>Come enjoy this unique and exciting concert dedicated to <strong>Romanian </strong><span class=\"blah blah blah blah\" /> culture! The concert program&nbsp;will feature masterworks by Béla Bartók and George Enescu. </p><p><br /></p><p><strong>Experience </strong>the fire, drama and fine lyricism of Romanian Art Songs and Romanian Folk Dances by Béla Bartók,&nbsp;and Sonata No. 3 'in <strong><em><u>Romanian </u></em></strong>Folk Character' for <em>violin </em>and piano, one of the most popular and critically respected works by George Enescu. All of these pieces take Romanian folk dances and songs as their point of departure. </p><p><br /></p><p>Distinguished and versatile violinist Mikhail Shmidt, acclaimed soprano Marcy Stonikas, and sought-after pianist Oana <strike>Rusu </strike>Tomai will perform.</p><p><br /></p><!-- br--> <p>This concert is presented by the <em>Romanian-American Society of Washington State</em>.</p><p><br /></p><!-- br--> <p>All seating is general admission.</p></html></p>";

            HtmlToXml objProcessor = new HtmlToXml();

            string result = objProcessor.Process(input, true, true, false);

            Assert.AreNotEqual("", result);
        }
    }
}
