#include "Consensus.h"
#include "Constants.h"
#include "VoteType.h"


void Consensus::InsertValidatedTransactions(vector<TransactionContent> Transactions)
{

}
void Consensus::PublishIDsToVallidatorsForVote()
{
	auto x = TranasctionSetTree.TraverseNodesAndReturnLeaf();
	vector<shared_ptr<LeafDataType>> Transactionleaves = x;

	vector<VoteType> VoteToSent;

	for (vector<shared_ptr<LeafDataType>>::iterator it = Transactionleaves.begin(); it != Transactionleaves.end(); ++it)
	{
		shared_ptr<LeafDataType> temleaf = *it;
		LeafDataType uu = *temleaf;

		TransactionSetType* tst = reinterpret_cast<TransactionSetType*>(&uu);
		vector<TransactionIDInfo> IDInfo = tst->TranIDinfo;

		for (vector<TransactionIDInfo>::iterator it1 = IDInfo.begin(); it1 != IDInfo.end(); ++it1)
		{
			vector<Hash> voters = it1->VoterPublickeys;
			int VoterCount = voters.size();
			if (VoterCount >= (int)floor((Constants::VALIDATOR_COUNT - 1)* (0.6)))
			{
				//check for bad vote here
				VoteType vt(it1->TransactionID, true);
				VoteToSent.push_back(vt);
			}
		}
	}
}

void Consensus::PublishIDsToVallidatorsInitial()
{
	auto x = TranasctionSetTree.TraverseNodesAndReturnLeaf();
	vector<shared_ptr<LeafDataType>> Transactionleaves = x;

	vector<Hash> IDSetToSent;

	for (vector<shared_ptr<LeafDataType>>::iterator it = Transactionleaves.begin(); it != Transactionleaves.end(); ++it)
	{
		shared_ptr<LeafDataType> temleaf = *it;
		LeafDataType uu = *temleaf;

		TransactionSetType* tst = reinterpret_cast<TransactionSetType*>(&uu);
		vector<TransactionIDInfo> IDInfo = tst->TranIDinfo;

		for (vector<TransactionIDInfo>::iterator it1 = IDInfo.begin(); it1 != IDInfo.end(); ++it1)
		{
			if (it1->IsMine)
				IDSetToSent.push_back(it1->TransactionID);
		}
	}
}

vector<Hash> Consensus::GetTransactionIDsInSet()
{
	vector<Hash> a;
	return a;
}

vector<TransactionContent> Consensus::GetTransactionsForID(vector<Hash> ID, Hash ValidatorPublickey)
{
	vector<TransactionContent> a;
	return a;
}

// Item may delete here if one transaction in a chein is found to be double spending
void Consensus::CheckTransactionChain()
{

}

void Consensus::RefreshVotes()
{

}
