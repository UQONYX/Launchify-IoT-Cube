using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string batTest = System.Environment.GetEnvironmentVariable("TEMP") +
                            @"\batchfile.bat";
            using (StreamWriter sw = new StreamWriter(batTest))
            {

                //  sw.WriteLine("echo Batch program has started...");
                //sw.WriteLine("REM platformio");
                sw.WriteLine(@"cd C:\Users\Uthma\Documents\CompilerFirstTry");
                sw.WriteLine("platformio run");
                sw.WriteLine("pause");

                sw.WriteLine("exit");
            }
            Process.Start(@"cmd.exe ", "/c " + "compile.bat");
            //Process.Start(@"cmd.exe ", "/c del " + batTest);
        }
    }
}
