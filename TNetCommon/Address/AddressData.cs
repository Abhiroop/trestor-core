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
        public byte[] Address { get; set; }

        public AddressData(string Base58Address)
        {
            byte[] add_data = Base58Encoding.DecodeWithCheckSum(Base58Address);

            if (add_data.Length == 22)
            {               
                AddressString = Base58Address;

                Address = new byte[20];

                Array.Copy(add_data, 2, Address, 0, 20);

                NetworkType = (NetworkType)add_data[0];
                AccountType = (AccountType)add_data[1];              
            }
            else
            {
                throw new ArgumentException("Invalid Decoded Address Length");
            }
        }
    }
}
