/*
 *  @Author: Arpan Jati
 *  @Version: 1.0
 *  @Date: Jan 22 2015
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
    }
}
