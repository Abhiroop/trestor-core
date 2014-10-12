//
// @Author : Arpan Jati
// @Date: 12th Oct 2014
//

#ifndef Consensus_H
#define Consensus_H

#include "TransactionSetType.h"
#include "TransactionContent.h"
#include "Ledger.h"
#include "Utils.h"
#include "ProtocolPackager.h"
#include "HashTree.h"
#include "TransactionIDInfo.h"
#include <hash_set>
#include "VoteType.h"
#include "tbb\concurrent_hash_map.h"
#include "tbb\concurrent_queue.h"

#include "ConsensusMap.h"
#include "IncomingTransactionMap.h"
#include "NetworkCommand.h"

using namespace tbb;

class Consensus
{
	ConsensusMap consensusMap;
	IncomingTransactionMap incomingTransactionMap;

public:

	void ProcessIncomingPacket(NetworkPacket packet);


};



#endif