//
// From Original c source, at cr.yp.to
// File: Salsa20.cs 
// Version: 1.0 
// Date: Jan 9 2013
// Author : Arpan Jati

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TNetD.Crypto
{
    public class Salsa20
    {
        private static ulong ROTATE_4L(ulong x)
        {
            return (x >> 60) | (x << 4);
        }

        private static UInt32 ROTATE(UInt32 x, byte r)
        {
            return (x << r) | (x >> (32 - r));
        }

        private static UInt32 XOR(UInt32 x, UInt32 y)
        {
            return (x ^ y);
        }

        private static UInt32 PLUS(UInt32 x, UInt32 y)
        {
            return ((x + y) & 0xFFFFFFFFU);
        }

        private static UInt32 PLUSONE(UInt32 x)
        {
            return PLUS(x, 1);
        }

        public Salsa20()
        {

        }

        static void salsa20_wordtobyte(byte[] output, UInt32[] input)
        {
            UInt32[] x = new UInt32[16];
            int i;

            for (i = 0; i < 16; ++i) x[i] = input[i];
            for (i = 20; i > 0; i -= 2)
            {
                x[4] = XOR(x[4], ROTATE(PLUS(x[0], x[12]), 7));
                x[8] = XOR(x[8], ROTATE(PLUS(x[4], x[0]), 9));
                x[12] = XOR(x[12], ROTATE(PLUS(x[8], x[4]), 13));
                x[0] = XOR(x[0], ROTATE(PLUS(x[12], x[8]), 18));
                x[9] = XOR(x[9], ROTATE(PLUS(x[5], x[1]), 7));
                x[13] = XOR(x[13], ROTATE(PLUS(x[9], x[5]), 9));
                x[1] = XOR(x[1], ROTATE(PLUS(x[13], x[9]), 13));
                x[5] = XOR(x[5], ROTATE(PLUS(x[1], x[13]), 18));
                x[14] = XOR(x[14], ROTATE(PLUS(x[10], x[6]), 7));
                x[2] = XOR(x[2], ROTATE(PLUS(x[14], x[10]), 9));
                x[6] = XOR(x[6], ROTATE(PLUS(x[2], x[14]), 13));
                x[10] = XOR(x[10], ROTATE(PLUS(x[6], x[2]), 18));
                x[3] = XOR(x[3], ROTATE(PLUS(x[15], x[11]), 7));
                x[7] = XOR(x[7], ROTATE(PLUS(x[3], x[15]), 9));
                x[11] = XOR(x[11], ROTATE(PLUS(x[7], x[3]), 13));
                x[15] = XOR(x[15], ROTATE(PLUS(x[11], x[7]), 18));
                x[1] = XOR(x[1], ROTATE(PLUS(x[0], x[3]), 7));
                x[2] = XOR(x[2], ROTATE(PLUS(x[1], x[0]), 9));
                x[3] = XOR(x[3], ROTATE(PLUS(x[2], x[1]), 13));
                x[0] = XOR(x[0], ROTATE(PLUS(x[3], x[2]), 18));
                x[6] = XOR(x[6], ROTATE(PLUS(x[5], x[4]), 7));
                x[7] = XOR(x[7], ROTATE(PLUS(x[6], x[5]), 9));
                x[4] = XOR(x[4], ROTATE(PLUS(x[7], x[6]), 13));
                x[5] = XOR(x[5], ROTATE(PLUS(x[4], x[7]), 18));
                x[11] = XOR(x[11], ROTATE(PLUS(x[10], x[9]), 7));
                x[8] = XOR(x[8], ROTATE(PLUS(x[11], x[10]), 9));
                x[9] = XOR(x[9], ROTATE(PLUS(x[8], x[11]), 13));
                x[10] = XOR(x[10], ROTATE(PLUS(x[9], x[8]), 18));
                x[12] = XOR(x[12], ROTATE(PLUS(x[15], x[14]), 7));
                x[13] = XOR(x[13], ROTATE(PLUS(x[12], x[15]), 9));
                x[14] = XOR(x[14], ROTATE(PLUS(x[13], x[12]), 13));
                x[15] = XOR(x[15], ROTATE(PLUS(x[14], x[13]), 18));
            }
            for (i = 0; i < 16; ++i) x[i] = PLUS(x[i], input[i]);
            for (i = 0; i < 16; ++i)
            {
                //U32TO8_LITTLE(output + 4 * i, x[i]);
                byte[] BYTES = BitConverter.GetBytes(x[i]);
                int start = 4 * i;
                for (int c = 0; c < 4; c++)
                {
                    output[start + c] = BYTES[c];
                }
            }
        }

        public class ECRYPT_ctx
        {
            public uint[] input = new uint[16];
            public ulong[] Key = new ulong[4];
        }

        static byte[] sigma = Encoding.UTF8.GetBytes("expand 32-byte k");
        static byte[] tau = Encoding.UTF8.GetBytes("expand 16-byte k");

        ECRYPT_ctx x = new ECRYPT_ctx();

        public void Key_Setup(byte[] k, uint kbits)
        {
            byte[] constants;

            x.input[1] = BitConverter.ToUInt32(k, 0);
            x.input[2] = BitConverter.ToUInt32(k, 4);
            x.input[3] = BitConverter.ToUInt32(k, 8);
            x.input[4] = BitConverter.ToUInt32(k, 12);

            int extra = 0;

            if (kbits == 256)
            { /* recommended */
                extra = 16;
                constants = sigma;
            }
            else
            { /* kbits == 128 */
                constants = tau;
            }
            x.input[11] = BitConverter.ToUInt32(k, extra + 0);
            x.input[12] = BitConverter.ToUInt32(k, extra + 4);
            x.input[13] = BitConverter.ToUInt32(k, extra + 8);
            x.input[14] = BitConverter.ToUInt32(k, extra + 12);

            x.input[0] = BitConverter.ToUInt32(constants, 0);
            x.input[5] = BitConverter.ToUInt32(constants, 4);
            x.input[10] = BitConverter.ToUInt32(constants, 8);
            x.input[15] = BitConverter.ToUInt32(constants, 12);
        }

        public void IV_Setup(byte[] iv)
        {
            x.input[6] = BitConverter.ToUInt32(iv, 0);
            x.input[7] = BitConverter.ToUInt32(iv, 4);
            //x.input[8] = 0;
            //x.input[9] = 0;
        }

        public void Counter_Setup(ulong counter)
        {
            x.input[8] = (uint)((counter >> 32));
            x.input[9] = (uint)(counter & 0xFFFFFFFF);
        }

        public void Encrypt_bytes(byte[] m, ref byte[] c, uint bytes)
        {
            byte[] output = new byte[64];
            int i;
            int pos = 0;
            if (bytes == 0) return;
            for (; ; )
            {
                salsa20_wordtobyte(output, x.input);
                x.input[8] = PLUSONE(x.input[8]);
                if (x.input[8] == 0)
                {
                    x.input[9] = PLUSONE(x.input[9]);
                    /* stopping at 2^70 bytes per nonce is user's responsibility */
                }
                if (bytes <= 64)
                {
                    for (i = 0; i < bytes; ++i) c[i + pos] = (byte)(m[i + pos] ^ output[i]);
                    return;
                }
                for (i = 0; i < 64; ++i) c[i + pos] = (byte)(m[i + pos] ^ output[i]);
                bytes -= 64;
                pos += 64;
            }
        }

        public void Cipher_keystream_bytes(ref byte[] stream, uint bytes)
        {
            for (uint i = 0; i < bytes; ++i)
            {
                stream[i] = 0;
            }

            Encrypt_bytes(stream, ref stream, bytes);
        }

        /// <summary>
        /// LAZY, LAZY, many fixes needed !!!
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="KEY"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        public static byte[] ProcessSalsa20(byte[] Data, byte[] KEY, byte[] IV, ulong counter)
        {
            Salsa20 sc = new Salsa20();
            sc.Key_Setup(KEY, 256);
            sc.IV_Setup(IV);
            sc.Counter_Setup(counter);
            byte[] Result = new byte[Data.Length];
            sc.Encrypt_bytes(Data, ref Result, (uint)Data.Length);
            return Result;
        }
    }
}

