//
// @Author : Arpan Jati
// @Date: 12th Oct 2014
//


#include "Constants.h"
#include "Consensus.h"

#include "TransactionSetType.h"
#include "TransactionContent.h"

#include <hash_set>
#include "VoteType.h"
#include "tbb\concurrent_hash_map.h"
#include "tbb\concurrent_queue.h"

Consensus::Consensus()
{

}

Consensus::Consensus(Ledger _ledger, FakeNetwork _network)
{
	ledger = _ledger;
	network = _network;
}

void Consensus::ProcessIncomingPacket(NetworkPacket packet)
{

	switch (packet.Type)
	{

	case TPT_CONS_STATE:


	case TPT_CONS_CURRENT_SET: //

		// SET OF TRANSACTION ID's
		// This means a node has sent its current consensus set, we should consider that and add it to our 
		// list of current candidate transactions.

	{
		vector<Hash> TransactionIDs;

		vector<ProtocolDataType> PDTs = ProtocolPackager::UnPackRaw(packet.Data);

		for (int i = 0; i < (int)PDTs.size(); i++)
		{
			vector<byte> TR_ID;
			if (ProtocolPackager::UnpackByteVector_s(PDTs[i++], 0, Constants::LEN_TRANSACTION_ID, TR_ID))
			{
				TransactionIDs.push_back(TR_ID);
			}
		}

	}

		break;

		// 

		//incomingTransactionMap.

	case TPT_CONS_REQUEST_TX:
	case TPT_CONS_RESP_TX:
	case TPT_CONS_VOTES:
	case TPT_CONS_TIME_SYNC:
	case TPT_CONS_DOUBLESPENDERS:

		break;

	}
}



