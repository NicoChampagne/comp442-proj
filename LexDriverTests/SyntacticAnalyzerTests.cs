using System.Collections.Generic;
using System.Linq;
using LexDriver;
using SynDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LexDriverTests
{
    [TestClass]
    public class SyntacticAnalyzerTests
    {
        public static List<TokenData> tokensToParse = new List<TokenData>
        {
            new TokenData("main","main",1,1),
            new TokenData("do","do",1,2),
            new TokenData("end","end",1,3),
        };

        public static List<TokenData> tokensToParseWithError = new List<TokenData>
        {
            new TokenData("main","main",1,1),
            new TokenData("main","main",1,2),
            new TokenData("do","do",1,2),
            new TokenData("end","end",1,3),
        };

        [TestMethod]
        public void ParseRunsTest()
        {
            var result = SyntacticAnalyzer.Parse(tokensToParse);
            var errors = result.errorList;

            Assert.IsTrue(errors.Count == 0);
        }

        [TestMethod]
        public void ParseWithErrorRunsTest()
        {
            var result = SyntacticAnalyzer.Parse(tokensToParseWithError);
            var errors = result.errorList;

            Assert.IsTrue(errors.Count == 1);
        }
    }
}
