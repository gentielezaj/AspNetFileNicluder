using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.Util
{
    public static class TestAppOutput
    {
        public static void ConsoleWriteException(Exception e, params string[] messages)
        {
            foreach (var message in messages)
            {
                System.Console.WriteLine("EX: " + message);
            }
        }

        public static void ConsoleWriteLine(params string[] messages)
        {
            foreach (var message in messages)
            {
                System.Console.WriteLine(message);
            }
        }
    }
}
