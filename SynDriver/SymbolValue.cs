using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynDriver
{
    public class SymbolValue
    {
        public string Name { get; set; }
        public string Kind { get; set; }
        public string Type { get; set; }
        public SymbolTable Link { get; set; }

        public SymbolValue(string name = "", string kind = "", string type = "", SymbolTable link = null)
        {
            Name = name;
            Kind = kind;
            Type = type;
            Link = link;
        }

        public override string ToString()
        {
            var linkName = Link?.Name ?? "";

            return "Name = " + Name +
                "; Kind = " + Kind +
                "; Type = "+ Type +
                "; Link = "+ linkName;
        }
    }
}
