
// @Author: Arpan Jati
// @Date: 3 Sept 2015 

using TNetD.Protocol;
using System.Collections.Generic;

namespace TNetD.Consensus
{
    class VoteResponseMessage : ISerializableBase
    {
        public Ballot Ballot;

        public bool GoodBallot;
        public bool IsSynced;

        public VotingStates VotingState;

        public ConsensusStates ConsensusState;

        public VoteResponseMessage(Ballot ballot, bool goodBallot, bool isSynced, VotingStates votingState, ConsensusStates consensusState)
        {
            this.Ballot = ballot;
            this.IsSynced = isSynced;
            this.GoodBallot = goodBallot;
            this.VotingState = votingState;
            this.ConsensusState = consensusState;
        }

        public VoteResponseMessage()
        {
            this.Ballot = new Ballot();
            this.GoodBallot = false;
            this.IsSynced = false;
        }

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();

            PDTs.Add(ProtocolPackager.Pack(Ballot.Serialize(), 0));                        
            PDTs.Add(ProtocolPackager.Pack(GoodBallot, 1));
            PDTs.Add(ProtocolPackager.Pack(IsSynced, 2));
            PDTs.Add(ProtocolPackager.Pack((byte)VotingState, 3));
            PDTs.Add(ProtocolPackager.Pack((byte)ConsensusState, 4));

            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(data);
            
            foreach(var PDT in PDTs)
            { 
                switch (PDT.NameType)
                {
                    case 0:
                        byte[] _data = null;
                        if (ProtocolPackager.UnpackByteVector(PDT, 0, ref _data))
                        {
                            Ballot.Deserialize(_data);
                        }

                        break;

                    case 1:
                        ProtocolPackager.UnpackBool(PDT, 1, ref GoodBallot);
                        break;

                    case 2:
                        ProtocolPackager.UnpackBool(PDT, 2, ref IsSynced);
                        break;

                    case 3:
                        byte b = 0;
                        if(ProtocolPackager.UnpackByte(PDT, 3, ref b))
                        {
                            VotingState = (VotingStates) b;
                        }
                        break;
                    case 4:
                        byte by = 0;
                        if(ProtocolPackager.UnpackByte(PDT, 4, ref by))
                        {
                            ConsensusState = (ConsensusStates) by;
                        }
                        break;
                }
            }
        }


    }
}
