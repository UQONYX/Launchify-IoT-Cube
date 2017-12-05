using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace WindowsFormsApp4
{
    public static class PlatformIOProcess
    {
        //the default file path where the PLATFORMIO file is
        private static string path = @"cd C:\Users\Uthma\Documents\CompilerFirstTry";

        //Method to make and execute a new process
        private static void makeTemp(string fileName, string[] commands)
        {
            string processName = System.Environment.GetEnvironmentVariable("TEMP") +
                            @"\" + fileName;
            using (StreamWriter sw = new StreamWriter(processName))
            {
                //take the process from commands array and add them line by line to the batch file
                foreach (string s in commands)
                {
                    sw.WriteLine(s);
                }
            }
            Process.Start(@"cmd.exe ", "/c " + processName);
            Process.Start(@"cmd.exe ", "/c del " + processName);
        }
        public static void BuildCode ()
        {
            string[] run = { path, "platformio run " };
            makeTemp("buildBinary", run);
        }
    }
}
