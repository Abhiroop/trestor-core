using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetCommon.Protocol;

namespace TNetWallet.WalletOperations
{
    class UserRegistrationPacket
    {
        public string UserName;
        public byte[] PublicKey;
        public byte[] ExpandedPrivateKey;

        public byte[] Address;

        public byte AccountType = 0;

        private byte[] Signature;

        public UserRegistrationPacket(string UserName, byte[] PublicKey, byte[] ExpandedPrivateKey)
        {
            this.UserName = UserName;
            this.PublicKey = PublicKey;
            this.ExpandedPrivateKey = ExpandedPrivateKey;

            byte[] TranData = GetRegistrationData();
            Signature = Ed25519.Sign(TranData, ExpandedPrivateKey);
        }

        byte[] GetRegistrationData()
        {
            List<byte> datas = new List<byte>();

            Encoding enc = Encoding.GetEncoding(28591);

            datas.AddRange(enc.GetBytes(UserName));
            datas.AddRange(PublicKey);

            

            datas.AddRange(Address);


            return datas.ToArray();
        }


        public byte [] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
            PDTs.Add(ProtocolPackager.Pack(UserName, 0));
            PDTs.Add(ProtocolPackager.Pack(PublicKey, 1));
            PDTs.Add(ProtocolPackager.Pack(Address, 2));
            PDTs.Add(ProtocolPackager.Pack(AccountType, 3));
            PDTs.Add(ProtocolPackager.Pack(Signature, 4));
            return ProtocolPackager.PackRaw(PDTs);
        }
    }
}
