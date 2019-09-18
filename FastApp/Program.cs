using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FastApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var str = "\n\r(1 row affected)\n\r";
            Console.WriteLine(Regex.IsMatch(str, "[(][0-9]+ row[s]* affected[)]", RegexOptions.Multiline));
            Console.ReadKey();
        }
    }
}
