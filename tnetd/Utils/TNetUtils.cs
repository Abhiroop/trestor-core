using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TNetD
{
    public enum Verbosity { NoDisplay, Errors, Warning, Info, ExtraInfo };

    class TNetUtils
    { 

        public static Hash GenerateNewToken()
        {
            byte[] randBytes = new byte[8];
            Random rand = new Random();
            rand.NextBytes(randBytes);
            return new Hash(randBytes);
        }

        


        public static void WaitForExit()
        {
            bool exit = false;
            do
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.X && cki.Modifiers == ConsoleModifiers.Control)
                {
                    exit = true;
                }
                Thread.Sleep(100);
            }
            while (!exit);
        }
        



    }
}
