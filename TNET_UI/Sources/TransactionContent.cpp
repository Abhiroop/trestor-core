
#include "TransactionContent.h"

TransactionContent::TransactionContent(Hash _PublicKey_Source, int64_t _Timestamp, vector<TransactionSink> _Destinations, Hash _Signature)
{
	Destinations = _Destinations;
	Timestamp = _Timestamp;
	PublicKey_Source = _PublicKey_Source;
	Signature = _Signature;

	UpdateIntHash();
}

TransactionContent::TransactionContent()
{

}
/*
Hash TransactionContent::getPublicKey_Source()
{
	return PublicKey_Source;
}

int64_t TransactionContent::getTimestamp()
{
	return Timestamp;
}

vector<TransactionSink> TransactionContent::getDestinations()
{
	return Destinations;
}

Hash TransactionContent::getSignature()
{
	return Signature;
}*/

#define U64TO8_LITTLE(p, v) (((int64_t*)(p))[0] = v)

Hash TransactionContent::GetTransactionData()
{
	Hash _data;

	byte bp[8];
	((int64_t*)(bp))[0] = Timestamp;

	vector<unsigned char> ts(bp, bp + 8);

	_data.insert(_data.end(), PublicKey_Source.begin(), PublicKey_Source.end());
	_data.insert(_data.end(), ts.begin(), ts.end());

	for (int i = 0; i < (int)Destinations.size(); i++)
	{
		TransactionSink ts = Destinations[i];

		byte temp1[8];
		((int64_t*)(temp1))[0] = ts.Amount;

		vector<unsigned char> temp2(temp1, temp1 + 8);

		_data.insert(_data.end(), ts.PublicKey_Sink.begin(), ts.PublicKey_Sink.end());
		_data.insert(_data.end(), temp2.begin(), temp2.end());
	}

	return _data;
}

void TransactionContent::UpdateAndSignContent(Hash _PublicKey_Source, int64_t _Timestamp, vector<TransactionSink> _Destinations, Hash _ExpandedPrivateKey)
{
	Destinations = _Destinations;
	Timestamp = _Timestamp;
	PublicKey_Source = _PublicKey_Source;

	byte temp_signature[64];
	Hash getTranData = GetTransactionData();
	ed25519_sign(temp_signature, getTranData.data(), getTranData.size(), PublicKey_Source.data(), _ExpandedPrivateKey.data());
	Signature = Hash(temp_signature, temp_signature + 64);

	UpdateIntHash();
}

void TransactionContent::UpdateIntHash()
{
	byte Hout[64];
	Hash tranDataSig = GetTransactionDataAndSignature();
	sha512(tranDataSig.data(), tranDataSig.size(), Hout);
	vector<byte> output(Hout, Hout + 32);

	intHash = output;
}

Hash TransactionContent::GetHash()
{
	return intHash;
}

Hash TransactionContent::GetID()
{
	return GetHash();
}

Hash TransactionContent::GetTransactionDataAndSignature()
{
	Hash _data;
	// Data Hash Format : PK_SRC, TS, DESTS[PK_Sink,Amount], SIG
	Hash tranData = GetTransactionData();
	_data.insert(_data.end(), tranData.begin(), tranData.end());
	_data.insert(_data.end(), Signature.begin(), Signature.end());
	return _data;
}