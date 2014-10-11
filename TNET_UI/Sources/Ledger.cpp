
/*
*
*  @Author: Arpan Jati + Aritra Dhar
*  @Version: 1.0
*  @Date: August 2014
*/

#include "Ledger.h"


Ledger::Ledger()
{

	//LedgerTree;
	TransactionFees = 0;
	//TotalAmount = 0;
	//this.CloseTime = 0;
	//ledgerData = new LedgerData();
	//newCandidates;
}

bool Ledger::AddUserToLedger(AccountInfo userInfo)
{
	// MAKE sure account does not exist

	AccountInfo ltd;
	bool contains = LedgerTree.GetNodeData(userInfo.AccountID, ltd);

	if (!contains)
	{
		LedgerTree.AddUpdate(userInfo);
		return true;
	}
	else
	{
		return false;
	}
}

bool Ledger::GetAccount(Hash userInfo, AccountInfo & ltd)
{
	return LedgerTree.GetNodeData(userInfo, ltd);
}

bool Ledger::GetAccountBalance(Hash userInfo, int64_t & balance)
{
	AccountInfo ac;

	if (LedgerTree.GetNodeData(userInfo, ac))
	{
		balance = ac.Money;
		return true;
	}

	return false;
}

//void Ledger::PushNewProposedTransactions(vector<TransactionContent> proposedTransactions)
//{
//	for (int i = 0; i < (int)proposedTransactions.size(); i++)
//	{
//		TransactionContent tc = proposedTransactions[i];
//		Hash tHash = tc.GetHash();
//		if (newCandidates.count(tHash) == 0)
//		{
//			newCandidates[tHash] = tc;
//		}
//	}
//}

//hash_map<Hash, TransactionContent> Ledger::GetValidatedTransactions(hash_map<Hash, TransactionContent> Candidates)
//{
//	hash_map<Hash, TransactionContent> finalGoodTransactions;
//	hash_map<Hash, CreditDebitData> cdData;
//
//	LedgerOpStatistics stats;
//
//	for (hash_map<Hash, TransactionContent>::iterator _ts = Candidates.begin(); _ts != Candidates.end(); ++_ts)
//	{
//		//keys.push_back(iter->first);
//		TransactionContent ts = _ts->second;
//		Hash Source = ts.PublicKey_Source;
//
//		if (cdData.count(Source) == 0)
//		{
//			// This may cause exception if Source is not in ledgerData.
//			AccountInfo ai;
//			bool ret= LedgerTree.GetNodeData(Source, ai);
//
//			if (!ret)
//				throw exception("Could not find entry.");
//
//			CreditDebitData newdata(ai, ai.Money, 0, 0);
//			cdData[Source] = newdata;
//		}
//		bool FAILED_TRANSACTION = false;
//
//		for (int i = 0; i < (int)ts.Destinations.size(); i++)
//		{
//			TransactionEntity sink = ts.Destinations[i];
//			stats.TotalTransactions++;
//
//			if (cdData.count(sink.PublicKey_Sink) == 0)
//			{
//				//AccountInfo ai = (AccountInfo)account_tree.GetNodeData(sink.PublicKey_Sink);
//				// This may cause exception if Source is not in ledgerData.
//				//AccountInfo ai = (AccountInfo)LedgerTree.GetNodeData(sink.PublicKey_Sink);
//
//				AccountInfo ai;
//				bool ret = LedgerTree.GetNodeData(sink.PublicKey_Sink, ai);
//
//				if (!ret)
//					throw exception("Could not find entry.");
//
//
//				CreditDebitData cr(ai, ai.Money, 0, 0);
//				cdData[sink.PublicKey_Sink] = cr;
//			}
//
//			int64_t Deductible = sink.Amount;
//
//			int64_t Remaining = cdData[Source].Money - cdData[Source].Debits - Deductible;
//
//			if ((Remaining > 0) && (BlackList.count(sink.PublicKey_Sink) == 0) && (BlackList.count(Source) == 0))
//			{
//				cdData[Source].Debits += Deductible;
//				cdData[sink.PublicKey_Sink].Credits += Deductible;
//				stats.GoodTransactions++;
//
//				//DisplayUtils.Display("\t ::GOOD:: " + HexUtil.ToString(Source.Hex) + " : " + HexUtil.ToString(sink.PublicKey_Sink.Hex) + " : " + sink.Amount);
//			}
//			else
//			{
//				if (BlackList.count(Source) == 0)
//				{
//					stats.BlackLists++;
//					BlackList.insert(Source);
//				}
//
//				//DisplayUtils.Display("\t ::FAIL:: " + HexUtil.ToString(Source.Hex) + " : " + HexUtil.ToString(sink.PublicKey_Sink.Hex) + " : " + sink.Amount);
//				FAILED_TRANSACTION = true;
//				stats.FailedTransactions++;
//			}
//		}
//
//		if (!FAILED_TRANSACTION)
//		{
//			finalGoodTransactions[_ts->first] = _ts->second;
//		}
//	}
//	return finalGoodTransactions;
//}
//
//
///// ########################################################
/////         TEST VERSION / NEEDS RIGOROUS REVIEW
///// ########################################################
//
//LedgerOpStatistics Ledger::ApplyTransactionToLedger(hash_map<Hash, TransactionContent> Candidates)
//{
//	hash_map<Hash, CreditDebitData> cdData;
//	hash_set<Hash> BlackList;
//	
//	LedgerOpStatistics stats;	
//
//	for (hash_map<Hash, TransactionContent>::iterator _ts = Candidates.begin(); _ts != Candidates.end(); ++_ts)
//	{
//		TransactionContent ts = _ts->second;
//
//		Hash Source = ts.PublicKey_Source;
//
//		if (cdData.count(Source) == 0)
//		{
//			//AccountInfo ai = (AccountInfo)LedgerTree.GetNodeData(Source);
//
//			AccountInfo ai;
//			bool ret = LedgerTree.GetNodeData(Source, ai);
//
//			if (!ret)
//				throw exception("Could not find entry.");
//
//
//			CreditDebitData cr(ai, ai.Money, 0, 0);
//			cdData[Source] = cr;
//		}
//
//		for (int i = 0; i < (int)ts.Destinations.size(); i++)
//		{
//			stats.TotalTransactions++;
//			TransactionEntity sink = ts.Destinations[i];
//
//			if (cdData.count(sink.PublicKey_Sink) == 0)
//			{
//				//AccountInfo ai = (AccountInfo)LedgerTree.GetNodeData(sink.PublicKey_Sink);
//
//				AccountInfo ai;
//				bool ret = LedgerTree.GetNodeData(sink.PublicKey_Sink, ai);
//
//				if (!ret)
//					throw exception("Could not find entry.");
//
//				CreditDebitData cr(ai, ai.Money, 0, 0);
//				cdData[sink.PublicKey_Sink] = cr;
//				//AccountInfo ai = (AccountInfo)account_tree.GetNodeData(sink.PublicKey_Sink);
//				// This may cause exception if Source is not in ledgerData.
//			}
//
//			int64_t Deductible = sink.Amount;
//
//			int64_t Remaining = cdData[Source].Money - cdData[Source].Debits - Deductible;
//
//			if ((Remaining > 0) && (BlackList.count(sink.PublicKey_Sink) == 0) && (BlackList.count(Source) == 0))
//			{
//				cdData[Source].Debits += Deductible;
//				cdData[sink.PublicKey_Sink].Credits += Deductible;
//				stats.GoodTransactions++;
//
//				//DisplayUtils.Display("\t ::GOOD:: " + HexUtil.ToString(Source.Hex) + " : " + HexUtil.ToString(sink.PublicKey_Sink.Hex) + " : " + sink.Amount);
//			}
//			else
//			{
//				if (BlackList.count(Source) == 0)
//				{
//					stats.BlackLists++;
//					BlackList.insert(Source);
//				}
//
//				//DisplayUtils.Display("\t ::FAIL:: " + HexUtil.ToString(Source.Hex) + " : " + HexUtil.ToString(sink.PublicKey_Sink.Hex) + " : " + sink.Amount);
//
//				stats.FailedTransactions++;
//			}
//		}
//	}
//
//
//	if ((stats.GoodTransactions > 0) && (stats.FailedTransactions == 0))
//	{
//		for (hash_map<Hash, CreditDebitData>::iterator kvp = cdData.begin(); kvp != cdData.end(); ++kvp)
//		{
//			string name = "";
//			int64_t lastTransactionTime=0;
//			byte IsBlocked = 0;
//
//
//			AccountInfo ai(kvp->second.ai.AccountID, kvp->second.Money + kvp->second.Credits - kvp->second.Debits, name, IsBlocked, lastTransactionTime);
//			LedgerTree.AddUpdate(ai); // GetNodeData(kvp.Key);
//		}
//	}
//	else
//	{
//		stats.Response= false;
//	}
//
//	stats.Response = true;
//
//	return stats;
//}

void Ledger::RefreshValidTransactions()
{
	//newCandidates = GetValidatedTransactions(newCandidates);
}


/*
hash_map<Hash, TransactionContent> Ledger::getCandidates()
{
	return newCandidates;
}*/

Hash Ledger::GetRootHash()
{
	return LedgerTree.GetRootHash();
}