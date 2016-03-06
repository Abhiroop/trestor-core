﻿
/*
 *  @Author: Arpan Jati
 *  @Version: 1.0
 *  @Date: Aug 2014
 *  @Description: Varint2
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TNetD.Protocol
{

    /// <summary>
    /// A variable length integer with, the following format:
    /// 
    /// [length in bytes [4 bits], [msb....][........][.......] ..... [.... lsb]
    /// 
    /// </summary>
    public static class Varint2
    {
        public static int GetBitLength(long value)
        {
            int length = 0;
            for (int i = 63; i >= 0; i--)
            {
                if (((value >> i) & 1) == 1)
                {
                    length = (i + 1);
                    break;
                }
            }

            return length;
        }

        /*
        public static byte[] EncodeOld(long value)
        {
            long val = value;

            List<byte> EncodedBytes = new List<byte>();
            int bytes = 0;
            int bitLength = GetBitLength(value);
            int remBitLength = bitLength;
            int shift = 0;
            while (remBitLength > 4)
            {
                EncodedBytes.Add((byte)(val & 0xFF));
                shift = remBitLength >= 8 ? 8 : remBitLength;
                val >>= shift;
                remBitLength -= shift;
                bytes++;
            }

            EncodedBytes.Add((byte)((((byte)(bytes + 1) & 0xF) << 4) | ((byte)val & 0xF)));
            remBitLength -= 4;

            return EncodedBytes.ToArray().Reverse().ToArray();
        }*/

        public static byte[] Encode(long value)
        {
            long val = value;// ; (value << 1) ^ (value >> 63);

            int bitLength = GetBitLength(value);

            int fullDataBytesNeeded = (byte)(bitLength >> 3); // Divide by 8 to get number of bytes
            int extraPackedBits = (byte)(bitLength - (fullDataBytesNeeded << 3));

            if (extraPackedBits <= 4) // MSB's in Length Byte
            {
                int totalBytesNeeded = fullDataBytesNeeded + 1;

                if (totalBytesNeeded > 9) throw new ArithmeticException("Encoding Enception. This should not happen.");

                byte[] EncodedBytes = new byte[totalBytesNeeded];

                EncodedBytes[0] = (byte)((((byte)(totalBytesNeeded) & 0xF) << 4) | ((byte)(val >> (fullDataBytesNeeded << 3) & 0xF)));

                for (int i = 1; i <= fullDataBytesNeeded; i++)
                {
                    EncodedBytes[i] = ((byte)(val >> ((fullDataBytesNeeded - i) << 3) & 0xFF));
                }
                //Console.WriteLine(" ============================================");
                return EncodedBytes;
            }
            else // Extra byte for MSB's*/
            {
                int totalBytesNeeded = fullDataBytesNeeded + 2;

                if (totalBytesNeeded > 9) throw new ArithmeticException("Encoding Enception. This should not happen.");

                byte[] EncodedBytes = new byte[totalBytesNeeded];

                EncodedBytes[0] = (byte)(((byte)(totalBytesNeeded) & 0xF) << 4);

                for (int i = 0; i <= fullDataBytesNeeded; i++)
                {
                    EncodedBytes[i + 1] = ((byte)(val >> ((fullDataBytesNeeded - i) << 3) & 0xFF));
                }
                //Console.WriteLine(" +++++++++++++++++++++++++++++++++++++++++++++");
                return EncodedBytes;
            }
        }

        public static long Decode(byte[] value, int startIndex, out int length)
        {
            if (value.Length - startIndex > 0)
            {
                int bytes = (byte)((value[startIndex] >> 4) & 0xF);
                if (bytes <= value.Length - startIndex)
                {
                    length = bytes;
                    long result = (value[startIndex] & 0xF);
                    for (int i = 1; i < bytes; i++)
                    {
                        result <<= 8;
                        result |= value[i + startIndex];
                    }
                    return result;
                }
                else
                {
                    throw new Exception("Bad varint2 encoding");
                }
            }
            else throw new Exception("Bad varint2 length");
        }

        public static long Decode(byte[] value)
        {
            int outlen = 0;
            return Decode(value, 0, out outlen);
        }

        /*
        public static long Decode(byte[] value)
        {
            if (value.Length > 0)
            {
                int bytes = (byte)((value[0] >> 4) & 0xF);
                if (bytes == value.Length)
                {
                    long result = (value[0] & 0xF);
                    for (int i = 1; i < bytes; i++)
                    {
                        result <<= 8;
                        result |= value[i];
                    }
                    return result;
                }
                else
                {
                    throw new Exception("Bad varint2 encoding");
                }
            }
            else throw new Exception("Bad varint2 length");
        }
        */

        public static void TestVarint2()
        {
            Random rnd = new Random();

            long varint2Bytes = 0;
            long varintBytes = 0;

            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

            watch.Start();

           // Parallel.For(0, 1000000, i => 
            for (int i = 0; i < 10000000; i++)
            {
                // max Tres ~= 100,000,000,000,000,000 
                long Long = (long)(rnd.Next(0, 2147483647)) * (long)(rnd.Next(0, 2147483647));//.NextDouble() * (long.MaxValue));
                //int bitLength = GetBitLength(Long);
                //byte[] enc_old = Encode(Long);
                byte[] enc = Encode(Long);
                //Interlocked.Add(ref varint2Bytes, enc.Length);
                varint2Bytes += enc.Length;

                byte[] enc_norm = VarintBitConverter.GetVarintBytes(Long);
                //Interlocked.Add(ref varintBytes, enc_norm.Length);
                varintBytes += enc_norm.Length;
                
                //Console.Write(Long + " - " + HexUtil.ToString(enc) + " - " + HexUtil.ToString(enc_norm) + "\n");

                long Decoded = Decode(enc);

                //UInt64 Decoded =  VarintBitConverter.ToUInt64(enc_norm);

                if ((Long != Decoded))
                {
                    Debug.Write(Long + " - " + " - " + HexUtil.ToString(enc));
                    Debug.WriteLine(" TEST FAIL ---------------------------");
                    return;
                }

                //Console.Write(Long + " - " + bitLength + " - " + HexUtil.ToString(enc));
                //Console.WriteLine((Long == Decoded) ? " : Match" : " : FAIL ---------------------------");

            }
            //});
                       

            watch.Stop();

            Debug.WriteLine(" TEST FINISHED ---------------------------\n Varint :" + varintBytes + "\n Varint 2: " + varint2Bytes +
                "\n\n Time Taken: " + watch.ElapsedMilliseconds  + " (ms)"      
                );
        }

    }
}


