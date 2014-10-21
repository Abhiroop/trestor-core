//
// @Author : Arpan Jati
// @Date: 12th Oct 2014
//

#include <Windows.h>

#include "Node.h"
#include "Constants.h"
#include "ProtocolPackager.h"
//#include "Timer.h"
#include <functional>

#include <mutex>
#include <memory>

Ledger Node::getLedger()
{
	return ledger;
}

Node::Node()
{
	
}

Node::Node(FakeNetwork _network)
{
	network = _network;
}

void Node::UpdateEvent()
{
	//MessageQueue.push(this->PublicKey.ToString() + " Is Awake");
	LocalMoney ++;

	CreateArbitraryTransactionAndSendToTrustedNodes();
}

void CALLBACK TimerProcND(void* lpParametar, BOOLEAN TimerOrWaitFired);

Node::Node(FakeNetwork _network, string _Name, int _ConnectionLimit,  long Money, int TimerRate)
{
	network = _network;

	ConnectionLimit = ConnectionLimit;
	byte Seed[32];

	RandomFillBytes(Seed, 32);

	ed25519_create_seed(Seed);

	ed25519_create_keypair(_PublicKey, _PrivateKey, Seed);

	PublicKey = Hash(_PublicKey, _PublicKey + 32);

	ledger = Ledger("LEDGER_" + _Name + ".dat", network);

	consensus = Consensus(PublicKey, ledger, network);

	Name = _Name;

	AI = AccountInfo(PublicKey, Money, _Name, 0, 0);

	ledger.AddUserToLedger(AI);

	LocalMoney = 0;

	//function<void()> f = bind(&Node::UpdateEvent, *this);
	
	//Tmr = TimerX::Timer(hTimerQueue, 0, Constants::SIM_REFRESH_MS, f);
	
	// Create the timer queue.
	hTimerQueue = CreateTimerQueue();
	if (NULL == hTimerQueue)
	{

	}

	if (!CreateTimerQueueTimer(&hTimer, hTimerQueue, (WAITORTIMERCALLBACK)TimerProcND, this, Constants::SIM_REFRESH_MS, Constants::SIM_REFRESH_MS, 0))
	{
		//printf("CreateTimerQueueTimer failed (%d)\n", GetLastError());
		//return 3;
	}
		
	/*

	//typedef void (Node::*)(void)function_t;

	string type = f.target_type().name();

	auto fptr = f.target<void (Node::*)()>();
	if (fptr != nullptr)
	{
		auto fX = *fptr;
		
		MessageQueue.push("Good MSG");

		//fX();
		//function_t* ptr_fun = *ptr_ptr_fun;
		//ptr_fun();
		//TimerX::Timer Tmr = TimerX::Timer(0, Constants::SIM_REFRESH_MS, **ptr_fun);
	}*/
	
}



void CALLBACK TimerProcND(void* lpParametar, BOOLEAN TimerOrWaitFired)
{
	Node* obj = (Node*)lpParametar;
	obj->UpdateEvent();
}

/*void Tmr_Elapsed(object sender, ElapsedEventArgs e)
{
while (PendingIncomingCandidates.Count > 0)
{

// ledger.PushNewCandidate(PendingIncomingCandidates.Pop());
}

// Send the transcation to the TrustedNodes
while (PendingIncomingTransactions.Count > 0)
{
// TransactionContent tc = PendingIncomingTransactions.Pop();

//  ReceivedCandidates
// ReceivedCandidates
}
}*/

void Node::CreateArbitraryTransactionAndSendToTrustedNodes()
{
	vector<TransactionEntity> tsks;
	
	//GlobalNodes.

	for (concurrent_hash_map<Hash, shared_ptr<Node>>::iterator _ts = GlobalNodes.begin(); _ts != GlobalNodes.end(); ++_ts)
	{
		shared_ptr<Node> dest = _ts->second;

		int64_t Amount = this->Money() / 8;

		TransactionEntity tsk = TransactionEntity(dest->PublicKey, Amount);
		tsks.push_back(tsk);

		//TransactionContent tco = TransactionContent(this->PublicKey, 0, tsks, Hash() );
		
		vector<byte> data;// = tco.Serialize();
		
		network.SendPacket(NetworkPacketQueueEntry(_ts->second->PublicKey, NetworkPacket(this->PublicKey, TPT_TRANS_REQUEST, data)));

		//_ts->second->SendTransaction(this->PublicKey, tco);

		OutTransactionCount++;
	}
	

	/*Node destNode = Constants.GlobalNodeList[Constants.random.Next(0, Constants.GlobalNodeList.Count)];

	if (destNode.PublicKey != PublicKey)
	{
		int Amount = Constants.random.Next(0, (int)(Money / 2));

		TransactionEntity tsk = new TransactionEntity(destNode.PublicKey, Amount);
		tsks.Add(tsk);

		TransactionContent tco = new TransactionContent(PublicKey, 0, tsks.ToArray(), new byte[0]);

		OutTransactionCount++;
	}*/
}

void Node::InitializeValuesFromGlobalLedger()
{

}

int64_t Node::Money()
{
	AccountInfo ai;
	bool got = ledger.GetAccount(PublicKey, ai);

	if (got)
		return ai.Money;
	else return -1;
}

void Node::Receive(NetworkPacket Packet)
{
	/*string _SZ = ", " + to_string( Packet.Data.size()) + (string)" Bytes";
	string _OTH_MSG = " Received Packet :" + to_string(Packet.Type) + (string)", From:" + Packet.PublicKey_Src.ToString() + _SZ;
	string MSG = (string)"[" + PublicKey.ToString() + "] : " + _OTH_MSG;
	MessageQueue.push(MSG);*/
	
	switch (Packet.Type)
	{

	case TPT_TRANS_REQUEST:

	case TPT_CONS_STATE:
	case TPT_CONS_CURRENT_SET:
	case TPT_CONS_REQUEST_TX:
	case TPT_CONS_RESP_TX:
	case TPT_CONS_VOTES:
	case TPT_CONS_TIME_SYNC:
	case TPT_CONS_DOUBLESPENDERS:

		consensus.ProcessIncomingPacket(Packet);

		break;

	case TPT_LSYNC_FETCH_ROOT:
	case TPT_LSYNC_FETCH_LAYER_INFO:
	case TPT_LSYNC_FETCH_LAYER_DATA:

		ledger.ProcessIncomingPacket(Packet);

	}

}