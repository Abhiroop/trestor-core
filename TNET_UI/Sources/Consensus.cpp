#include "Consensus.h"
#include "Constants.h"



void Consensus::ProcessIncomingPacket(NetworkPacket packet)
{
	switch (packet.Type)
	{

	case TPT_TRANS_REQUEST:

		TransactionContent tc;
		tc.Deserialize(packet.Data);

		

		break;


	}


}

