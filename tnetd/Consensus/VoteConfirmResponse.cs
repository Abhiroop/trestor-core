
//  @Author: Arpan Jati
//  @Date: 5 Sept 2015 

using TNetD.Protocol;
using System.Collections.Generic;

namespace TNetD.Consensus
{
    class VoteConfirmResponse : ISerializableBase
    {
        public Ballot FinalBallot;
        public bool BallotGood;
        public bool IsSynced;

        public VoteConfirmResponse()
        {
            Init();
        }

        public void Init()
        {
            FinalBallot = new Ballot();
            BallotGood = false;
            IsSynced = false;
        }

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
            PDTs.Add(ProtocolPackager.Pack(FinalBallot.Serialize(), 0));
            PDTs.Add(ProtocolPackager.Pack(BallotGood, 1));
            PDTs.Add(ProtocolPackager.Pack(IsSynced, 2));
            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] data)
        {
            Init();

            foreach (var PDT in ProtocolPackager.UnPackRaw(data))
            {
                switch (PDT.NameType)
                {
                    case 0:
                        byte[] _finalBallot;
                        ProtocolPackager.UnpackByteVector(PDT, 0, out _finalBallot);
                        if (_finalBallot.Length > 0)
                        {
                            FinalBallot.Deserialize(_finalBallot);
                        }

                        break;

                    case 1:
                        ProtocolPackager.UnpackBool(PDT, 1, ref BallotGood);
                        break;

                    case 2:
                        ProtocolPackager.UnpackBool(PDT, 2, ref IsSynced);
                        break;
                }
            }
        }       
    }
}
