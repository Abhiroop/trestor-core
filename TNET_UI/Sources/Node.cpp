


#include "Node.h"


Ledger Node::getLocalLedger()
{
	return ledger;
}

Node::Node()
{

}


//Timer Tmr;

Node::Node(string Name, int _ConnectionLimit, Ledger & _ledger, long Money, int TimerRate)
{
	ConnectionLimit = ConnectionLimit;
	byte Seed[32];

	RandomFillBytes(Seed, 32);

	ed25519_create_seed(Seed);

	ed25519_create_keypair(_PublicKey, _PrivateKey, Seed);

	PublicKey = Hash(_PublicKey, _PublicKey + 32);

	ledger = _ledger;

	AI = AccountInfo(PublicKey, Money, Name, 0);

	ledger.AddUserToLedger(AI);

	

	//Tmr = new Timer();
	//Tmr.Elapsed += Tmr_Elapsed;
	//Tmr.Enabled = true;
	//Tmr.Interval = TimerRate;
	//Tmr.Start();
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
	bool got = ledger.GetAccount(PublicKey, ai);

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

}