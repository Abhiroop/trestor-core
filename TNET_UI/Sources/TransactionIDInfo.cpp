

#include"TransactionIDInfo.h"

TransactionIDInfo::TransactionIDInfo()
{

}
TransactionIDInfo::TransactionIDInfo(Hash transactionID, vector<Hash> voterPublickeys, bool isMine)
{
	TransactionID = transactionID;
	VoterPublickeys = voterPublickeys;
	IsMine = isMine;
}