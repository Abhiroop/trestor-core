/*
 *  @Author: Arpan Jati
 *  @Version: 1.0
 *  @Date: Jan 22 2015
 *  @Description: AccountIdentifier to keep the individual addresses.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Address
{
    public class AccountIdentifier
    {
        public byte[] PublicKey { get; private set; }

        public string Name { get; private set; }

        public AddressData AddressData { get; private set; }

        public AccountIdentifier(byte[] publicKey, string name, string addressDataString)
        {
            this.PublicKey = publicKey;
            this.Name = name;
            this.AddressData = AddressFactory.DecodeAddressString(addressDataString);

            if(!AddressFactory.VerfiyAddress(addressDataString, PublicKey, Name, AddressData.NetworkType, AddressData.AccountType))
            {
                throw new ArgumentException("AccountIdentifier: Invalid Address Arguments");
            }
        }

    }
}


