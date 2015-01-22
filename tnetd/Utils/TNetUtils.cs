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
        /// <summary>
        /// Generates nRandom Numbers
        /// </summary>
        /// <param name="maxNumber">Exclusive upperbound, lowerbound = 0</param>
        /// <param name="Count">Number of elements to be generated</param>
        /// <returns></returns>
        public static int[] GenerateNonRepeatingDistribution(int maxNumber, int Count, int self)
        {
            if (maxNumber < Count) throw new Exception("maxNumber < Count");

            List<byte> data = new List<byte>();

            HashSet<int> ints = new HashSet<int>();

            while (ints.Count < Count)
            {
                int Rand = Common.random.Next(0, maxNumber);

                if (!ints.Contains(Rand) && (self != Rand))
                {
                    ints.Add(Rand);
                }
            }

            return ints.ToArray();
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
