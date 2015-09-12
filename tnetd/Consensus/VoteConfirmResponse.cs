
//  @Author: Arpan Jati
//  @Date: 5 Sept 2015 

using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

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

        public void Deserialize(byte[] Data)
        {
            Init();

            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);

            foreach (ProtocolDataType PDT in PDTs)
            {
                switch (PDT.NameType)
                {

                    case 0:
                        byte[] data = new byte[0];
                        ProtocolPackager.UnpackByteVector(PDT, 0, ref data);
                        if (data.Length > 0)
                        {
                            FinalBallot.Deserialize(data);
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
