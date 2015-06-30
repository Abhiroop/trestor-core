﻿
//  @Author: Arpan Jati
//  @Date: June 2015 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD.Consensus
{
    class BallotRequestMessage : ISerializableBase
    {
        public long LedgerCloseSequence;

        public BallotRequestMessage()
        {
            LedgerCloseSequence = 0;
        }

        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[1];
            PDTs[0] = ProtocolPackager.PackVarint(LedgerCloseSequence, 0);
            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(data);

            if (PDTs.Count == 1)
            {
                long lcs = 0;
                if (ProtocolPackager.UnpackVarint(PDTs[0], 0, ref lcs))
                {
                    LedgerCloseSequence = lcs;
                }
            }

        }
    }
}
