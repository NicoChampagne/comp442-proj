using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexDriver
{
    public class TokenData
    {
        public string Type { get; private set; }
        public string Lexeme { get; set; }
        public int Line { get; set; }
        public int Index { get; set; }

        public static Dictionary<string, string> ReservedWordDictionary { get; } =
            new Dictionary<string, string>
            {
                {"==", "eq"},
                {"<>", "noteq"},
                {"<", "lt"},
                {">", "gt"},
                {"<=", "leq" },
                {">=", "geq"},
                {"+", "plus"},
                {"-", "minus"},
                {"*", "mult"},
                {"/", "div"},
                {"=", "assign"},
                {"(", "openpar"},
                {")", "closepar"},
                {"{", "opencbr"},
                {"}", "closecbr"},
                {"[", "opensqbr"},
                {"]", "closesqbr"},
                {";", "semi"},
                {",", "comma"},
                {".", "dot"},
                {":", "colon"},
                {"::", "coloncolon"},
                {"if", "if"},
                {"then", "then"},
                {"else", "else"},
                {"while", "while"},
                {"class", "class"},
                {"integer", "integer"},
                {"float", "float"},
                {"do", "do"},
                {"end", "end"},
                {"public", "public"},
                {"private", "private"},
                {"or", "or"},
                {"and", "and"},
                {"not", "not"},
                {"read", "read"},
                {"write", "write"},
                {"void", "void"},
                {"return", "return"},
                {"main", "main"},
                {"inherits", "inherits"},
                {"local", "local"},
                {"/*", "inlinecmt"},
                {"*/", "inlinecmt"},
                {"//", "blockcmt"},
            };

        public TokenData(string type, string lexeme, int line, int index)
        {
            Type = type;
            Lexeme = lexeme;
            Line = line;
            Index = index;
        }

        public override string ToString()
        {
            return "[" + Type + "," + Lexeme + "," + Line + "," + Index + "] ";
        }

        public string ToErrorString()
        {
            return "Lexical error: " + Type + ": \"" + Lexeme + "\": line(" + Line + "," + Index + ").";
        }
    }
}