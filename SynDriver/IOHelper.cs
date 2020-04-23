using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynSemDriver
{
    public class IOHelper
    {
        public static void WriteLine(StreamWriter sw, string toPrint)
        {
            sw.WriteLine(toPrint);
            Console.WriteLine(toPrint);
        }

        public static void Write(StreamWriter sw, string toPrint)
        {
            sw.Write(toPrint);
            Console.Write(toPrint);
        }
    }
}
