#include "Constants.h"
#include "TransactionSink.h"
#include "ProtocolPackager.h"

TransactionSink::TransactionSink(Hash _PublicKey_Sink, int64_t _Amount)
{
	PublicKey_Sink = _PublicKey_Sink;
	Amount = _Amount;
}

TransactionSink::TransactionSink()
{

}

vector<byte> TransactionSink::Serialize()
{
	vector<ProtocolDataType> PDTs;
	PDTs.push_back(*ProtocolPackager::Pack(PublicKey_Sink, 0));
	PDTs.push_back(*ProtocolPackager::Pack(Amount, 1));
	return ProtocolPackager::PackRaw(PDTs);
}

void TransactionSink::Deserialize(vector<byte> Data)
{
	vector<ProtocolDataType> PDTs = ProtocolPackager::UnPackRaw(Data);
	int cnt = 0;

	while (cnt < (int) PDTs.size())
	{
		ProtocolDataType* PDT = &PDTs[cnt++];
		
		switch (PDT->NameType)
		{
		case 0:
			ProtocolPackager::UnpackByteVector_s(*PDT, 0, Constants::KEYLEN_PUBLIC, PublicKey_Sink);
			break;

		case 1:
			ProtocolPackager::UnpackInt64(*PDT, 1, Amount);
			break;
		}
	}

}