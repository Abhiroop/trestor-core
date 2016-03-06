
// @ Author : Arpan Jati
// @ Date : 14th January 2015

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TNetD.Address
{
    // GENESIS ACCOUNT FORMAT (.gen_secret) 
    // RANDOMPRIVATE | PUBLIC | NAME | ADDRESS | DESCRIPTION 

    // GENESIS ACCOUNT PUBLIC FORMAT (.gen_public)
    // PUBLIC | NAME | ADDRESS | DESCRIPTION 
    
    public struct GenesisAccountData
    {
        public byte[] RandomPrivate;
        public byte[] Public;
        public string Name;
        public string Address;
        public string Description;
        public bool HasPrivate;
    }

    /// <summary>
    /// Parse the data from the Genesis File to Load the DB.
    /// </summary>
    public class GenesisFileParser
    {
        bool SecretFile = false;

        StreamReader sr;

        public GenesisFileParser(string FileName)
        {
            sr = new StreamReader(FileName);

            if (FileName.ToLower().EndsWith(".gen_secret")) // Ugly Hack
            {
                SecretFile = true;
            }
        }

        public bool GetAccounts(out List<GenesisAccountData> accounts)
        {
            accounts = new List<GenesisAccountData>();

            sr.BaseStream.Seek(0, SeekOrigin.Begin);

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] parts = line.Split(new char[] { '|' }, StringSplitOptions.None);

                GenesisAccountData GAD = new GenesisAccountData();
                bool good = false;

                if (parts.Length == 5 && SecretFile)
                {
                    GAD.RandomPrivate = HexUtil.GetBytes(parts[0].Trim());
                    GAD.Public = HexUtil.GetBytes(parts[1].Trim());
                    GAD.Name = parts[2].Trim();
                    GAD.Address = parts[2].Trim();
                    GAD.Description = parts[2].Trim();
                    GAD.HasPrivate = true;
                    good = true;
                }
                else if (parts.Length == 4 && !SecretFile)
                {
                    GAD.RandomPrivate = new byte[0];
                    GAD.Public = HexUtil.GetBytes(parts[0].Trim());
                    GAD.Name = parts[1].Trim();
                    GAD.Address = parts[2].Trim();
                    GAD.Description = parts[3].Trim();
                    GAD.HasPrivate = false;
                    good = true;
                }

                if (good)
                {
                    accounts.Add(GAD);
                }
            }

            return SecretFile;
        }
    }


        
}
