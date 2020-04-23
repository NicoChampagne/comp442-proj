using System.Collections.Generic;
using System.Linq;
using LexDriver;
using SynSemDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LexDriverTests
{
    [TestClass]
    public class SemanticAnalyzerTests
    {
        public static List<TokenData> tokensToParse = new List<TokenData>
        {
            new TokenData("main","main",1,1),
            new TokenData("do","do",1,2),
            new TokenData("end","end",1,3),
        };

        public static List<TokenData> tokensToParseWithSemError = new List<TokenData>
        {
            new TokenData("main","main",1,1),
            new TokenData("do","do",1,2),
            new TokenData("integer","integer",1,3),
            new TokenData("id","n",1,3),
            new TokenData("semi",";",1,3),
            new TokenData("end","end",1,4),
        };

        public static List<TokenData> tokensForBubbleSort = new List<TokenData>
        {
            new TokenData("id", "bubbleSort",2,0),
            new TokenData("openpar","(", 2, 1),
            new TokenData("integer", "integer",2,2),
            new TokenData("id", "arr",2,3),
            new TokenData("opensqbr","[",2,4),
            new TokenData("closesqbr","]",2,5),
            new TokenData("comma",",",2,6),
            new TokenData("integer", "integer",2,7),
            new TokenData("id", "size",2,8),
            new TokenData("closepar",")",2,9),
             new TokenData("colon",":", 2, 10),
             new TokenData("void","void",2,11),
             new TokenData("local", "local",3,1),
             new TokenData("integer", "integer",4,1),
             new TokenData("id", "n",4,2),
             new TokenData("semi",";",4,3),
             new TokenData("integer", "integer",5,1),
             new TokenData("id", "i",5,2),
             new TokenData("semi",";",5,3),
             new TokenData("integer", "integer",6,1),
             new TokenData("id", "j",6,2),
             new TokenData("semi",";",6,3),
             new TokenData("integer", "integer",7,1),
             new TokenData("id", "temp",7,2),
             new TokenData("semi",";",7,3),
             new TokenData("do","do",8,1),
             new TokenData("id", "n",9,1),
             new TokenData("assign","=",9,2),
             new TokenData("id", "size",9,3),
             new TokenData("semi",";",9,4),
             new TokenData("id", "i",10,1),
             new TokenData("assign","=",10,2),
             new TokenData("intnum","0",10,3),
             new TokenData("semi",";",10,4),
             new TokenData("id", "j",11,1),
             new TokenData("assign","=",11,2),
             new TokenData("intnum","0",11,3),
             new TokenData("semi",";",11,4),
             new TokenData("id", "temp",12,1),
             new TokenData("assign","=",12,2),
            new TokenData("intnum","0",12,3),
            new TokenData("semi",";",12,4),
            new TokenData("while","while",13,1),
            new TokenData("openpar","(", 13, 2),
            new TokenData("id", "i",13,3),
            new TokenData("lt","<",13,4),
            new TokenData("id", "n",13,5),
            new TokenData("minus","-",13,6),
            new TokenData("intnum","1",13,7),
            new TokenData("closepar",")",13,8),
            new TokenData("do","do",14,1),
            new TokenData("while","while",15,1),
            new TokenData("openpar","(", 15, 2),
            new TokenData("id", "j",15,3),
            new TokenData("lt","<",15,4),
            new TokenData("id", "n",15,5),
            new TokenData("minus","-",15,6),
            new TokenData("id", "i",15,7),
            new TokenData("minus","-",15,8),
            new TokenData("intnum","1",15,9),
            new TokenData("closepar",")",15,10),
            new TokenData("do","do",16,1),
            new TokenData("if","if",17,1),
            new TokenData("openpar","(", 17, 2),
            new TokenData("id", "arr",17,3),
            new TokenData("opensqbr","[",17,4),
            new TokenData("id", "j",17,5),
            new TokenData("closesqbr","]",17,6),
            new TokenData("gt",">",17,7),
            new TokenData("id", "arr",17,8),
            new TokenData("opensqbr","[",17,9),
            new TokenData("id", "j",17,10),
            new TokenData("plus","+",17,11),
            new TokenData("intnum","1",17,12),
            new TokenData("closesqbr","]",17,13),
            new TokenData("closepar",")",17,14),
            new TokenData("then", "then",18,1),
            new TokenData("do","do",19,1),
            new TokenData("id", "temp",21,1),
            new TokenData("assign","=",21,2),
            new TokenData("id", "arr",21,3),
            new TokenData("opensqbr","[",21,4),
            new TokenData("id", "j",21,5),
            new TokenData("closesqbr","]",21,6),
            new TokenData("semi",";",21,7),
            new TokenData("id", "arr",22,1),
            new TokenData("opensqbr","[",22,2),
            new TokenData("id", "j",22,3),
            new TokenData("closesqbr","]",22,4),
            new TokenData("assign","=",22,5),
            new TokenData("id", "arr",22,6),
            new TokenData("opensqbr","[",22,7),
            new TokenData("id", "j",22,8),
            new TokenData("plus","+",22,9),
            new TokenData("intnum","1",22,10),
            new TokenData("closesqbr","]",22,11),
            new TokenData("semi",";",22,12),
            new TokenData("id","arr",23,1),
            new TokenData("opensqbr","[",23,2),
            new TokenData("id", "j",23,3),
            new TokenData("plus","+",23,4),
            new TokenData("intnum","1",23,5),
            new TokenData("closesqbr","]",23,6),
            new TokenData("assign","=",23,7),
            new TokenData("id", "temp",23,8),
            new TokenData("semi",";",23,9),
            new TokenData("end", "end",24,1),
            new TokenData("else","else",25,1),
            new TokenData("semi",";",26,1),
            new TokenData("id", "j",27,1),
            new TokenData("assign","=",27,2),
            new TokenData("id", "j",27,3),
            new TokenData("plus","+",27,4),
            new TokenData("intnum","1",27,5),
            new TokenData("semi",";",27,6),
            new TokenData("end", "end",28,1),
            new TokenData("semi",";",28,2),
            new TokenData("id", "i",29,1),
            new TokenData("assign","=",29,2),
            new TokenData("id", "i",29,3),
            new TokenData("plus","+",29,4),
            new TokenData("intnum","1",29,5),
            new TokenData("semi",";",29,6),
            new TokenData("end", "end",30,1),
            new TokenData("semi",";",30,2),
            new TokenData("end", "end",31,1),
            new TokenData("id", "printArray",34,0),
            new TokenData("openpar","(", 34, 1),
            new TokenData("integer", "integer",34,2),
            new TokenData("id", "arr",34,3),
            new TokenData("opensqbr","[",34,4),
            new TokenData("closesqbr","]",34,5),
            new TokenData("comma",",",34,6),
            new TokenData("integer", "integer",34,7),
            new TokenData("id", "size",34,8),
            new TokenData("closepar",")",34,9),
            new TokenData("colon",":",34,10),
            new TokenData("void","void",34,11),
            new TokenData("local", "local",35,1),
            new TokenData("integer", "integer",36,1),
            new TokenData("id", "n",36,2),
            new TokenData("semi",";",36,3),
            new TokenData("integer", "integer",37,1),
            new TokenData("id", "i",37,2),
            new TokenData("semi",";",37,3),
            new TokenData("do","do",38,1),
            new TokenData("id", "n",39,1),
            new TokenData("assign","=",39,2),
            new TokenData("id", "size",39,3),
            new TokenData("semi",";",39,4),
            new TokenData("id", "i",40,1),
            new TokenData("assign","=",40,2),
            new TokenData("intnum","0",40,3),
            new TokenData("semi",";",40,4),
            new TokenData("while","while",41,1),
            new TokenData("openpar","(", 41, 2),
            new TokenData("id", "i",41,3),
            new TokenData("lt","<",41,4),
            new TokenData("id", "n",41,5),
            new TokenData("closepar",")",41,6),
            new TokenData("do","do",42,1),
            new TokenData("write", "write",43,1),
            new TokenData("openpar","(", 43, 2),
            new TokenData("id", "arr",43,3),
            new TokenData("opensqbr","[",43,4),
            new TokenData("id", "i",43,5),
            new TokenData("closesqbr","]",43,6),
            new TokenData("closepar",")",43,7),
            new TokenData("semi",";",43,8),
            new TokenData("id", "i",44,1),
            new TokenData("assign","=",44,2),
            new TokenData("id", "i",44,3),
            new TokenData("plus","+",44,4),
            new TokenData("intnum","1",44,5),
            new TokenData("semi",";",44,6),
            new TokenData("end", "end",45,1),
            new TokenData("semi",";",45,2),
            new TokenData("end", "end",46,1),
            new TokenData("main", "main",49,0),
            new TokenData("local", "local",50,1),
            new TokenData("integer", "integer",51,1),
            new TokenData("id", "arr",51,2),
            new TokenData("opensqbr","[",51,3),
            new TokenData("intnum","7",51,4),
            new TokenData("closesqbr","]",51,5),
            new TokenData("semi",";",51,6),
            new TokenData("do","do",52,1),
            new TokenData("id", "arr",53,1),
            new TokenData("opensqbr","[",53,2),
            new TokenData("intnum","0",53,3),
            new TokenData("closesqbr","]",53,4),
            new TokenData("assign","=",53,5),
            new TokenData("intnum","64",53,6),
            new TokenData("semi",";",53,7),
            new TokenData("id", "arr",54,1),
            new TokenData("opensqbr","[",54,2),
            new TokenData("intnum","1",54,3),
            new TokenData("closesqbr","]",54,4),
            new TokenData("assign","=",54,5),
            new TokenData("intnum","34",54,6),
            new TokenData("semi",";",54,7),
            new TokenData("id", "arr",55,1),
            new TokenData("opensqbr","[",55,2),
            new TokenData("intnum","2",55,3),
            new TokenData("closesqbr","]",55,4),
            new TokenData("assign","=",55,5),
            new TokenData("intnum","25",55,6),
            new TokenData("semi",";",55,7),
            new TokenData("id", "arr",56,1),
            new TokenData("opensqbr","[",56,2),
            new TokenData("intnum","3",56,3),
            new TokenData("closesqbr","]",56,4),
            new TokenData("assign","=",56,5),
            new TokenData("intnum","12",56,6),
            new TokenData("semi",";",56,7),
            new TokenData("id", "arr",57,1),
            new TokenData("opensqbr","[",57,2),
            new TokenData("intnum","4",57,3),
            new TokenData("closesqbr","]",57,4),
            new TokenData("assign","=",57,5),
            new TokenData("intnum","22",57,6),
            new TokenData("semi",";",57,7),
            new TokenData("id", "arr",58,1),
            new TokenData("opensqbr","[",58,2),
            new TokenData("intnum","5",58,3),
            new TokenData("closesqbr","]",58,4),
            new TokenData("assign","=",58,5),
            new TokenData("intnum","11",58,6),
            new TokenData("semi",";",58,7),
            new TokenData("id", "arr",59,1),
            new TokenData("opensqbr","[",59,2),
            new TokenData("intnum","6",59,3),
            new TokenData("closesqbr","]",59,4),
            new TokenData("assign","=",59,5),
            new TokenData("intnum","90",59,6),
            new TokenData("semi",";",59,7),
            new TokenData("id", "printarray",60,1),
            new TokenData("openpar","(", 60, 2),
            new TokenData("id", "arr",60,3),
            new TokenData("comma",",",60,4),
            new TokenData("intnum","7",60,5),
            new TokenData("closepar",")",60,6),
            new TokenData("semi",";",60,7),
            new TokenData("id", "bubbleSort",61,1),
            new TokenData("openpar","(", 61, 2),
            new TokenData("id", "arr",61,3),
            new TokenData("comma",",",61,4),
            new TokenData("intnum","7",61,5),
            new TokenData("closepar",")",61,6),
            new TokenData("semi",";",61,7),
            new TokenData("id", "printarray",62,1),
            new TokenData("openpar","(", 62, 2),
            new TokenData("id", "arr",62,3),
            new TokenData("comma",",",62,4),
            new TokenData("intnum","7",62,5),
            new TokenData("closepar",")",62,6),
            new TokenData("semi",";",62,7),
            new TokenData("end", "end",63,1)
        };

        [TestMethod]
        public void SingleSymbolInGlobalTableTest()
        {
            var synResult = SyntacticAnalyzer.Parse(tokensToParse);
            var tableResult = SemanticAnalyzer.CreateSymbolTable(synResult.parseTree);

            Assert.IsTrue(tableResult.symbolTable.TableEntries.Count() == 1);
        }

        [TestMethod]
        public void ParseWithSemErrorsTest()
        {
            var synResult = SyntacticAnalyzer.Parse(tokensToParseWithSemError);
            var tableResult = SemanticAnalyzer.CreateSymbolTable(synResult.parseTree);
            var semErrors = SemanticAnalyzer.SemanticAnalysis(synResult.parseTree, tableResult.symbolTable, tableResult.errorList);

            Assert.IsTrue(semErrors.Count == 1);
        }

        [TestMethod]
        public void ParseBubbleSort()
        {
            var synResult = SyntacticAnalyzer.Parse(tokensForBubbleSort);
            var tableResult = SemanticAnalyzer.CreateSymbolTable(synResult.parseTree);
            var semErrors = SemanticAnalyzer.SemanticAnalysis(synResult.parseTree, tableResult.symbolTable, tableResult.errorList);
            var setErrors = new HashSet<string>();

            Assert.IsTrue(tableResult.symbolTable.TableEntries.Count() > 1);
            Assert.IsTrue(semErrors.Count() > 1);

            foreach(var error in semErrors)
            {
                var errorIndex = error.IndexOf("(");
                var errorIndexEnd = error.IndexOf(")");
                setErrors.Add(error.Substring(errorIndex + 1, errorIndexEnd - errorIndex - 1));
            }

            Assert.IsTrue(setErrors.Contains("5,8"));
            Assert.IsTrue(setErrors.Contains("6"));
            Assert.IsTrue(setErrors.Contains("11"));
            Assert.IsTrue(setErrors.Contains("12"));
        }
    }
}
