/*
*
 @Author: Arpan Jati
 @Version: 1.2
 @Description: Originally used in DirectShare
* @ TODO: IMPROVE, by, performing equality and comparison in one step.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD
{
    [Serializable]
    public class Hash : IEquatable<Hash>, IComparable<Hash>
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
            set { HashValue = value; }
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
        /// Well, looks ok, should work, : Punisher
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

        public int CompareTo(Hash other)
        {
            if (Object.ReferenceEquals(this, other))
                return 0;

            if (other == null)
            {
                throw new ArgumentException("Argument is NULL");
            }

            if (this.Equals(other))
                return 0;

            if (this < other)
                return -1;
            else
                return 1;
        }

        public int Compare(Hash x, Hash y)
        {
            if (Object.ReferenceEquals(x, y))
                return 0;

            if (x == null || y == null)
            {
                throw new ArgumentException("Arguments NULL");
            }

            return 0;
        }

        public static bool operator <(Hash a, Hash b)
        {
            if (a.Hex.Length == b.Hex.Length)
            {
                for (int i = 0; i < a.Hex.Length; i++)
                {
                    if (a.Hex[i] < b.Hex[i])
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        public static bool operator >(Hash a, Hash b)
        {
            if (a.Hex.Length == b.Hex.Length)
            {
                for (int i = 0; i < a.Hex.Length; i++)
                {
                    if (a.Hex[i] > b.Hex[i])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool operator !=(Hash a, Hash b)
        {
            return !(a == b);
        }

    }
}
