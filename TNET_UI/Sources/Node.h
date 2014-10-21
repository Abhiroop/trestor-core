//
// @Author : Arpan Jati
// @Date: 12th Oct 2014
//

#ifndef NODE_CONTROLLER_H
#define NODE_CONTROLLER_H

#include "AccountInfo.h"
#include "CandidateSet.h"
#include "Ledger.h"
#include "ed25519/ed25519.h"
#include <hash_map>
#include <functional>
#include <memory>
#include <Windows.h>
#include "tbb/concurrent_queue.h"
#include "FakeNetwork.h"
#include "Consensus.h"

using namespace tbb;

typedef struct CandidateStatus
{
public: bool Vote;
		bool Forwarded;
		CandidateStatus(bool _Vote, bool _Forwarded)
		{
			Vote = _Vote;
			Forwarded = _Forwarded;
		}

} CandidateStatus;

typedef struct TransactionContentPack
{
public: Hash Source;
		TransactionContent Transacation;

		TransactionContentPack(Hash _Source, TransactionContent _Transacation)
		{
			Source = _Source;
			Transacation = _Transacation;
		}

		TransactionContentPack()
		{

		}

} TransactionContentPack;

class Node
{
	FakeNetwork network;

	HANDLE hTimerQueue = nullptr;
	//function<void()> f;
	//TimerX::Timer Tmr;
	HANDLE hTimer = NULL;

public:
	Ledger ledger;
	Ledger getLedger();

	Consensus consensus;

	hash_map<Hash, shared_ptr<Node>> Connections;
	hash_map<Hash, shared_ptr<Node>> TrustedNodes;

	int ConnectionLimit = 0;
	int OutTransactionCount = 0;
	int InCandidatesCount = 0;
	int InTransactionCount = 0;

	Node();

	Node(FakeNetwork _network);

	AccountInfo AI;	

	byte _PrivateKey[32];
	byte _PublicKey[32];

	Hash PublicKey;

	string Name;

	int64_t LocalMoney;

	Node(FakeNetwork _network, string _Name, int _ConnectionLimit, long Money, int TimerRate);

	void CreateArbitraryTransactionAndSendToTrustedNodes();

	void InitializeValuesFromGlobalLedger();

	int64_t Money();

	void Receive(NetworkPacket Packet);

	void UpdateEvent();

};


#endif