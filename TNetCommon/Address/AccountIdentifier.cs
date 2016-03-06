/*
 *  @Author: Arpan Jati
 *  @Version: 1.0
 *  @Date: Jan 22 2015 | Jan 20, 2015
 *  @Description: AccountIdentifier to keep the individual addresses.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD.Address
{
    public class AccountIdentifier
    {
        public byte[] PublicKey { get; private set; }

        public string Name { get; private set; }

        public AddressData AddressData { get; private set; }

        public AccountIdentifier()
        {

        }

        public AccountIdentifier(byte[] publicKey, string name, string addressDataString)
        {
            this.PublicKey = publicKey;
            this.Name = name;
            this.AddressData = AddressFactory.DecodeAddressString(addressDataString);

            if (!AddressFactory.VerfiyAddress(addressDataString, PublicKey, Name, AddressData.NetworkType, AddressData.AccountType))
            {
                throw new ArgumentException("AccountIdentifier: Invalid Address Arguments");
            }
        }

        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[3];
            PDTs[0] = (ProtocolPackager.Pack(PublicKey, 0));
            PDTs[1] = (ProtocolPackager.Pack(Name, 1));
            PDTs[2] = (ProtocolPackager.Pack(AddressData.AddressString, 2));
            return ProtocolPackager.PackRaw(PDTs);
        }


        public void Deserialize(byte[] Data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);
            int cnt = 0;

            while (cnt < (int)PDTs.Count)
            {
                ProtocolDataType PDT = PDTs[cnt++];

                switch (PDT.NameType)
                {
                    case 0:
                        byte[] PK = new byte[Common.KEYLEN_PUBLIC];
                        ProtocolPackager.UnpackByteVector_s(PDT, 0, Common.KEYLEN_PUBLIC, ref PK);
                        PublicKey = PK;
                        break;

                    case 1:
                        string _Name = "";
                        ProtocolPackager.UnpackString(PDT, 1, ref _Name);
                        Name = _Name;
                        break;

                    case 2:
                        string AddressString = "";
                        ProtocolPackager.UnpackString(PDT, 2, ref AddressString);
                        AddressData = new AddressData(AddressString);
                        break;
                }
            }
        }

    }
}


