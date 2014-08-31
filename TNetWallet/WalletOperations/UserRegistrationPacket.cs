using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetCommon;
using TNetCommon.Protocol;
using TNetCommon.Address;

namespace TNetWallet.WalletOperations
{
    class UserRegistrationPacket
    {
        public byte[] UserName;
        public byte[] PublicKey;
        public byte[] ExpandedPrivateKey;

        public byte[] Address;

        public byte NetworkType = 1;
        public byte AccountType = 1;

        private byte[] Signature;

        public UserRegistrationPacket(string UserName, byte[] PublicKey, byte[] ExpandedPrivateKey)
        {
            this.UserName = Utils.Encoding88591.GetBytes(UserName);
            this.PublicKey = PublicKey;
            this.ExpandedPrivateKey = ExpandedPrivateKey;
            this.Address = AddressFactory.GetAddress(PublicKey, UserName, NetworkType, AccountType);

            byte[] TranData = GetRegistrationData();
            Signature = Ed25519.Sign(TranData, ExpandedPrivateKey);
        }

        byte[] GetRegistrationData()
        {
            List<byte> datas = new List<byte>();

            datas.AddRange(UserName);
            datas.AddRange(PublicKey);
            datas.AddRange(Address);

            return datas.ToArray();
        }
        
        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
            PDTs.Add(ProtocolPackager.Pack(UserName, 0));
            PDTs.Add(ProtocolPackager.Pack(PublicKey, 1));
            PDTs.Add(ProtocolPackager.Pack(Address, 2));
            PDTs.Add(ProtocolPackager.Pack(Signature, 3));
            return ProtocolPackager.PackRaw(PDTs);
        }
    }
}
