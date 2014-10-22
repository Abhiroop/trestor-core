//
// @Author : Arpan Jati
// @Date: 12th Oct 2014
//

#ifndef Consensus_H
#define Consensus_H

//#include "Constants.h"

#include "Ledger.h"
#include "Utils.h"
#include "ProtocolPackager.h"
#include "TransactionIDInfo.h"

// FOR TESTING
#include "FakeNetwork.h"

#include "ConsensusMap.h"
#include "IncomingTransactionMap.h"
#include "NetworkCommand.h"


#include "tbb\concurrent_queue.h"

using namespace tbb;

class Consensus
{
	ConsensusMap consensusMap;
	IncomingTransactionMap incomingTransactionMap;
	Ledger ledger;

	Hash PublicKey;

	// A list of transactions which we don't have, but others in the public
	// set have.
	concurrent_queue<Hash> TransactionsToBeFetched;

	FakeNetwork network;

public:

	Consensus();

	void DoEvents();

	Consensus(Hash _PublicKey, Ledger _ledger, FakeNetwork _network);

	void ProcessIncomingPacket(NetworkPacket packet);


};



#endif