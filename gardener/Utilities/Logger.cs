using System;
using System.Collections.Generic;
using System.Text;

namespace gardener.Utilities
{
    public static class Logger
    {
        public static void Log(this string s)
        {
            Console.WriteLine("[" + DateTime.Now.ToString("h:mm:ss tt") + "]: " + s);
        }

        public static void Log(this Exception e, string text)
        {
            if(e != null)
            Console.WriteLine("[" + DateTime.Now.ToString("h:mm:ss tt") + "]: Exception at " + e.StackTrace + e.Message + " " + text);
        }
    }
}
