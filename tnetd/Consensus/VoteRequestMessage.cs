
//  @Author: Arpan Jati
//  @Date: June 2015 | Sept 2015

using TNetD.Protocol;

namespace TNetD.Consensus
{
    class VoteRequestMessage : ISerializableBase
    {
        public LedgerCloseSequence LedgerCloseSequence;
        public VotingStates VotingState;

        public VoteRequestMessage()
        {
            LedgerCloseSequence = new LedgerCloseSequence();
            VotingState = VotingStates.STNone;
        }

        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[2];
            PDTs[0] = ProtocolPackager.Pack(LedgerCloseSequence, 0);
            PDTs[1] = ProtocolPackager.Pack((byte)VotingState, 1);
            return ProtocolPackager.PackRaw(PDTs);
        }
       
        public void Deserialize(byte[] data)
        {
            foreach (var PDT in ProtocolPackager.UnPackRaw(data))
            {
                switch (PDT.NameType)
                {
                    case 0:
                        byte[] _lcs;
                        if (ProtocolPackager.UnpackByteVector(PDT, 0, out _lcs))
                        {
                            LedgerCloseSequence.Deserialize(_lcs);
                        }

                        break;

                    case 1:
                        byte _byte = 0;
                        if (ProtocolPackager.UnpackByte(PDT, 1, ref _byte))
                        {
                            VotingState = (VotingStates)_byte;
                        }

                        break;
                }
            }
        }
    }
}
