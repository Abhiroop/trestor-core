/*
 *  @Author: Arpan Jati
 *  @Version: 1.0
 *  @Date: Jan 22, 2015 | Feb 1, 2015
 *  @Description: AddressData to keep the individual address info.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Address
{
    public class AddressData
    {
        public string AddressString { get; set; }
        public NetworkType NetworkType { get; set; }
        public AccountType AccountType { get; set; }

        [JsonIgnore]
        public byte[] AddressBinary { get; set; }

        public AddressData(string Base58Address)
        {
            byte[] add_bin = Base58Encoding.DecodeWithCheckSum(Base58Address);

            if (add_bin.Length == 22)
            {
                AddressString = Base58Address;

                AddressBinary = add_bin;// new byte[20];

                // Array.Copy(add_data, 2, Address, 0, 20);

                NetworkType = (NetworkType)add_bin[0];
                AccountType = (AccountType)add_bin[1];
            }
            else
            {
                throw new ArgumentException("Invalid Decoded Address Length");
            }
        }

        /// <summary>
        /// Returns True if the NetworkType and AccountType is valid.
        /// </summary>
        /// <returns></returns>
        public bool ValidateAccountType()
        {
            bool TypesFine = ValidateNetworkType();

            if (Common.NetworkType == Address.NetworkType.MainNet)
            {
                if (!((AccountType == AccountType.MainNormal) ||
                    (AccountType == AccountType.MainValidator) ||
                    (AccountType == AccountType.MainGenesis)))
                {
                    TypesFine = false;
                }
            }
            else
            {
                if (!((AccountType == AccountType.TestNormal) ||
                    (AccountType == AccountType.TestValidator) ||
                    (AccountType == AccountType.TestGenesis)))
                {
                    TypesFine = false;
                }
            }

            return TypesFine;
        }

        /// <summary>
        /// Returns True if NetworkType is correct. 
        /// </summary>
        /// <returns></returns>
        public bool ValidateNetworkType()
        {            
            return NetworkType == Common.NetworkType;
        }
    }
}
