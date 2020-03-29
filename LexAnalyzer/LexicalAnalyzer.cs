using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LexDriver
{
    public class LexicalAnalyzer
    {
        public static TokenData Analyze(string wordToParse, int lineNumber, int indexNumber)
        {
            if (TokenData.ReservedWordDictionary.ContainsKey(wordToParse))
            {
                return new TokenData(TokenData.ReservedWordDictionary[wordToParse], wordToParse, lineNumber, indexNumber);
            }
            else if (Regex.IsMatch(wordToParse, @"^([a-z]|[A-Z])"))
            {
                if (Regex.IsMatch(wordToParse, @"^([a-z]|[A-Z])([a-z]|[A-Z]|[0-9]|_)*$"))
                {
                    return new TokenData("id", wordToParse, lineNumber, indexNumber);
                }
                else
                {
                    return new TokenData("Invalid identifer", wordToParse, lineNumber, indexNumber);
                }
            }
            else if (Regex.IsMatch(wordToParse, @"^[0-9]"))
            {
                if (wordToParse.Equals("0"))
                {
                    return new TokenData("intnum", wordToParse, lineNumber, indexNumber);
                }
                else if (Regex.IsMatch(wordToParse, @"^[1-9][0-9]*$"))
                {
                    return new TokenData("intnum", wordToParse, lineNumber, indexNumber);
                }
                else if (Regex.IsMatch(wordToParse, @"^([1-9][0-9]*|0)(\.([0-9]*[1-9]))?e([+|-])?(0|[1-9][0-9]*)$"))
                {
                    return new TokenData("floatnum", wordToParse, lineNumber, indexNumber);
                }
                else if (Regex.IsMatch(wordToParse, @"^([1-9][0-9]*|0)\.([0-9]*[1-9])$"))
                {
                    return new TokenData("floatnum", wordToParse, lineNumber, indexNumber);
                }
                else if (Regex.IsMatch(wordToParse, @"^([1-9][0-9]*|0)\.([0-9])$"))
                {
                    return new TokenData("floatnum", wordToParse, lineNumber, indexNumber);
                }
                else
                {
                    return new TokenData("Invalid number", wordToParse, lineNumber, indexNumber);
                }
            }

            return new TokenData("Invalid character", wordToParse, lineNumber, indexNumber);
        }
    }
}
