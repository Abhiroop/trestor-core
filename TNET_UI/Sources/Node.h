#ifndef NODE_CONTROLLER_H
#define NODE_CONTROLLER_H

#include "AccountInfo.h"
#include "CandidateSet.h"
#include "Ledger.h"
#include "ed25519/ed25519.h"
#include <hash_map>
#include <functional>
#include <Windows.h>
//#include "Timer.h"
#include "tbb/concurrent_queue.h"
#include <memory>

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
public:
	hash_map<Hash, shared_ptr<Node>> Connections;

	hash_map<Hash, shared_ptr<Node>> TrustedNodes;

	HANDLE hTimerQueue = nullptr;
	//function<void()> f;
	//TimerX::Timer Tmr;
	HANDLE hTimer = NULL;
	/// <summary>
	/// Outer Dict is TransactionID, inner is Voter node.
	/// </summary>
	//public Dictionary<Hash, Dictionary<Hash, CandidateStatus>> ReceivedCandidates = new Dictionary<Hash, Dictionary<Hash, CandidateStatus>>();
	
	int ConnectionLimit = 0;
	int OutTransactionCount =0;
	int InCandidatesCount = 0;;
	int InTransactionCount =0;

	Node();

	AccountInfo AI;

	shared_ptr<Ledger> ledger;

	Ledger getLocalLedger();

	byte _PrivateKey[32];
	byte _PublicKey[32];

	Hash PublicKey;

	string Name;

	int64_t LocalMoney;

	Node(string _Name, int _ConnectionLimit, shared_ptr<Ledger> ledger, long Money, int TimerRate);

	void CreateArbitraryTransactionAndSendToTrustedNodes();

	void InitializeValuesFromGlobalLedger();

	int64_t Money();

	stack<TransactionContentPack> PendingIncomingCandidates;

	tbb::concurrent_queue<TransactionContentPack> PendingIncomingTransactions;

	void SendTransaction(Hash source, TransactionContent Transaction);

	void SendCandidates(Hash source, vector<TransactionContent> Transactions);

	void Receive(NetworkPacket Packet);

	void UpdateEvent();

};


#endif