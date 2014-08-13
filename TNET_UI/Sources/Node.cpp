
#include <Windows.h>

#include "Node.h"
#include "Constants.h"
//#include "Timer.h"
#include <functional>

#include <mutex>
#include <memory>

//std::mutex MTX;

/*Ledger Node::getLocalLedger()
{
	return ledger;
}
*/

Node::Node()
{

}


void Node::UpdateEvent()
{
	//MTX.lock();
	//MessageQueue.push(" Is Awake");
	LocalMoney ++;
	//MTX.unlock();
}

//Timer Tmr;

void CALLBACK TimerProcND(void* lpParametar, BOOLEAN TimerOrWaitFired);

Node::Node(string _Name, int _ConnectionLimit, shared_ptr<Ledger> _ledger, long Money, int TimerRate)
{
	ConnectionLimit = ConnectionLimit;
	byte Seed[32];

	RandomFillBytes(Seed, 32);

	ed25519_create_seed(Seed);

	ed25519_create_keypair(_PublicKey, _PrivateKey, Seed);

	PublicKey = Hash(_PublicKey, _PublicKey + 32);

	ledger = _ledger;

	Name = _Name;

	AI = AccountInfo(PublicKey, Money, _Name, 0);

	ledger->AddUserToLedger(AI);

	LocalMoney = 0;

	//function<void()> f = bind(&Node::UpdateEvent, *this);
	
	//Tmr = TimerX::Timer(hTimerQueue, 0, Constants::SIM_REFRESH_MS, f);
	/*
	// Create the timer queue.
	hTimerQueue = CreateTimerQueue();
	if (NULL == hTimerQueue)
	{

	}

	if (!CreateTimerQueueTimer(&hTimer, hTimerQueue, (WAITORTIMERCALLBACK)TimerProcND, this, Constants::SIM_REFRESH_MS, Constants::SIM_REFRESH_MS, 0))
	{
		//printf("CreateTimerQueueTimer failed (%d)\n", GetLastError());
		//return 3;
	}*/
		
	//typedef void (Node::*)(void)function_t;

	/*string type = f.target_type().name();

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

	//Tmr = new Timer();
	//Tmr.Elapsed += Tmr_Elapsed;
	//Tmr.Enabled = true;
	//Tmr.Interval = TimerRate;
	//Tmr.Start();
}


/*
void CALLBACK TimerProcND(void* lpParametar, BOOLEAN TimerOrWaitFired)
{
	Node* obj = (Node*)lpParametar;
	obj->UpdateEvent();
}*/

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
	vector<TransactionSink> tsks;
	/*
	Node destNode = Constants.GlobalNodeList[Constants.random.Next(0, Constants.GlobalNodeList.Count)];

	if (destNode.PublicKey != PublicKey)
	{
	int Amount = Constants.random.Next(0, (int)(Money / 2));

	TransactionSink tsk = new TransactionSink(destNode.PublicKey, Amount);
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
	bool got = ledger->GetAccount(PublicKey, ai);

	if (got)
		return ai.Money;
	else return -1;
}


/// <summary>
/// [TO BE CALLED BY OTHER NODES] Sends transactions to destination, only valid ones will be processed.
/// </summary>
/// <param name="source"></param>
/// <param name="Transactions"></param>
void Node::SendTransaction(Hash source, TransactionContent Transaction)
{
	PendingIncomingTransactions.push(TransactionContentPack(source, Transaction));
	InTransactionCount++;
}

/// <summary>
/// [TO BE CALLED BY OTHER NODES] Sends candidates to destination [ONLY AFTER > 50% voting], only valid ones will be processed.
/// </summary>
/// <param name="source"></param>
/// <param name="Transactions"></param>
void Node::SendCandidates(Hash source, vector<TransactionContent> Transactions)
{
	if (TrustedNodes.count(source) > 0) // Is Trusted Node
	{
		for (int i = 0; i < (int)Transactions.size(); i++)
		{
			TransactionContent tc = Transactions[i];
			PendingIncomingCandidates.push(TransactionContentPack(source, tc));
			InCandidatesCount++;
		}
	}
}


void Node::Receive(NetworkPacket Packet)
{
	string _SZ = ", " + Packet.Data.size() + (string)" Bytes";
	string _OTH_MSG = " Received Packet :" + Packet.Type + (string)", From:" + Packet.PublicKey_Src.ToString() + _SZ;
	string MSG = (string)"[" + PublicKey.ToString() + "] : " + _OTH_MSG;

	MessageQueue.push(MSG);
}