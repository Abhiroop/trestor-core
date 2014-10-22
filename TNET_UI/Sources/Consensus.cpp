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

Consensus::Consensus(State _state, Hash _PublicKey, Ledger _ledger, FakeNetwork _network)
{
	state = _state;
	incomingTransactionMap = IncomingTransactionMap(_state);
	consensusMap = ConsensusMap(_state, incomingTransactionMap, _ledger);
	PublicKey = _PublicKey;
	ledger = _ledger;
	network = _network;
}

// Process pending/queued operations.
void Consensus::DoEvents()
{
	incomingTransactionMap.DoEvents();
	consensusMap.DoEvents();

	// Do Blacklist processing
}

void Consensus::ProcessIncomingPacket(NetworkPacket packet)
{
	switch (packet.Type)
	{

	case TPT_TRANS_REQUEST:

	{
		TransactionContent tc;
		tc.Deserialize(packet.Data);

		incomingTransactionMap.InsertNewTransaction(tc, packet.PublicKey_Src);

		// PendingIncomingTransactions.push(TransactionContentPack(Packet.PublicKey_Src, tc));
		// MessageQueue.push(to_string(InTransactionCount));
	}

		break;

	case TPT_CONS_STATE:


	case TPT_CONS_CURRENT_SET:

		// SET OF TRANSACTION ID's
		// This means a node has sent its current consensus set, we should consider that and add it to our 
		// list of current candidate transactions.

	{
		// Decode the request as a sequence of TransactionID's
		vector<vector<unsigned char>> TransactionIDs;
		ProtocolPackager::UnpackVectorVector_s(packet.Data, Constants::LEN_TRANSACTION_ID, TransactionIDs);

		for (int i = 0; i < (int)TransactionIDs.size(); i++)
		{
			// We don't have transaction info for the transaction, so get it from others.
			if (!incomingTransactionMap.HaveTransactionInfo(TransactionIDs[i]))
			{
				// Add to the queue of pending transactions.
				TransactionsToBeFetched.push(TransactionIDs[i]);
			}
		}
	}

		break;

	case TPT_CONS_REQUEST_TX:

		// Its a request for TransactionContent objects for corresponding list of TransactionID's
	{
		// Decode the request as a sequence of TransactionID's
		vector<vector<unsigned char>> TransactionIDs;
		ProtocolPackager::UnpackVectorVector_s(packet.Data, Constants::LEN_TRANSACTION_ID, TransactionIDs);

		vector<vector<unsigned char>> ResponseList;

		for (int i = 0; i < (int)TransactionIDs.size(); i++)
		{
			TransactionContentData TCD;
			// Apparently, we do have the TransactionID, yayy, add it to the list to be sent back.
			if (incomingTransactionMap.GetTransactionContentData(TCD, TransactionIDs[i]))
			{
				ResponseList.push_back(TCD.TC.Serialize());
			}
		}

		NetworkPacketQueueEntry npqe;

		//  Set the reply address
		npqe.PublicKey_Dest = packet.PublicKey_Src;
		npqe.Packet.Type = TPT_CONS_RESP_TX;
		npqe.Packet.PublicKey_Src = PublicKey;
		npqe.Packet.Data = ProtocolPackager::Pack(ResponseList);

		network.SendPacket(npqe);


	}

		break;

	case TPT_CONS_RESP_TX:
	case TPT_CONS_VOTES:
	case TPT_CONS_TIME_SYNC:
	case TPT_CONS_DOUBLESPENDERS:

		break;

	}
}



