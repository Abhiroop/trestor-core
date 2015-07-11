
// Author: Arpan Jati
// Version: 1.0
// Date: July 2015 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD
{
    public class HashPair : IEquatable<HashPair>, IComparer<HashPair>, IComparable<HashPair>
    {
        private byte[] H1;
        private byte[] H2;

        private byte[] HashValue;

        public HashPair(Hash H1, Hash H2)
        {
            this.H1 = (byte[])H1.Hex;
            this.H2 = (byte[])H2.Hex;

            if (Compare(H1.Hex, H2.Hex) < 0)
            {
                this.HashValue = H1.Hex.Concat(H2.Hex).ToArray();
            }
            else
            {
                this.HashValue = H2.Hex.Concat(H1.Hex).ToArray();
            }
        }

        public HashPair()
        {
            H1 = new byte[0];
            H2 = new byte[0];
            this.HashValue = H1.Concat(H2).ToArray();
        }

        public byte[] HexH1
        {
            get { return H1; }
        }

        public byte[] HexH2
        {
            get { return H2; }
        }

        public byte[] JointHash
        {
            get { return HashValue; }
        }

        /// <summary>
        /// Returns if the compared value is equal to the Hash value.
        /// </summary>
        /// <param name="obj">The value to be compared.</param>
        /// <returns></returns>
        public bool Equals(HashPair obj)
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
            return this.Equals(obj as HashPair);
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
        /// Certainly, can be improved.
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

        public static bool operator ==(HashPair a, HashPair b)
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

        public int CompareTo(HashPair other)
        {
            if (Object.ReferenceEquals(this, other))
                return 0;

            if (other == null)
            {
                throw new ArgumentException("Argument is NULL");
            }

            return Compare(HashValue, other.HashValue);
        }

        public int Compare(HashPair x, HashPair y)
        {
            if (Object.ReferenceEquals(x, y))
                return 0;

            if (x == null || y == null)
            {
                throw new ArgumentException("Arguments NULL");
            }

            return Compare(x.HashValue, y.HashValue);
        }

        public static bool operator !=(HashPair a, HashPair b)
        {
            return !(a == b);
        }

    }
}
