using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynSemDriver
{
    public class SymbolValue
    {
        public string Name { get; set; }
        public string Kind { get; set; }
        public string Type { get; set; }
        public string ArrayType { get; set; }
        public int Offset { get; set; }
        public int ScopedOffset { get; set; }
        public SymbolTable Link { get; set; }

        public SymbolValue(string name = "", string kind = "", string type = "", string arrayType = "", int offset = 0, int scopedOffset = 0, SymbolTable link = null)
        {
            Name = name;
            Kind = kind;
            Type = type;
            ArrayType = arrayType;
            Offset = offset;
            ScopedOffset = scopedOffset;
            Link = link;
        }

        public override string ToString()
        {
            var linkName = Link?.Name ?? "";

            return "Name = " + Name +
                "; Kind = " + Kind +
                "; Type = "+ Type +
                "; Offset = " + Offset +
                "; ScopedOffset = " + ScopedOffset +
                "; Link = " + linkName;
        }
    }
}
