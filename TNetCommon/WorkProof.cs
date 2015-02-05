
// File: WorkProof.cs 
// Version: 1.0 
// Date: Jan 7, 2013
// Author : Arpan Jati

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TNetD
{
    public enum ProofOfWorkType { DOUBLE_SHA256, SCRYPT, BCRYPT, RIG, RIG2 };

    public static class WorkProof
    {
        // Specifies the number of zeros in increasing order

        static byte[] bitMask = new byte[] { 0xFF, 0x7F, 0x3F, 0x1F, 0x0F, 0x07, 0x03, 0x01 }; 
               
        /// <summary>
        /// Calculates a Work Proof based on a given Difficulty
        /// </summary>
        /// <param name="Initial"></param>
        /// <param name="Difficulty"></param>
        /// <returns></returns>
        public static byte[] CalculateProof(byte[] Initial, int Difficulty)
        {
            bool Found = false;
            int InitialLength = Initial.Length;

            int zeroBytes = Difficulty / 8;
            int zeroBits = Difficulty % 8;

            byte[] Content = new byte[InitialLength + 4];

            Array.Copy(Initial, Content, InitialLength);

            int counter = 0;

            if (zeroBytes < 30)
            {
                while (!Found)
                {
                    byte[] CountBytes = Utils.GetLengthAsBytes(counter);  // Get bytes from counter | Changed Little Endian

                    Array.Copy(CountBytes, 0, Content, InitialLength, 4); // Copy to the end.

                    byte[] Hash = (new SHA256Managed()).ComputeHash((new SHA256Managed()).ComputeHash(Content)); // Calculate Double Hash

                    bool BytesGood = true;

                    for (int i = 0; i < zeroBytes; i++)
                    {
                        if (Hash[i] != 0)
                        {
                            BytesGood = false;
                            break;
                        }
                    }

                    if (BytesGood)
                    {
                        if ((bitMask[zeroBits] | Hash[zeroBytes]) == bitMask[zeroBits])
                        {
                            Found = true;
                            return BitConverter.GetBytes(counter);
                        }
                    }

                    counter++;
                }
            }

            throw new Exception("Proof Calculation Failure");
        }

        public static bool VerifyProof(byte[] Initial, byte[] Proof, int Difficulty)
        {
            if (Proof.Length == 4)
            {
                int InitialLength = Initial.Length;
                byte[] Content = new byte[InitialLength + 4];
                Array.Copy(Initial, Content, InitialLength);
                Array.Copy(Proof, 0, Content, InitialLength, 4);
                return VerifyProof(Content, Difficulty);
            }
            return false;            
        }

        public static bool VerifyProof(byte[] Content, int Difficulty)
        {
            int zeroBytes = Difficulty / 8;
            int zeroBits = Difficulty % 8;

            byte[] Hash = (new SHA256Managed()).ComputeHash((new SHA256Managed()).ComputeHash(Content)); // Calculate Double Hash

            bool BytesGood = true;

            for (int i = 0; i < zeroBytes; i++)
            {
                if (Hash[i] != 0)
                {
                    BytesGood = false;
                    break;
                }
            }

            if (BytesGood)
            {
                if ((bitMask[zeroBits] | Hash[zeroBytes]) == bitMask[zeroBits])
                {
                    return true;
                }
            }

            return false;
        }



    }
}
