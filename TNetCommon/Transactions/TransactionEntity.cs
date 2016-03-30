
// @Author : Arpan Jati
// @Date: 23th Dec 2014 | 12 Jan 2015
// 22 Jan 2015 : Name Addition for adresses and new account creation.
// 26 Jan 2015 : ValidateEntity()
// TODO: Needs Good cleanup !!!.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;
using TNetD;
using TNetD.Address;
using TNetD.Json.JS_Structs;

namespace TNetD.Transactions
{
    public class TransactionEntity : ISerializableBase
    {
        byte[] publicKey;
        string name;
        string address;
        long _value;

        /// <summary>
        /// Public key for the account.
        /// </summary>
        public byte[] PublicKey
        {
            get { return publicKey; }
        }

        /// <summary>
        /// Name of the account. (Optional)
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Base58 Encoded Address.
        /// </summary>
        public string Address
        {
            get { return address; }
        }

        /// <summary>
        /// Remaining value/amount held with the account.
        /// </summary>
        public long Value
        {
            get { return _value; }
        }

        public TransactionEntity()
        {
            publicKey = new byte[0];
            _value = 0;
            this.name = "";
            this.address = "";
        }

        public TransactionEntity(byte[] publicKey, string name, string address, long value)
        {
            this.publicKey = PublicKey;
            this._value = value;
            this.name = Name;
            this.address = address;
        }

        public TransactionEntity(byte[] publicKey, string address, long value)
        {
            this.publicKey = PublicKey;
            this._value = value;
            this.name = "";
            this.address = address;
        }

        public TransactionEntity(AccountIdentifier account, long value)
        {
            this._value = value;
            this.publicKey = account.PublicKey;
            this.name = account.Name;
            this.address = account.AddressData.AddressString;
        }

        public TransactionEntity(JS_TransactionEntity entity)
        {
            Deserialize(entity);
        }

        /// <summary>
        /// Returns True if the Address is valid and AccountType, NetworkType is correct;
        /// The value is aslo checked for a positive value. 
        /// </summary>
        /// <returns></returns>
        public bool ValidateEntity()
        {
            AddressData AD;
            bool Address_OK = AddressFactory.VerfiyAddress(out AD, address, publicKey, name);
            bool Value_OK = (_value > 0);
            return AD.ValidateAccountType() && Address_OK && Value_OK;
        }

        public byte[] Serialize()
        {
            var PDTs = new List<ProtocolDataType>();

            PDTs.Add(ProtocolPackager.Pack(publicKey, 0));
            PDTs.Add(ProtocolPackager.Pack(_value, 1));
            PDTs.Add(ProtocolPackager.Pack(name, 2));
            PDTs.Add(ProtocolPackager.Pack(address, 3));

            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(JS_TransactionEntity entity)
        {
            publicKey = entity.PublicKey;
            _value = entity.Value;
            name = entity.Name;
            address = entity.Address;            
        }

        public void Deserialize(byte[] Data)
        {
            foreach(var PDT in ProtocolPackager.UnPackRaw(Data))
            {
                switch (PDT.NameType)
                {
                    case 0:
                        ProtocolPackager.UnpackByteVector_s(PDT, 0, Common.KEYLEN_PUBLIC, out publicKey);
                        break;

                    case 1:
                        ProtocolPackager.UnpackInt64(PDT, 1, ref _value);
                        break;

                    case 2:
                        ProtocolPackager.UnpackString(PDT, 2, ref name);
                        break;

                    case 3:
                        ProtocolPackager.UnpackString(PDT, 3, ref address);
                        break;
                }
            }
        }
    }
}
