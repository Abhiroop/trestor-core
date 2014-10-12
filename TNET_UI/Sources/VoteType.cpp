
#include "VoteType.h"
#include "ProtocolPackager.h"

VoteType::VoteType()
{

}

VoteType::VoteType(Hash transactionID, bool vote)
{
	TransactionID = transactionID;
	Vote = vote;
}

vector<byte> VoteType::Serialize()
{
	vector<ProtocolDataType> PDTs;
	PDTs.push_back(*ProtocolPackager::Pack(TransactionID, 0));
	PDTs.push_back(*ProtocolPackager::Pack(Vote, 1));
	return ProtocolPackager::PackRaw(PDTs);
}

void VoteType::Deserialize(vector<byte> Data)
{

}