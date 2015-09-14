using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD.Consensus
{
    class SyncMessage
    {
        public LedgerCloseSequence LedgerCloseSequence;
        public ConsensusStates ConsensusState;

        public SyncMessage()
        {
            LedgerCloseSequence = new LedgerCloseSequence();
            ConsensusState = ConsensusStates.Sync;
        }

        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[2];
            PDTs[0] = ProtocolPackager.Pack(LedgerCloseSequence, 0);
            PDTs[1] = ProtocolPackager.Pack((byte)ConsensusState, 1);
            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(data);

            foreach (var PDT in PDTs)
            {
                switch (PDT.NameType)
                {
                    case 0:
                        byte[] _data = null;
                        if (ProtocolPackager.UnpackByteVector(PDT, 0, ref _data))
                        {
                            LedgerCloseSequence.Deserialize(_data);
                        }

                        break;

                    case 1:
                        byte _byte = 0;
                        if (ProtocolPackager.UnpackByte(PDT, 1, ref _byte))
                        {
                            ConsensusState = (ConsensusStates)_byte;
                        }

                        break;
                }
            }
        }
    }
}
