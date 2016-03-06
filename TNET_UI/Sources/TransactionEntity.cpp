#include "Constants.h"
#include "TransactionEntity.h"
#include "ProtocolPackager.h"

TransactionEntity::TransactionEntity(Hash _PublicKey, int64_t _Amount)
{
	PublicKey = _PublicKey;
	Amount = _Amount;
}

TransactionEntity::TransactionEntity()
{

}

vector<byte> TransactionEntity::Serialize()
{
	vector<ProtocolDataType> PDTs;
	PDTs.push_back(*ProtocolPackager::Pack(PublicKey, 0));
	PDTs.push_back(*ProtocolPackager::Pack(Amount, 1));
	return ProtocolPackager::PackRaw(PDTs);
}

void TransactionEntity::Deserialize(vector<byte> Data)
{
	vector<ProtocolDataType> PDTs = ProtocolPackager::UnPackRaw(Data);
	int cnt = 0;

	while (cnt < (int) PDTs.size())
	{
		ProtocolDataType* PDT = &PDTs[cnt++];
		
		switch (PDT->NameType)
		{
		case 0:
			ProtocolPackager::UnpackByteVector_s(*PDT, 0, Constants::KEYLEN_PUBLIC, PublicKey);
			break;

		case 1:
			ProtocolPackager::UnpackInt64(*PDT, 1, Amount);
			break;
		}
	}

}