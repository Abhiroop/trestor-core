
//  @Author: Arpan Jati
//  @Date: March 2016

using System.Collections.Generic;
using TNetD.Consensus;
using TNetD.Protocol;

namespace TNetD.Nodes
{
    class HeartbeatMessage : ISerializableBase
    {
        public LedgerCloseSequence LCS;
        public VotingStates VotingState;
        public ConsensusStates ConsensusState;

        public HeartbeatMessage()
        {
            LCS = new LedgerCloseSequence();
            VotingState = VotingStates.STNone;
            ConsensusState = ConsensusStates.Sync;
        }

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
            PDTs.Add(ProtocolPackager.Pack(LCS.Serialize(), 0));
            PDTs.Add(ProtocolPackager.Pack((byte)VotingState, 1));
            PDTs.Add(ProtocolPackager.Pack((byte)ConsensusState, 2));
            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] data)
        {
            foreach (var PDT in ProtocolPackager.UnPackRaw(data))
            {
                switch (PDT.NameType)
                {
                    case 0:

                        byte[] _data;
                        if (ProtocolPackager.UnpackByteVector(PDT, 0, out _data))
                        {
                            LCS.Deserialize(_data);
                        }

                        break;

                    case 1:

                        byte _vs = 0;
                        if (ProtocolPackager.UnpackByte(PDT, 1, ref _vs))
                        {
                            VotingState = (VotingStates)_vs;
                        }

                        break;

                    case 2:

                        byte _cs = 0;
                        if (ProtocolPackager.UnpackByte(PDT, 2, ref _cs))
                        {
                            ConsensusState = (ConsensusStates)_cs;
                        }

                        break;
                }
            }
        }
    }
}
