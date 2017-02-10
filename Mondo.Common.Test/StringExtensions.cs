using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mondo.Common;

namespace MondoTest
{
    [TestClass]
    public class StringExtensions
    {
        [TestMethod]
        public void Mondo_Common_StringExtensions_Normalized()
        {
            string s = null;

            Assert.AreEqual("bob", "  bob  ".Normalized());
            Assert.AreEqual("", s.Normalized());
        }

        [TestMethod]
        public void Mondo_Common_StringExtensions_IsEmpty()
        {
            string s = null;

            Assert.IsFalse("bob".IsEmpty());
            Assert.IsFalse(" bob  ".IsEmpty());
            Assert.IsTrue(s.IsEmpty());
            Assert.IsTrue("".IsEmpty());
            Assert.IsTrue("  ".IsEmpty());
        }

        [TestMethod]
        public void Mondo_Common_StringExtensions_AppendSpaces()
        {
            StringBuilder sb = new StringBuilder("bob");

            Assert.AreEqual("bob ", sb.AppendSpaces(1).ToString());
            Assert.AreEqual("bob     ", sb.AppendSpaces(4).ToString());
        }

        [TestMethod]
        public void Mondo_Common_StringExtensions_NormalizeNewLines()
        {
            string s = null;

            Assert.AreEqual("", s.NormalizeNewLines());
            Assert.AreEqual("bob\r\nbob", "bob\rbob".NormalizeNewLines());
            Assert.AreEqual("bob\r\nbob", "bob\nbob".NormalizeNewLines());
            Assert.AreEqual("bob\r\n\r\nbob", "bob\r\n\nbob".NormalizeNewLines());
            Assert.AreEqual("bob\r\n\r\nbob", "bob\r\r\nbob".NormalizeNewLines());
        }

        [TestMethod]
        public void Mondo_Common_StringExtensions_StripAfterLast()
        {
            Assert.AreEqual("abc", "abc}".StripAfterLast("}", true));
            Assert.AreEqual("",    "}aa".StripAfterLast("}", true));
            Assert.AreEqual("aba", "aba".StripAfterLast("}", true));
            Assert.AreEqual("abc", "abc}def".StripAfterLast("}", true));
            Assert.AreEqual("",    "}".StripAfterLast("}", true));

            Assert.AreEqual("abc}", "abc}".StripAfterLast("}", false));
            Assert.AreEqual("}",    "}aa".StripAfterLast("}", false));
            Assert.AreEqual("aba", "aba".StripAfterLast("}", false));
            Assert.AreEqual("abc}", "abc}def".StripAfterLast("}"));
            Assert.AreEqual("}",    "}".StripAfterLast("}"));
        }

        [TestMethod]
        public void Mondo_Common_StringExtensions_NormalizeSpace()
        {
            string s = null;

            Assert.AreEqual("", s.NormalizeSpace());
            Assert.AreEqual("", "".NormalizeSpace());
            Assert.AreEqual("", " ".NormalizeSpace());
            Assert.AreEqual("bob", "bob".NormalizeSpace());
            Assert.AreEqual("bob", " bob ".NormalizeSpace());
            Assert.AreEqual("bob is a friend", " bob  is   a   friend   ".NormalizeSpace());
        }

        [TestMethod]
        public void Mondo_Common_StringExtensions_Truncate()
        {
            string s = null;

            Assert.AreEqual("", s.Truncate(4));
            Assert.AreEqual("", " ".Truncate(1));
            Assert.AreEqual("bo", " bob ".Truncate(2));
            Assert.AreEqual("bob is", "bob is a friend".Truncate(7));
            Assert.AreEqual("bob is a fr", "bob is a friend".Truncate(11));
        }

        [TestMethod]
        public void Mondo_Common_StringExtensions_EnsureEndsWith()
        {
            string s = null;

            Assert.AreEqual("bob", s.EnsureEndsWith("bob"));
            Assert.AreEqual("bob's", "bob".EnsureEndsWith("'s"));
            Assert.AreEqual("bob's", "bob's".EnsureEndsWith("'s"));
            Assert.AreEqual("bob's\r\n", "bob's".EnsureEndsWith("\r\n"));

            Assert.AreEqual("m", s.EnsureEndsWith('m'));
            Assert.AreEqual("bobm", "bob".EnsureEndsWith('m'));
            Assert.AreEqual("bobm", "bobm".EnsureEndsWith('m'));
            Assert.AreEqual("bob'sm", "bob's".EnsureEndsWith('m'));
        }

        [TestMethod]
        public void Mondo_Common_StringExtensions_EnsureEndsWithSeparator()
        {
            string s = null;
            string sep = System.IO.Path.DirectorySeparatorChar.ToString();

            Assert.AreEqual(sep, s.EnsureEndsWithSeparator());
            Assert.AreEqual("bob" + sep, ("bob" + sep).EnsureEndsWithSeparator());
            Assert.AreEqual("bob" + sep, ("bob").EnsureEndsWithSeparator());
        }

        [TestMethod]
        public void Mondo_Common_StringExtensions_EnsureNotEndsWithSeparator()
        {
            string s = null;

            Assert.AreEqual("", s.EnsureNotEndsWithSeparator());
            Assert.AreEqual("bob", ("bob" + System.IO.Path.DirectorySeparatorChar).EnsureNotEndsWithSeparator());
            Assert.AreEqual("bob", ("bob").EnsureNotEndsWithSeparator());
        }

        [TestMethod]
        public void Mondo_Common_StringExtensions_RemoveLastChar()
        {
            string s = null;

            Assert.AreEqual("", s.RemoveLastChar());
            Assert.AreEqual("bo", ("bob").RemoveLastChar());
            Assert.AreEqual("bob", ("bob  ").RemoveLastChar());
        }

        [TestMethod]
        public void Mondo_Common_StringExtensions_RemoveLast()
        {
            string s = null;

            Assert.AreEqual("", s.RemoveLast("bob"));
            Assert.AreEqual("bo", "bob".RemoveLast("b"));
            Assert.AreEqual("bo", "bob  ".RemoveLast("b"));
            Assert.AreEqual("bob", "bob  ".RemoveLast("m"));
            Assert.AreEqual("", "bob  ".RemoveLast("bob"));
        }

        [TestMethod]
        public void Mondo_Common_StringExtensions_StripAfter()
        {
            string s = null;

            Assert.AreEqual("", s.StripAfter("bob"));
            Assert.AreEqual("b", "bob".StripAfter("o"));
            Assert.AreEqual("b", "bob  ".StripAfter("ob"));
            Assert.AreEqual("bob", "bob  roberts  ".StripAfter("roberts"));
            Assert.AreEqual("bob", "bob  ".StripAfter("m"));
            Assert.AreEqual("", "bob  ".StripAfter("bob"));
        }

        [TestMethod]
        public void Mondo_Common_StringExtensions_StripUpTo()
        {
            string s = null;

            Assert.AreEqual("", s.StripUpTo("bob"));
            Assert.AreEqual("b", "bob".StripUpTo("o"));
            Assert.AreEqual("", "bob  ".StripUpTo("ob"));
            Assert.AreEqual("", "bob  roberts  ".StripUpTo("roberts"));
            Assert.AreEqual("jones", "bob  roberts  jones".StripUpTo("roberts"));
            Assert.AreEqual("bob", "bob  ".StripUpTo("m"));
            Assert.AreEqual("bob bob", "bob bob bob  ".StripUpTo("bob"));
        }
    }
}
