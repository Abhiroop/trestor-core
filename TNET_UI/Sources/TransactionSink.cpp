#include "Constants.h"
#include "TransactionSink.h"
#include "ProtocolPackager.h"

TransactionSink::TransactionSink(Hash _PublicKey_Sink, int64_t _Amount)
{
	PublicKey_Sink = _PublicKey_Sink;
	Amount = _Amount;
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
	if (PDTs.size() == 2)
	{
		vector<byte> acNo;
		if (ProtocolPackager::UnpackByteVector_s(PDTs[0], 0, Constants::KEYLEN_PUBLIC, acNo))
		{
			PublicKey_Sink = acNo;
		}
		
		int64_t unpackVal;
		if (ProtocolPackager::UnpackInt64(PDTs[1], 1, unpackVal)) {
			Amount = unpackVal;
		}
	}
}