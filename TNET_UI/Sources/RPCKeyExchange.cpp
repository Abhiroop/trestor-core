#include"RPCKeyExchange.h"

#define TRACE(msg)            wcout << msg
#define TRACE_ACTION(a, k, v) wcout << a << L" (" << k << L", " << v << L")\n"


// this is to initiate a key exchange for a new session
//state 1
web::json::value RPCKeyExchange::initExchange()
{
	value jsonToSend;
	// initialte key exchange with state 1

	jsonToSend[L"state"] = json::value::number(1);
	
	unsigned char seed[32];
	unsigned char privateKey[32];
	unsigned char publicKey[32];
	unsigned char Token[32];

	ed25519_create_seed(seed);
	ed25519_create_seed(Token);

	Hash token(Token, Token + 32);

	ed25519_create_keypair(privateKey, publicKey, seed);

	string encoeToken = base64_encode_2((const char*)Token, 32);
	std::wstring wt;
	wt.assign(encoeToken.begin(), encoeToken.end());
	jsonToSend[L"token"] = json::value::string(wt);

	string encodedOwnPK = base64_encode_2((const char*)publicKey, 32);
	std::wstring ws;
	ws.assign(encodedOwnPK.begin(), encodedOwnPK.end());
	jsonToSend[L"sessionPublicKey"] = json::value::string(ws);

	concurrent_hash_map<Hash, KeyExchangeState>::accessor acc;
	if (!keyExchangeStateMap.find(acc, token))
	{
		Hash pk(publicKey, publicKey + 32);
		Hash sk(privateKey, privateKey + 32);
		
		KeyExchangeState KES(1, pk, sk);
		keyExchangeStateMap.insert(make_pair(token, KES));
	}
	else
	{
		keyExchangeStateMap.erase(token);

		Hash pk(publicKey, publicKey + 32);
		Hash sk(privateKey, privateKey + 32);

		KeyExchangeState KES(1, pk, sk);
		keyExchangeStateMap.insert(make_pair(token, KES));
	}

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
				int keyExchangeState = v.as_integer();

				switch (keyExchangeState)
				{
					//state 2
				case 1:
				{
						  bool flag = false;
						  unsigned char seed[32];
						  unsigned char ownSessionPk[32];
						  unsigned char ownSessionSk[32];
						  unsigned char sessionToken[32];
						  
						  unsigned char sharedSessionKey[32];
						  
						  if (jvalue.has_field(L"token"))
						  {
							  value token = jvalue[L"token"];
							  if (token.is_string())
							  {
								  string randomToken;
								  randomToken.assign(token.as_string().begin(), token.as_string().end());
								  string decodedRandomToken = base64_decode_2(randomToken);
								  
								  for (int i = 0; i < (int)decodedRandomToken.length(); i++)
								  {
									  sessionToken[i] = (byte) decodedRandomToken[i];
								  }
								  
								  flag = true;
							  }
						  }

						  if (!flag)
							  break;

						  if (jvalue.has_field(L"sessionPublicKey") || flag)
						  {
							  //generate keypair
							  ed25519_create_seed(seed);
							  ed25519_create_keypair(ownSessionPk, ownSessionSk, seed);

							  value pk = jvalue[L"sessionPublicKey"];
							  if (pk.is_string())
							  {
								  string publicKey;
								  publicKey.assign(pk.as_string().begin(), pk.as_string().end());
								  string decodedPK = base64_decode_2(publicKey);

								  if (decodedPK.size() != 32)
									  break;

								  //shared secret
								  byte b[32];
								  for (int i = 0; i < (int)decodedPK.size(); i++)
								  {
									  b[i] = (byte)decodedPK[i];
								  }

								  //generate shared secret here

								  unsigned char shared_secret[32];
								  unsigned char temp_hash[64];

								  ed25519_key_exchange(shared_secret, b, ownSessionSk);
								  sha512(shared_secret, 32, temp_hash);

								  memcpy(sharedSessionKey, temp_hash, 32);

								  // make a new jason object to send own public key

								  value jsonToSend;
								  jsonToSend[L"state"] = json::value::number(2);

								  string encodedOwnPK = base64_encode_2((const char*)ownSessionPk, 32);
								  std::wstring ws;
								  ws.assign(encodedOwnPK.begin(), encodedOwnPK.end());
								  jsonToSend[L"sessionPublicKey"] = json::value::string(ws);

								  unsigned char signedSessionPk[64];
								  ed25519_sign(signedSessionPk, ownSessionPk, 32, state.PublicKey.data(), state.PrivateKey.data());
								  string encodedSignature = base64_encode_2((const char*)signedSessionPk, 64);
								  std::wstring wsg;
								  wsg.assign(encodedSignature.begin(), encodedSignature.end());

								  string encodedOwnIdentityPK = base64_encode_2((const char*)state.PublicKey.data(), 32);
								  std::wstring woipk;
								  woipk.assign(encodedOwnIdentityPK.begin(), encodedOwnIdentityPK.end());
								  jsonToSend[L"publicKey"] = json::value::string(woipk);

								  jsonToSend[L"signature"] = json::value::string(wsg);

								  //register all data in the hashmap
								  concurrent_hash_map<Hash, KeyExchangeState>::accessor acc;
								  Hash token(sessionToken, sessionToken + 8);

								  if (!keyExchangeStateMap.find(acc, token))
								  {
									  Hash pk(ownSessionPk, ownSessionPk + 32);
									  Hash sk(ownSessionSk, ownSessionSk + 32);
									   
									  KeyExchangeState KES(2, sk, pk);

									  keyExchangeStateMap.insert(make_pair(token, KES));
								  }
								  //else it will ignore
								  //possiblity of attack

								  return jsonToSend;
							  }
						  }
				}

					//state 3
				case 2:
				{
						  unsigned char otherPublicKey[32];
						  unsigned char signature[64];
						  unsigned char otherSessionPublicKey[32];

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

								  //shared secret
								  for (int i = 0; i < (int)decodedPK.size(); i++)
								  {
									  otherPublicKey[i] = (byte)decodedPK[i];
								  }


							  }
						  }
				}


				}

			}


		}
	}
}

RPCKeyExchange::RPCKeyExchange(State _state)
{
	state = _state;
}

