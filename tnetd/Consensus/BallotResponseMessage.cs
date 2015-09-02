
// @Author: Arpan Jati
// @Date: 3 Sept 2015 

using TNetD.Protocol;
using System.Collections.Generic;

namespace TNetD.Consensus
{
    class BallotResponseMessage : ISerializableBase
    {
        public Ballot ballot;

        public bool goodBallot;

        public BallotResponseMessage(Ballot ballot, bool goodBallot)
        {
            this.ballot = ballot;
            this.goodBallot = goodBallot;
        }

        public BallotResponseMessage()
        {
            this.ballot = new Ballot();
            this.goodBallot = false;
        }

        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();

            PDTs.Add(ProtocolPackager.Pack(ballot.Serialize(), 0));                        
            PDTs.Add(ProtocolPackager.Pack(goodBallot, 1));

            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] Data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);
            int cnt = 0;

            while (cnt < (int)PDTs.Count)
            {
                ProtocolDataType PDT = PDTs[cnt++];

                switch (PDT.NameType)
                {
                    case 0:
                        byte[] data = null;
                        if (ProtocolPackager.UnpackByteVector(PDT, 0, ref data))
                        {
                            Ballot blt = new Ballot();
                            blt.Deserialize(data);
                        }

                        break;

                    case 1:

                        ProtocolPackager.UnpackBool(PDT, 1, ref goodBallot);
                        break;                  
                }
            }
        }


    }
}
