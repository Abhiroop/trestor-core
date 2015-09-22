//
//  @Author: Arpan Jati
//  @Date: September 2015 
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD.Consensus
{
    class LedgerCloseSequence : ISerializableBase, IEquatable<LedgerCloseSequence>
    {
        private long sequence;
        private Hash hash;

        public long Sequence { get { return sequence; } }
        public Hash Hash { get { return hash; } }

        public LedgerCloseSequence(long sequence, Hash hash)
        {
            this.sequence = sequence;
            this.hash = hash;
        }

        public LedgerCloseSequence()
        {
            sequence = 0;
            hash = new Hash();
        }

        public LedgerCloseSequence(LedgerCloseData lcd)
        {
            sequence = lcd.SequenceNumber;
            hash = new Hash(lcd.LedgerHash);
        }

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();

            PDTs.Add(ProtocolPackager.PackVarint(sequence, 0));
            PDTs.Add(ProtocolPackager.Pack(hash, 1));

            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] Data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);

            foreach (var PDT in PDTs)
            {
                switch (PDT.NameType)
                {
                    case 0:
                        ProtocolPackager.UnpackVarint(PDT, 0, ref sequence);
                        break;

                    case 1:
                        ProtocolPackager.UnpackHash(PDT, 1, out hash);
                        break;
                }
            }
        }

        public bool Equals(LedgerCloseSequence other)
        {
            if (other == null)
                return false;

            if (sequence != other.sequence) return false;

            if (hash != other.hash) return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            LedgerCloseSequence lcsObj = obj as LedgerCloseSequence;
            if (lcsObj == null)
                return false;
            else
                return Equals(lcsObj);
        }

        public static bool operator ==(LedgerCloseSequence a, LedgerCloseSequence b)
        {
            if (Object.ReferenceEquals(a, b))
                return true;

            if (Object.ReferenceEquals(a, null))
                return false;

            return a.Equals((object)b);
        }

        public static bool operator !=(LedgerCloseSequence a, LedgerCloseSequence b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            if (hash.Hex.Length == 0)
            {
                return hash + " - " + sequence;
            }
            else
            {
                return hash.ToString().Substring(0, 6) + " - " + sequence;
            }            
        }

        /// <summary>
        /// Pretty Dodgy. Eh !
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return hash.GetHashCode();
        }

    }
}
