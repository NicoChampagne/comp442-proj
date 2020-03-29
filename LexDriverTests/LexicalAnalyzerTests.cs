using System;
using LexDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LexDriverTests
{
    [TestClass]
    public class LexicalAnalyzerTests
    {
        [TestMethod]
        public void IdTokenTest()
        {
            var token = LexicalAnalyzer.Analyze("a394jojfsd_3498fds", 1, 1);
            Assert.IsTrue(token.Type.Equals("id"));
        }

        [TestMethod]
        public void IdErrorTokenTest()
        {
            var token = LexicalAnalyzer.Analyze("a394jojfsd_3498fds&", 1, 1);
            Assert.IsTrue(token.Type.Contains("Invalid"));
        }

        [TestMethod]
        public void DictionaryTokenTest()
        {
            var token = LexicalAnalyzer.Analyze("=", 1, 1);
            Assert.IsTrue(token.Type.Equals("assign"));
        }

        [TestMethod]
        public void ErrorTokenTest()
        {
            var token = LexicalAnalyzer.Analyze("&", 1, 1);
            Assert.IsTrue(token.Type.Contains("Invalid"));
        }

        [TestMethod]
        public void BlockCommentTokenTest()
        {
            var token = LexicalAnalyzer.Analyze("//", 1, 1);
            Assert.IsTrue(token.Type.Equals("blockcmt"));
        }

        [TestMethod]
        public void InlineCommentTokenTest()
        {
            var token = LexicalAnalyzer.Analyze("/*", 1, 1);
            var token2 = LexicalAnalyzer.Analyze("*/", 1, 1);

            Assert.IsTrue(token.Type.Contains("inlinecmt"));
            Assert.IsTrue(token2.Type.Contains("inlinecmt"));
        }

        [TestMethod]
        public void ZeroIntegerTokenTest()
        {
            var token = LexicalAnalyzer.Analyze("123", 1, 1);
            Assert.IsTrue(token.Type.Equals("intnum"));
        }

        [TestMethod]
        public void FloatTokenTest()
        {
            var token = LexicalAnalyzer.Analyze("2e+1", 1, 1);
            Assert.IsTrue(token.Type.Contains("floatnum"));
        }

        [TestMethod]
        public void TokenTest_GradingInt1()
        {
            var token = LexicalAnalyzer.Analyze("0", 1, 1);
            Assert.IsTrue(token.Type.Equals("intnum"));
        }

        [TestMethod]
        public void TokenTest_GradingInt2()
        {
            var token = LexicalAnalyzer.Analyze("1", 1, 1);
            Assert.IsTrue(token.Type.Equals("intnum"));
        }
    }
}
