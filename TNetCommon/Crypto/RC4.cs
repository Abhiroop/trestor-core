// @Author: Arpan Jati

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TNetD.Crypto
{
    /// <summary>
    /// The RC4 Stream Cipher
    /// Well, the cipher is outdated / many weaknesses found.
    /// DO NOT USE.
    /// </summary>
    public class RC4
    {
        public Byte[] RC4_Process(Byte[] data, Byte[] key)
        {
            Byte[] s = new Byte[256];
            Byte[] k = new Byte[256];
            Byte temp;
            int i, j;

            for (i = 0; i < 256; i++)
            {
                s[i] = (Byte)i;
                k[i] = key[i % key.GetLength(0)];
            }

            j = 0;
            for (i = 0; i < 256; i++)
            {
                j = (j + s[i] + k[i]) % 256;
                temp = s[i];
                s[i] = s[j];
                s[j] = temp;
            }

            byte[] OUT = new byte[data.Length];

            i = j = 0;

            unchecked
            {
                for (int x = 0; x < data.GetLength(0); x++)
                {
                    i = (i + 1) % 256;
                    j = (j + s[i]) % 256;
                    temp = s[i];
                    s[i] = s[j];
                    s[j] = temp;
                    int t = (s[i] + s[j]) % 256;
                    OUT[x] = (byte)(data[x] ^ s[t]);
                }
            }

            return OUT;

        }

    }
}
