
#include "TransactionContent.h"
#include "ProtocolPackager.h"
#include "Constants.h"

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
	PublicKey_Source = Hash();
	Timestamp = 0;
	Destinations = vector<TransactionSink>();
	Signature = Hash();
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



///////////////////////////////////////////////////////////


/*
Hash PublicKey_Source;
int64_t Timestamp;
vector<TransactionSink> Destinations;
Hash Signature;
*/

vector<byte> TransactionContent::Serialize()
{
	vector<ProtocolDataType> PDTs;
	PDTs.push_back(*ProtocolPackager::Pack(PublicKey_Source, 0));
	PDTs.push_back(*ProtocolPackager::Pack(Timestamp, 1));

	for (vector<TransactionSink>::iterator it = Destinations.begin(); it != Destinations.end(); ++it)
	{
		PDTs.push_back(*ProtocolPackager::Pack((*it).Serialize(), 2));
	}

	PDTs.push_back(*ProtocolPackager::Pack(Signature, 3));

	return ProtocolPackager::PackRaw(PDTs);
}

/////////////////////////


void TransactionContent::Deserialize(vector<byte> Data)
{
	TransactionContent();

	vector<ProtocolDataType> PDTs = ProtocolPackager::UnPackRaw(Data);
	int cnt = 0;

	while (cnt < (int)PDTs.size())
	{
		ProtocolDataType* PDT = &PDTs[cnt++];



		switch (PDT->NameType)
		{
		case 0:
			ProtocolPackager::UnpackByteVector_s(*PDT, 0, Constants::KEYLEN_PUBLIC, PublicKey_Source);
			break;

		case 1:
			ProtocolPackager::UnpackInt64(*PDT, 1, Timestamp);
			break;

		case 2:

		{
			vector<byte> tempVector;
			ProtocolPackager::UnpackByteVector(*PDT, 2, tempVector);
			if (tempVector.size() > 0)
			{
				TransactionSink tsk;
				tsk.Deserialize(tempVector);
				Destinations.push_back(tsk);
			}
		}

			break;

		case 3:
			ProtocolPackager::UnpackByteVector_s(*PDT, 3, Constants::KEYLEN_SIGNATURE, Signature);
			break;
		}
	}

}


