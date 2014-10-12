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

using namespace tbb;

class Consensus
{
	ConsensusMap consensusMap;
	IncomingTransactionMap incomingTransactionMap;
	Ledger ledger;

	FakeNetwork network;

public:

	Consensus();

	Consensus(Ledger _ledger, FakeNetwork _network);

	void ProcessIncomingPacket(NetworkPacket packet);


};



#endif