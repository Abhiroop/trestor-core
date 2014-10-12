//
// @Author : Arpan Jati
// @Date: 12th Oct 2014
//

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
	case TPT_CONS_CURRENT_SET:
	case TPT_CONS_REQUEST_TX:
	case TPT_CONS_RESP_TX:
	case TPT_CONS_VOTES:
	case TPT_CONS_TIME_SYNC:
	case TPT_CONS_DOUBLESPENDERS:

		break;

	}
}



