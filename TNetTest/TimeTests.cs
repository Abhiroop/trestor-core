using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TNetTest
{
    class TimeTests
    {
        public delegate void PrintHandler(string value);
        public event PrintHandler Print;

        public void Execute()
        {
            string file1 = @"NODE_2\voteLog_2.log";
            string file2 = @"NODE_2\resultLog_2.csv";

            TimeTests parse = new TimeTests();
            List<string[]> fileParsed = parse.file(file1);

            Dictionary<string, long> timeMap = new Dictionary<string, long>();
            foreach (var lines in fileParsed)
            {
                if (lines.Length == 7)
                {
                    if (!(lines[0] == "R"))
                    {        //filtering the received ones
                        if (timeMap.ContainsKey(lines[2]))
                        {

                            timeMap[lines[2]] = Convert.ToInt64(lines[3]) - timeMap[lines[2]];

                        }
                        else
                        {
                            timeMap.Add(lines[2], Convert.ToInt64(lines[3]));
                        }
                    }
                }

            }

            List<long> beforeBucketing = new List<long>();
            string result = "";
            foreach (KeyValuePair<string, long> entry in timeMap)
            {
                result += entry.Key + "-" + entry.Value + "\n";
                beforeBucketing.Add(entry.Value);
            }
            String k = "";
            Tuple<int[], float> afterBucket = bucketize(beforeBucketing, 100); //100 buckets
            for (int i = 0; i < afterBucket.Item1.Length; i++)
            {
                k += afterBucket.Item1[i] + ",";
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 100; i++)
            {
                sb.Append(afterBucket.Item2 * i + ",");
            }
            
            //logging
            FileStream logger = default(FileStream);

            TextWriter tr = default(TextWriter);

            logger = new FileStream(file2, FileMode.Create);
            tr = new StreamWriter(logger);
            tr.WriteLine(sb.ToString().TrimEnd());
            tr.WriteLine(k.TrimEnd());
            Print?.Invoke(afterBucket.Item2.ToString());
        }

        private List<string[]> file(string csvFile)
        {
            List<string[]> total = new List<string[]>();
            try
            {
                var lines = File.ReadLines(csvFile);
                foreach (var line in lines)
                {
                    string[] lineRead = line.Split(',');

                    total.Add(lineRead);
                }

            }
            catch (Exception ex)
            {
                Print?.Invoke(ex.Message + "\n" + ex.StackTrace);
            }
            return total;
        }

        private Tuple<int[], float> bucketize(IEnumerable<long> source, int totalBuckets)
        {
            var min = source.Min();
            var max = source.Max();

            var bucketSize = (max - min) / totalBuckets;
            int[] buckets = new int[totalBuckets];
            foreach (var value in source)
            {
                int bucketIndex = 0;
                if (bucketSize > 0.0)
                {
                    bucketIndex = (int)((value - min) / bucketSize);
                    if (bucketIndex == totalBuckets)
                    {
                        bucketIndex--;
                    }
                }
                buckets[bucketIndex]++;
            }

            var bucketSizeF = ((float)max - (float)min) / (float)totalBuckets;

            Tuple<int[], float> result = new Tuple<int[], float>(buckets, bucketSizeF);
            return result;
        }
    }
}
