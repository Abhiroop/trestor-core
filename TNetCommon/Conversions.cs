
// @Author: Arpan Jati
// @Date: Dec 2014

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD
{
    public class Conversions
    {
        public static byte[] Int64ToVector(long data)
        {
            byte[] _out = new byte[8];

            for (int i = 0; i < 8; i++)
            {
                _out[i] = (byte)(((data >> (8 * i)) & 0xFF));
            }

            return _out;
        }

        public static byte[] Int16ToVector(short data)
        {
            byte[] _out = new byte[2];

            for (int i = 0; i < 2; i++)
            {
                _out[i] = (byte)(((data >> (8 * i)) & 0xFF));
            }

            return _out;
        }

        public static byte[] Int32ToVector(int data)
        {
            byte[] _out = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                _out[i] = (byte)(((data >> (8 * i)) & 0xFF));
            }

            return _out;
        }

        public static byte[] FloatToVector(float data)
        {
            return BitConverter.GetBytes(data);
        }

        public static byte[] DoubleToVector(double data)
        {
            return BitConverter.GetBytes(data);
        }


        public static long VectorToInt64(byte[] data)
        {
            long result;
            result = (long)data[0];
            result |= ((long)data[1]) << 8;
            result |= ((long)data[2]) << 16;
            result |= ((long)data[3]) << 24;
            result |= ((long)data[4]) << 32;
            result |= ((long)data[5]) << 40;
            result |= ((long)data[6]) << 48;
            result |= ((long)data[7]) << 56;
            return result;
        }

        public static short VectorToInt16(byte[] data)
        {
            short result;
            result = (short)data[0];
            result |= (short)(((short)data[1]) << 8);
            return result;
        }

        public static int VectorToInt32(byte[] data)
        {
            int result;
            result = (int)data[0];
            result |= ((int)data[1]) << 8;
            result |= ((int)data[2]) << 16;
            result |= ((int)data[3]) << 24;
            return result;
        }

        public static float VectorToFloat(byte[] data)
        {
            return BitConverter.ToSingle(data, 0);
        }

        public static double VectorToDouble(byte[] data)
        {
            return BitConverter.ToDouble(data, 0);
        }
    }
}
