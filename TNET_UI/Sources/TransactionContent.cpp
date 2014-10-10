
/*
*
*  @Author: Arpan Jati
*  @Version: 1.0
*  @Date: August 2014
*/

#include "TransactionContent.h"
#include "ProtocolPackager.h"
#include "Constants.h"
#include "AddressFactory.h"

TransactionContent::TransactionContent(vector<TransactionEntity> _Sources,
	int64_t _Timestamp, vector<TransactionEntity> _Destinations, vector<Hash> _Signatures)
{
	Destinations = _Destinations;
	Timestamp = _Timestamp;
	Sources = _Sources;
	Signatures = _Signatures;

	UpdateIntHash();
}

TransactionContent::TransactionContent()
{
	//PublicKey_Source = Hash();
	Timestamp = 0;
	Destinations = vector<TransactionEntity>();
	//Signature = Hash();
}

#define U64TO8_LITTLE(p, v) (((int64_t*)(p))[0] = v)

byte ConfigData[8] = { 0, 0, 0, 0, 0, 0, 0, 0 };

Hash TransactionContent::GetTransactionData()
{
	Hash _data;

	// Adding config Data
	vector<unsigned char> cd(ConfigData, ConfigData + 8);
	_data.insert(_data.end(), cd.begin(), cd.end());

	// Adding Sources
	for (int i = 0; i < (int)Sources.size(); i++)
	{
		TransactionEntity ts = Sources[i];

		byte temp1[8];
		((int64_t*)(temp1))[0] = ts.Amount;

		vector<unsigned char> temp2(temp1, temp1 + 8);
		
		_data.insert(_data.end(), ts.PublicKey.begin(), ts.PublicKey.end());
		_data.insert(_data.end(), temp2.begin(), temp2.end());
	}

	// Adding Destinations
	for (int i = 0; i < (int)Destinations.size(); i++)
	{
		TransactionEntity ts = Destinations[i];

		byte temp1[8];
		((int64_t*)(temp1))[0] = ts.Amount;

		vector<unsigned char> temp2(temp1, temp1 + 8);

		_data.insert(_data.end(), ts.PublicKey.begin(), ts.PublicKey.end());
		_data.insert(_data.end(), temp2.begin(), temp2.end());
	}

	// Adding Timestamp
	byte bp[8];
	((int64_t*)(bp))[0] = Timestamp;
	vector<unsigned char> ts(bp, bp + 8);

	_data.insert(_data.end(), ts.begin(), ts.end());

	return _data;
}

/*void TransactionContent::UpdateAndSignContent(vector<TransactionEntity> _Sources, int64_t _Timestamp, vector<TransactionEntity> _Destinations, vector<Hash> _ExpandedPrivateKeys)
{
	Destinations = _Destinations;
	Timestamp = _Timestamp;
	Sources = _Sources;

	byte temp_signature[64];
	Hash getTranData = GetTransactionData();
	ed25519_sign(temp_signature, getTranData.data(), getTranData.size(), PublicKey_Source.data(), _ExpandedPrivateKeys.data());
	Signature = Hash(temp_signature, temp_signature + 64);

	UpdateIntHash();
}*/

bool TransactionContent::IntegrityCheck()
{
	int64_t incoming = 0;
	int64_t outgoing = 0;

	for (int i = 0; i < (int)Sources.size(); i++)
	{
		incoming += Sources[i].Amount;
	}

	for (int i = 0; i < (int)Destinations.size(); i++)
	{
		outgoing += Destinations[i].Amount;
	}

	if ((incoming == outgoing) &&
		(incoming > 0) && 
		(Sources.size() > 0) && 
		(Destinations.size() > 0))
	{
		return true;
	}

	return false;
}

bool TransactionContent::VerifySignature()
{
	if (!IntegrityCheck())
	{
		return false;
	}

	Hash getTranData = GetTransactionData();
	
	if (Sources.size() == Signatures.size())
	{
		// Adding Sources

		int PassedSignatures = 0;

		for (int i = 0; i < (int)Sources.size(); i++)
		{
			TransactionEntity ts = Sources[i];

			bool good = ed25519_verify(Signatures[i].data(), getTranData.data(), getTranData.size(), Sources[i].PublicKey.data()) == 1 ? true : false;

			if (good)
			{
				PassedSignatures++;
			}
			else
			{
				return false;
			}
		}

		if (PassedSignatures == Sources.size())
		{
			return true;
		}
	}

	return false;
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
	return intHash;
}

Hash TransactionContent::GetTransactionDataAndSignature()
{
	Hash _data;
	// Data Hash Format : PK_SRC, TS, DESTS[PK_Sink,Amount], SIG
	Hash tranData = GetTransactionData();
	_data.insert(_data.end(), tranData.begin(), tranData.end());
	_data.insert(_data.end(), Signatures.begin(), Signatures.end());
	return _data;
}

///////////////////////////////////////////////////////////

/*
Hash PublicKey_Source;
int64_t Timestamp;
vector<TransactionEntity> Destinations;
Hash Signature;
*/

vector<byte> TransactionContent::Serialize()
{
	vector<ProtocolDataType> PDTs;
	/*PDTs.push_back(*ProtocolPackager::Pack(PublicKey_Source, 0));
	PDTs.push_back(*ProtocolPackager::Pack(Timestamp, 1));

	for (vector<TransactionEntity>::iterator it = Destinations.begin(); it != Destinations.end(); ++it)
	{
		PDTs.push_back(*ProtocolPackager::Pack((*it).Serialize(), 2));
	}

	PDTs.push_back(*ProtocolPackager::Pack(Signature, 3));*/

	return ProtocolPackager::PackRaw(PDTs);
}

/////////////////////////


void TransactionContent::Deserialize(vector<byte> Data)
{
	TransactionContent();

	/*vector<ProtocolDataType> PDTs = ProtocolPackager::UnPackRaw(Data);
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
					  TransactionEntity tsk;
					  tsk.Deserialize(tempVector);
					  Destinations.push_back(tsk);
				  }
		}

			break;

		case 3:
			ProtocolPackager::UnpackByteVector_s(*PDT, 3, Constants::KEYLEN_SIGNATURE, Signature);
			break;
		}
	}*/

}


