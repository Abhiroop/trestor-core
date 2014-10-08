//#include "Consensus.h"
//#include "Constants.h"
//#include "VoteType.h"
////#include "TransactionSetType.h"
//#include "AccountInfo.h"
//
//void Consensus::InsertValidatedTransactions(vector<TransactionContent> Transactions)
//{
//
//}
//
//bool Consensus::CheckSingleTransaction(TransactionSetType transactionSet)
//{
//	unique_ptr<Hash> ID (& transactionSet.GetID());
//	
//	concurrent_hash_map <Hash, AccountInfo>::accessor acc;
//	AccountInfo accInfo;
//	uint64_t accountBalance = 0;
//	uint64_t totalMoney = 0;
//	uint64_t loopCounter = 0;
//	
//	if (GLOBAL_LEDGER_MAP.find(acc, *ID))
//	{
//		accInfo = acc->second;
//		accountBalance = accInfo.Money;
//	}
//	else
//		return false;
//
//	concurrent_hash_map<Hash, TransactionContent>::accessor conHashMap_accr;
//
//	if (presentTransaction.find(conHashMap_accr, *ID))
//	{
//		vector<TransactionSink> Destinations = conHashMap_accr->second.Destinations;
//
//		
//		for (vector<TransactionSink>::iterator it = Destinations.begin(); it != Destinations.end(); ++it)
//		{
//			totalMoney += it->Amount;
//			loopCounter++;
//		}
//	}
//
//	if ( (totalMoney > (accountBalance - Constants::FIN_MIN_BALANCE)) 
//		&& (conHashMap_accr->second.Destinations.size() == loopCounter) 
//		&& (loopCounter > 0))
//	{
//		return true;
//	}
//
//	return false;
//
//}
//
////bool Consensus::AddUpdateTransactionTree(TransactionSetType transactionSet)
////{
////	if (CheckSingleTransaction(transactionSet))
////	{
////		//TranasctionSetTree.AddUpdate(transactionSet);
////		return true;
////	}
////	return false;
////}
////
////void Consensus::PublishIDsToVallidatorsForVote()
////{
////	auto x = TranasctionSetTree.TraverseNodesAndReturnLeaf();
////	vector<shared_ptr<LeafDataType>> Transactionleaves = x;
////
////	vector<VoteType> VoteToSent;
////
////	for (vector<shared_ptr<LeafDataType>>::iterator it = Transactionleaves.begin(); it != Transactionleaves.end(); ++it)
////	{
////		shared_ptr<LeafDataType> temleaf = *it;
////		LeafDataType uu = *temleaf;
////
////		TransactionSetType* tst = reinterpret_cast<TransactionSetType*>(&uu);
////		vector<TransactionIDInfo> IDInfo = tst->TranIDinfo;
////
////		for (vector<TransactionIDInfo>::iterator it1 = IDInfo.begin(); it1 != IDInfo.end(); ++it1)
////		{
////			vector<Hash> voters = it1->VoterPublickeys;
////			int VoterCount = voters.size();
////			if (VoterCount >= (int)floor((Constants::VALIDATOR_COUNT - 1)* (0.6)))
////			{
////				//check for bad vote here
////				VoteType vt(it1->TransactionID, true);
////				VoteToSent.push_back(vt);
////			}
////		}
////	}
////}
////
////void Consensus::PublishIDsToVallidatorsInitial()
////{
////	auto x = TranasctionSetTree.TraverseNodesAndReturnLeaf();
////	vector<shared_ptr<LeafDataType>> Transactionleaves = x;
////
////	vector<Hash> IDSetToSent;
////
////	concurrent_hash_map<Hash, bool>::accessor conHashMap_accr;
////
////	for (vector<shared_ptr<LeafDataType>>::iterator it = Transactionleaves.begin(); it != Transactionleaves.end(); ++it)
////	{
////		shared_ptr<LeafDataType> temleaf = *it;
////		LeafDataType uu = *temleaf;
////
////		TransactionSetType* tst = reinterpret_cast<TransactionSetType*>(&uu);
////		vector<TransactionIDInfo> IDInfo = tst->TranIDinfo;
////
////		for (vector<TransactionIDInfo>::iterator it1 = IDInfo.begin(); it1 != IDInfo.end(); ++it1)
////		{
////			Hash tranID = it1->TransactionID;
////			if (it1->IsMine && !forwardedTransaction.find(conHashMap_accr, tranID))
////			{
////				IDSetToSent.push_back(tranID);
////				forwardedTransaction.insert(make_pair(tranID, true));
////			}
////		}
////	}
////}
//
////check if we have the transaction packet
////If not it will call GetTransactionsForID to get the packets
//
//void Consensus::GetTransactionIDsInSet()
//{
//	concurrent_hash_map<Hash, TransactionContent>::accessor conHashMap_accr;
//
//	for (concurrent_hash_map<Hash, concurrent_hash_map<Hash, bool>>::iterator it = IncomingVotes.begin();
//		it != IncomingVotes.end(); ++it)
//	{
//		Hash incomingTranID = it->first;
//		if (!presentTransaction.find(conHashMap_accr, incomingTranID))
//		{
//			transactionContentRequest.push(incomingTranID);
//		}
//
//	}
//}
//
//vector<TransactionContent> Consensus::GetTransactionsForID(vector<Hash> ID, Hash ValidatorPublickey)
//{
//	//serarch the
//	return vector<TransactionContent>();
//}
//
//// Item may delete here if one transaction in a chein is found to be double spending
//void Consensus::CheckTransactionChain()
//{
//
//}
//
//void Consensus::RefreshVotes()
//{
//
//}
