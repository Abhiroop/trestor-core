#include"RPCKeyExchange.h"

#define TRACE(msg)            wcout << msg
#define TRACE_ACTION(a, k, v) wcout << a << L" (" << k << L", " << v << L")\n"


// this is to initiate a key exchange for a new session

web::json::value RPCKeyExchange::initExchange()
{
	value jsonToSend;
	// initialte key exchange with state 1

	jsonToSend[L"state"] = json::value::number(1);
	string encodedOwnPK = base64_encode_2((const char*)publicKey, 32);
	std::wstring ws;
	ws.assign(encodedOwnPK.begin(), encodedOwnPK.end());
	jsonToSend[L"publicKey"] = json::value::string(ws);

	return jsonToSend;
}

/*
after the key exchange initiated use this method
to continue with the rest of the key exchage stapes
to make sure both the parties have same shared secret
*/
web::json::value RPCKeyExchange::handleKetExchange(http_request request)
{
	value jvalue = request.extract_json().get();

	if (!jvalue.is_null())
	{
		if (jvalue.has_field(L"state"))
		{
			value v = jvalue[L"state"];
			if (v.is_integer())
			{
				int state = v.as_integer();

				switch (state)
				{
				case 1:
				{
						  if (jvalue.has_field(L"publicKey"))
						  {
							  value pk = jvalue[L"publicKey"];
							  if (pk.is_string())
							  {
								  string publicKey;
								  publicKey.assign(pk.as_string().begin(), pk.as_string().end());
								  string decodedPK = base64_decode_2(publicKey);

								  if (decodedPK.size() != 32)
									  break;

								  byte b[32];
								  for (int i = 0; i < (int)decodedPK.size(); i++)
								  {
									  b[i] = (byte)decodedPK[i];
								  }

								  generate_shared_secret(b);

								  // make a new jason object to send own public key

								  value jsonToSend;
								  jsonToSend[L"state"] = json::value::number(2);
								  string encodedOwnPK = base64_encode_2(publicKey.data(), 32);
								  std::wstring ws;
								  ws.assign(encodedOwnPK.begin(), encodedOwnPK.end());
								  jsonToSend[L"publicKey"] = json::value::string(ws);

								  return jsonToSend;
							  }
						  }
				}

				case 2:
				{

				}


				}

			}


		}
	}
}

RPCKeyExchange::RPCKeyExchange()
{
	memset(publicKey, 0, 32);
	memset(privateKey, 0, 64);
	memset(seed, 0, 32);
	memset(sharedKey, 0, 32);

	ed25519_create_seed(seed);
	ed25519_create_keypair(publicKey, privateKey, seed);
}

RPCKeyExchange::~RPCKeyExchange()
{
	memset(publicKey, 0, 32);
	memset(privateKey, 0, 64);
	memset(seed, 0, 32);
	memset(sharedKey, 0, 32);
}

void RPCKeyExchange::generate_shared_secret(unsigned char* other_public_key) 
{
	unsigned char shared_secret[32];
	unsigned char temp_hash[64];

	ed25519_key_exchange(shared_secret, other_public_key, privateKey);
	sha512(shared_secret, 32, temp_hash);

	memcpy(sharedKey, temp_hash, 32);

}

void RPCKeyExchange::updateExchangedKey(Hash publicKey)
{

}
