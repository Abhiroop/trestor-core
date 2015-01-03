/*
  @Author: Arpan Jati
  @Version: 1.3
  @Description: Originally used in DirectShare
  @ TODO: IMPROVE, by, performing equality and comparison in one step.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD
{
    [Serializable]
    public class Hash : IEquatable<Hash>, IComparer<Hash>, IComparable<Hash>
    {
        private byte[] HashValue;

        public Hash(byte[] Value)
        {
            HashValue = (byte[])Value.Clone();
        }

        public Hash()
        {
            HashValue = new byte[0];
        }

        public byte[] Hex
        {
            get { return HashValue; }
            //set { HashValue = value; }
        }

        public byte GetNibble(int NibbleIndex)
        {
            int Address = NibbleIndex >> 1;
            int IsLow = NibbleIndex & 1;
            return (byte)((IsLow == 0) ? ((HashValue[Address] >> 4) & 0xF) : (HashValue[Address] & 0xF));
        }

        /// <summary>
        /// Returns if the compared value is equal to the Hash value.
        /// </summary>
        /// <param name="obj">The value to be compared.</param>
        /// <returns></returns>
        public bool Equals(Hash obj)
        {
            if (obj == null) return false;

            if (HashValue.Length == obj.HashValue.Length)
            {
                bool equal = true;
                for (int i = 0; i < HashValue.Length; i++) if (HashValue[i] != obj.HashValue[i]) { equal = false; break; }
                return equal;
            }
            return false;
        }

        /// <summary>
        /// Returns if the compared value is equal to the Hash value.
        /// </summary>
        /// <param name="obj">The value to be compared.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Hash);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < HashValue.Length; i++)
            {
                sb.Append(HashValue[i].ToString("X2") + "");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Well, looks ok, should work !! [Punisher]
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int len = HashValue.Length;
            int hc = 0x12345678;
            for (int i = 0; i < len; i += 4)
            {
                hc ^= (HashValue[i] << 0) + (HashValue[i + 1] << 8) + (HashValue[i + 2] << 16) + (HashValue[i + 3] << 24);
            }
            return hc;
        }

        public static bool operator ==(Hash a, Hash b)
        {
            if (Object.ReferenceEquals(a, b))
                return true;

            if (Object.ReferenceEquals(a, null))
                return false;

            return a.Equals((object)b);
        }
                
        // Simple comparer from http://stackoverflow.com/questions/10658709/linq-orderbybyte-values
        // Too simple to write one :)
        public int Compare(byte[] x, byte[] y)
        {
            // Shortcuts: If both are null, they are the same.
            if (x == null && y == null) return 0;

            // If one is null and the other isn't, then the
            // one that is null is "lesser".
            if (x == null && y != null) return -1;
            if (x != null && y == null) return 1;

            // Both arrays are non-null.  Find the shorter
            // of the two lengths.
            int bytesToCompare = Math.Min(x.Length, y.Length);

            // Compare the bytes.
            for (int index = 0; index < bytesToCompare; ++index)
            {
                // The x and y bytes.
                byte xByte = x[index];
                byte yByte = y[index];

                // Compare result.
                int compareResult = Comparer<byte>.Default.Compare(xByte, yByte);

                // If not the same, then return the result of the
                // comparison of the bytes, as they were the same
                // up until now.
                if (compareResult != 0) return compareResult;

                // They are the same, continue.
            }

            // The first n bytes are the same.  Compare lengths.
            // If the lengths are the same, the arrays
            // are the same.
            if (x.Length == y.Length) return 0;

            // Compare lengths.
            return x.Length < y.Length ? -1 : 1;
        }

        public int CompareTo(Hash other)
        {
            if (Object.ReferenceEquals(this, other))
                return 0;

            if (other == null)
            {
                throw new ArgumentException("Argument is NULL");
            }

            return Compare(HashValue, other.Hex);
        }

        public int Compare(Hash x, Hash y)
        {
            if (Object.ReferenceEquals(x, y))
                return 0;

            if (x == null || y == null)
            {
                throw new ArgumentException("Arguments NULL");
            }

            return Compare(x.Hex, y.Hex);
        }

        public static bool operator !=(Hash a, Hash b)
        {
            return !(a == b);
        }             

    }
}
