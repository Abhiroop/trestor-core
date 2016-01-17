
//
//  @Author: Arpan Jati
//  @Date: September 2015 
//

using System.Collections.Generic;
using TNetD.Protocol;

namespace TNetD.Consensus
{
    class SyncState
    {
        public ConsensusStates ConsensusState;
        public VotingStates VotingState;
        public SyncState(ConsensusStates consensusState, VotingStates votingState)
        {
            ConsensusState = consensusState;
            VotingState = votingState;
        }
    }

    class SyncMessage
    {
        public LedgerCloseSequence LedgerCloseSequence;
        public ConsensusStates ConsensusState;
        public VotingStates VotingState;

        public SyncState SyncState
        {
            get { return new SyncState(ConsensusState, VotingState); }
        }

        public SyncMessage()
        {
            LedgerCloseSequence = new LedgerCloseSequence();
            ConsensusState = ConsensusStates.Sync;
            VotingState = VotingStates.STNone;
        }

        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[3];
            PDTs[0] = ProtocolPackager.Pack(LedgerCloseSequence, 0);
            PDTs[1] = ProtocolPackager.Pack((byte)ConsensusState, 1);
            PDTs[2] = ProtocolPackager.Pack((byte)VotingState, 2);
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

                    case 2:
                        byte _byte_ = 0;
                        if (ProtocolPackager.UnpackByte(PDT, 2, ref _byte_))
                        {
                            VotingState = (VotingStates)_byte_;
                        }

                        break;
                }
            }
        }
    }
}
