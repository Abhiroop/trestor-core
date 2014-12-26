using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD
{
    public static class Utils
    {
        public static DateTime FromUnixTime(this long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        public static long ToUnixTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }

        public static bool ByteArrayEquals(byte[] x, int xOffset, byte[] y, int yOffset, int length)
        {
            if (x == null) new Exception("x is NULL");
            if (y == null) new Exception("y is NULL");

            if (length < 0) new Exception("Invalid Length");
            if (xOffset < 0 || x.Length - xOffset < length) new Exception("Invalid xOffset");
            if (yOffset < 0 || y.Length - yOffset < length) new Exception("Invalid yOffset");

            int DiffBytes = 0;

            for (int i = 0; i < length; i++ )
            {
                DiffBytes += (x[i + xOffset] != y[i + yOffset]) ? 1 : 0;
            }

            return (DiffBytes == 0);
        }

        public static Encoding Encoding88591 = Encoding.GetEncoding(28591);

       


    }
}
