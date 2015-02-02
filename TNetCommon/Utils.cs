using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TNetD
{
    public static class Utils
    {
        public static Encoding Encoding88591 = Encoding.GetEncoding(28591);

        public readonly static string Allowed = "abcdefghijklmnopqrstuvwxyz0123456789_";

        public static bool ValidateUserName(string userName)
        {
            if (userName != userName.ToLowerInvariant()) return false;

            foreach (char x in userName)
            {
                bool exists = false;
                
                foreach(char a in Allowed)
                {
                    if (x == a) { exists = true; break; }
                }

                if (!exists) return false;
            }
            return true;
        }

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

            for (int i = 0; i < length; i++)
            {
                DiffBytes += (x[i + xOffset] != y[i + yOffset]) ? 1 : 0;
            }

            return (DiffBytes == 0);
        }

        /// <summary>
        /// Returns true if the two byte arrays are equal.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool ByteArrayEquals(this byte[] x, byte[] y)
        {
            return ByteArrayEquals(x, 0, y, 0, x.Length);
        }
        
        /// <summary>
        /// Little Endian Byte Order
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] GetLengthAsBytes(int length)
        {
            byte[] len = new byte[4];

            len[0] = (byte)((length >> 0) & 0xFF);
            len[1] = (byte)((length >> 8) & 0xFF);
            len[2] = (byte)((length >> 16) & 0xFF);
            len[3] = (byte)((length >> 24) & 0xFF);

            return len;
        }

        public static byte[] GetLengthAsBytesLong(long length)
        {
            byte[] len = new byte[8];

            len[0] = (byte)((length >> 0) & 0xFF);
            len[1] = (byte)((length >> 8) & 0xFF);
            len[2] = (byte)((length >> 16) & 0xFF);
            len[3] = (byte)((length >> 24) & 0xFF);
            len[4] = (byte)((length >> 32) & 0xFF);
            len[5] = (byte)((length >> 40) & 0xFF);
            len[6] = (byte)((length >> 48) & 0xFF);
            len[7] = (byte)((length >> 56) & 0xFF);

            return len;
        }

        public static long GetLengthFromBytes(byte[] data)
        {
            if (data.Length == 4)
            {
                return ((long)data[0] << 0) + ((long)data[1] << 8) + ((long)data[2] << 16) + ((long)data[3] << 24);
            }
            else if (data.Length == 8)
            {
                long val = ((long)data[0] << 0) + ((long)data[1] << 8) + ((long)data[2] << 16) + ((long)data[3] << 24)
                    + ((long)data[4] << 32) + ((long)data[5] << 40) + ((long)data[6] << 48) + ((long)data[7] << 56);
                return val;
            }
            else
            {
                throw new Exception("Invalid byte array length for packet size calculation.");
            }
        }

        

        public static bool CompareByteArrays(this byte[] H1, byte[] H2)
        {
            if (H1.Length == H2.Length)
            {
                for (int i = 0; i < H1.Length; i++)
                {
                    if (H1[i] != H2[i])
                    {
                        return false;
                    }
                }
            }
            else return false;

            return true;
        }
        
        public static string GenerateUniqueGUID()
        {
            byte[] GUID = new byte[32];
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            rngCsp.GetBytes(GUID);
            return HexUtil.ToString(GUID);
        }

        public static string GenerateUniqueGUID(int lengthInBytes)
        {
            byte[] GUID = new byte[lengthInBytes];
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            rngCsp.GetBytes(GUID);
            return HexUtil.ToString(GUID);
        }

        public static byte[] GenerateUniqueGUID_Bytes()
        {
            byte[] GUID = new byte[32];
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            rngCsp.GetBytes(GUID);
            return GUID;
        }

        public static byte[] GenerateUniqueGUID_Bytes(int lengthInBytes)
        {
            byte[] GUID = new byte[lengthInBytes];
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            rngCsp.GetBytes(GUID);
            return GUID;
        }


        /// <summary>
        /// Generates nRandom Numbers
        /// </summary>
        /// <param name="maxNumber">Exclusive upperbound, lowerbound = 0</param>
        /// <param name="Count">Number of elements to be generated</param>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int[] GenerateNonRepeatingDistribution(int maxNumber, int Count, int notEqualTo=-1)
        {
            if (maxNumber < Count) throw new Exception("maxNumber < Count");

            List<byte> data = new List<byte>();

            HashSet<int> ints = new HashSet<int>();

            while (ints.Count < Count)
            {
                int Rand = Common.random.Next(0, maxNumber);

                if (!ints.Contains(Rand) && (notEqualTo != Rand))
                {
                    ints.Add(Rand);
                }
            }

            return ints.ToArray();
        }


    }
}
