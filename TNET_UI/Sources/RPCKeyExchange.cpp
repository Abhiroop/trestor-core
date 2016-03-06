#include<iostream>
#include "Utils.h"
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

	Hash token(Token, Token + 8);

	ed25519_create_keypair(privateKey, publicKey, seed);

	string encoeToken = base64_encode_2((const char*)Token, 8);
	std::wstring wt;
	//wt.assign(encoeToken.begin(), encoeToken.end());
	wt = s2ws(encoeToken);


	jsonToSend[L"packetType"] = json::value::number(0x20);
	jsonToSend[L"token"] = json::value::string(wt);

	string encodedOwnPK = base64_encode_2((const char*)publicKey, 32);
	std::wstring ws;
	//ws.assign(encodedOwnPK.begin(), encodedOwnPK.end());
	ws = s2ws(encodedOwnPK);
	
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
web::json::value RPCKeyExchange::handleKetExchange(value jvalue)
{
	//value jvalue = request.extract_json().get();


	try
	{
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
							  unsigned char sessionToken[8];
							  string randomToken;
							  unsigned char sharedSessionKey[32];

							  if (jvalue.has_field(L"token"))
							  {
								  value token = jvalue[L"token"];
								  if (token.is_string())
								  {
									  wstring ws = token.as_string();
									 // randomToken.assign(token.as_string().begin(), token.as_string().end());
								
									  randomToken = ws2s(ws);

									  string decodedRandomToken = base64_decode_2(randomToken);


									  if (decodedRandomToken.size() != 8)
									  {
										  throw exception("Decoded token size is not 8 in JSON : state expected = 1");
									  }
									  for (int i = 0; i < (int)decodedRandomToken.length(); i++)
									  {
										  sessionToken[i] = (byte)decodedRandomToken[i];
									  }

									  flag = true;
								  }
							  }

							  if (!flag)
							  {
								  throw exception("No token included in JSON: state expected = 1");
								  
								  break;
							  }

							  if (jvalue.has_field(L"sessionPublicKey") && flag)
							  {
								  //generate keypair
								  ed25519_create_seed(seed);
								  ed25519_create_keypair(ownSessionPk, ownSessionSk, seed);

								  value pk = jvalue[L"sessionPublicKey"];
								  if (pk.is_string())
								  {
									  string publicKey;
									  //publicKey.assign(pk.as_string().begin(), pk.as_string().end());
									  publicKey = ws2s(pk.as_string());

									  string decodedPK = base64_decode_2(publicKey);

									  if (decodedPK.size() != 32)
									  {
										  throw exception("decoded public key size is not 32 in JSON : state expected = 1");
										  break;
									  }

									  //shared secret
									  byte otherPK[32];
									  for (int i = 0; i < (int)decodedPK.size(); i++)
									  {
										  otherPK[i] = (byte)decodedPK[i];
									  }

									  //generate shared secret here

									  unsigned char shared_secret[32];
									  unsigned char temp_hash[64];

									  ed25519_key_exchange(shared_secret, otherPK, ownSessionSk);
									  sha512(shared_secret, 32, temp_hash);

									  memcpy(sharedSessionKey, temp_hash, 32);

									  // make a new jason object to send own public key

									  value jsonToSend;

									  jsonToSend[L"packetType"] = json::value::number(0x20);

									  jsonToSend[L"state"] = json::value::number(2);

									  string encodedOwnPK = base64_encode_2((const char*)ownSessionPk, 32);
									  std::wstring ws;
									  //ws.assign(encodedOwnPK.begin(), encodedOwnPK.end());
									  ws = s2ws(encodedOwnPK);

									  jsonToSend[L"sessionPublicKey"] = json::value::string(ws);

									  unsigned char signedSessionPk[64];
									  ed25519_sign(signedSessionPk, ownSessionPk, 32, state.PublicKey.data(), state.PrivateKey.data());

									  string encodedSignature = base64_encode_2((const char*)signedSessionPk, 64);
									  std::wstring wsg;
									  //wsg.assign(encodedSignature.begin(), encodedSignature.end());
									  wsg = s2ws(encodedSignature);
									  
									  jsonToSend[L"signature"] = json::value::string(wsg);

									  string encodedOwnIdentityPK = base64_encode_2((const char*)state.PublicKey.data(), 32);
									  std::wstring woipk;
									  //woipk.assign(encodedOwnIdentityPK.begin(), encodedOwnIdentityPK.end());
									  woipk = s2ws(encodedOwnIdentityPK);
									  
									  jsonToSend[L"publicKey"] = json::value::string(woipk);

									  std::wstring wrt;
									  //wrt.assign(randomToken.begin(), randomToken.end());
									  wrt = s2ws(randomToken);
									  
									  jsonToSend[L"token"] = json::value::string(wrt);

									  //register all data in the hashmap
									  concurrent_hash_map<Hash, KeyExchangeState>::accessor acc;
									  Hash token(sessionToken, sessionToken + 8);

									  if (!keyExchangeStateMap.find(acc, token))
									  {
										  Hash pk(ownSessionPk, ownSessionPk + 32);
										  Hash sk(ownSessionSk, ownSessionSk + 32);

										  KeyExchangeState KES(2, sk, pk);
										  Hash sharedSecretHash(shared_secret, shared_secret + 32);
										  KES.sharedSecret = sharedSecretHash;

										  keyExchangeStateMap.insert(make_pair(token, KES));
									  }
									  //else it will ignore
									  //possiblity of attack

									  return jsonToSend;
								  }
							  }
							  else
							  {
								  throw exception("No session public key passed in the JSON : state expected = 1");
							  }
					}

						//state 3
					case 2:
					{
							  unsigned char otherPublicKey[32];
							  unsigned char signature[64];
							  unsigned char otherSessionPublicKey[32];
							  unsigned char token[8];
							  unsigned char sharedSessionKey[32];
							  string randomToken;

							  if (jvalue.has_field(L"token"))
							  {
								  value tk = jvalue[L"token"];

								  if (tk.is_string())
								  {

									  //randomToken.assign(tk.as_string().begin(), tk.as_string().end());
									  randomToken = ws2s(tk.as_string());
									  
									  string decodedTK = base64_decode_2(randomToken);

									  if (decodedTK.size() != 8)
									  {
										  throw exception("Decoded toke size is not 8 in JSON : state expected = 2");
										  break;
									  }

									  //other's public key
									  for (int i = 0; i < (int)decodedTK.size(); i++)
									  {
										  token[i] = (byte)decodedTK[i];
									  }
								  }
							  }

							  if (jvalue.has_field(L"publicKey"))
							  {
								  value pk = jvalue[L"publicKey"];

								  if (pk.is_string())
								  {
									  string publicKey;
									  //publicKey.assign(pk.as_string().begin(), pk.as_string().end());
									  publicKey = ws2s(pk.as_string());
									  
									  string decodedPK = base64_decode_2(publicKey);

									  if (decodedPK.size() != 32)
									  {
										  throw exception("Decoded public key size is not 32 in JSON : state expected = 2");
										  break;
									  }

									  //other's public key
									  for (int i = 0; i < (int)decodedPK.size(); i++)
									  {
										  otherPublicKey[i] = (byte)decodedPK[i];
									  }
								  }
							  }

							  if (jvalue.has_field(L"sessionPublicKey"))
							  {
								  value spk = jvalue[L"sessionPublicKey"];

								  if (spk.is_string())
								  {
									  string sessionPublicKey;
									  //sessionPublicKey.assign(spk.as_string().begin(), spk.as_string().end());
									  sessionPublicKey = ws2s(spk.as_string());

									  string decodedSPK = base64_decode_2(sessionPublicKey);

									  if (decodedSPK.size() != 32)
									  {
										  throw exception("Decoded session public key size is not 32 in JSON : state expected = 2");
										  break;
									  }

									  //other's session public key
									  for (int i = 0; i < (int)decodedSPK.size(); i++)
									  {
										  otherSessionPublicKey[i] = (byte)decodedSPK[i];
									  }
								  }
							  }

							  if (jvalue.has_field(L"signature"))
							  {
								  value sig = jvalue[L"signature"];

								  if (sig.is_string())
								  {
									  string signature;
									  //signature.assign(sig.as_string().begin(), sig.as_string().end());
									  signature = ws2s(sig.as_string());
									  
									  string decodedSIG = base64_decode_2(signature);

									  if (decodedSIG.size() != 64)
									  {
										  throw exception("Decoded signature size is not 64 in JSON : state expected = 2");
										  break;
									  }

									  //other's session public key
									  for (int i = 0; i < (int)decodedSIG.size(); i++)
									  {
										  signature[i] = (byte)decodedSIG[i];
									  }
								  }
								  //verify the signature

								  int ver = ed25519_verify(signature, otherSessionPublicKey, 32, otherPublicKey);

								  if (ver == 0)
								  {
									  throw exception("Signature is not verified : state expected = 2");
									  break;
								  }
							  }

							  //get own key from the token
							  concurrent_hash_map<Hash, KeyExchangeState>::accessor acc;
							  KeyExchangeState KES;

							  Hash tokenHash(token, token + 8);
							  if (keyExchangeStateMap.find(acc, tokenHash))
							  {
								  KES = acc->second;
							  }

							  else
							  {
								  throw exception("Token not found in the hash map : state expected");
								  break;
							  }


							  //generate shared secret here

							  unsigned char shared_secret[32];
							  unsigned char temp_hash[64];

							  ed25519_key_exchange(shared_secret, otherPublicKey, KES.privateKey.data());
							  sha512(shared_secret, 32, temp_hash);

							  memcpy(sharedSessionKey, temp_hash, 32);


							  //make the json object and send own identity public key along withh a signature
							  //also do a wrap up
							  Hash sharedSessionKeyHash(sharedSessionKey, sharedSessionKey + 32);
							  Hash otherPublicKeyHash(otherPublicKey, otherPublicKey + 32);
							  //delete
							  keyExchangeStateMap.erase(tokenHash);

							  concurrent_hash_map<Hash, Hash>::accessor acc1;
							  //entry
							  if (!exchangedKey.find(acc1, tokenHash))
							  {
								  exchangedKey.insert(make_pair(otherPublicKeyHash, sharedSessionKeyHash));
							  }
							  //else ignore

							  //json object
							  value jsonToSend;

							  jsonToSend[L"packetType"] = json::value::number(0x20);

							  jsonToSend[L"state"] = json::value::number(3);

							  std::wstring wrt;
							  //wrt.assign(randomToken.begin(), randomToken.end());
							  wrt = s2ws(randomToken);
							  
							  jsonToSend[L"token"] = json::value::string(wrt);

							  std::wstring woipk;
							  string encodedOwnIdentityPublicKey = base64_encode_2((const char*)state.PublicKey.data(), 32);
							  //woipk.assign(encodedOwnIdentityPublicKey.begin(), encodedOwnIdentityPublicKey.end());
							  woipk = s2ws(encodedOwnIdentityPublicKey);
							  
							  jsonToSend[L"publicKey"] = json::value::string(woipk);


							  unsigned char signedSessionPk[64];
							  ed25519_sign(signedSessionPk, KES.publicKey.data(), 32, state.PublicKey.data(), state.PrivateKey.data());

							  string encodedSignature = base64_encode_2((const char*)signedSessionPk, 64);
							  std::wstring wsg;
							  //wsg.assign(encodedSignature.begin(), encodedSignature.end());
							  wsg = s2ws(encodedSignature);
							  
							  jsonToSend[L"signature"] = json::value::string(wsg);

							  return jsonToSend;

					}

						//final state of the key exchange
						//wrap up all the data structures
					case 3:
					{
							  
							  unsigned char otherPublicKey[32];
							  unsigned char signature[64];
							  unsigned char otherSessionPublicKey[32];
							  unsigned char token[8];
							  string randomToken;


							  if (jvalue.has_field(L"token"))
							  {
								  value tk = jvalue[L"token"];

								  if (tk.is_string())
								  {

									  //randomToken.assign(tk.as_string().begin(), tk.as_string().end());
									  randomToken = ws2s(tk.as_string());
									  
									  string decodedTK = base64_decode_2(randomToken);

									  if (decodedTK.size() != 8)
									  {
										  throw exception("Decoded toke size is not 8 in JSON : state expected = 3");
										  break;
									  }

									  //other's public key
									  for (int i = 0; i < (int)decodedTK.size(); i++)
									  {
										  token[i] = (byte)decodedTK[i];
									  }
								  }
							  }

							  if (jvalue.has_field(L"publicKey"))
							  {
								  value pk = jvalue[L"publicKey"];

								  if (pk.is_string())
								  {
									  string publicKey;
									  //publicKey.assign(pk.as_string().begin(), pk.as_string().end());
									  publicKey = ws2s(pk.as_string());
									  
									  string decodedPK = base64_decode_2(publicKey);

									  if (decodedPK.size() != 32)
									  {
										  throw exception("Decoded public key size is not 32 in JSON : state expected = 3");
										  break;
									  }

									  //other's public key
									  for (int i = 0; i < (int)decodedPK.size(); i++)
									  {
										  otherPublicKey[i] = (byte)decodedPK[i];
									  }
								  }
							  }

							  if (jvalue.has_field(L"signature"))
							  {
								  value sig = jvalue[L"signature"];

								  if (sig.is_string())
								  {
									  string signature;
									  //signature.assign(sig.as_string().begin(), sig.as_string().end());
									  signature = ws2s(sig.as_string());
									  
									  string decodedSIG = base64_decode_2(signature);

									  if (decodedSIG.size() != 64)
									  {
										  throw exception("Decoded signature size is not 64 in JSON : state expected = 3");
										  break;
									  }

									  //other's session public key
									  for (int i = 0; i < (int)decodedSIG.size(); i++)
									  {
										  signature[i] = (byte)decodedSIG[i];
									  }
								  }
								  //verify the signature

								  int ver = ed25519_verify(signature, otherSessionPublicKey, 32, otherPublicKey);

								  if (ver == 0)
								  {
									  throw exception("Signature is not veerified : state expected = 3");
									  break;
								  }
							  }


							  //get own key from the token
							  concurrent_hash_map<Hash, KeyExchangeState>::accessor acc;
							  KeyExchangeState KES;

							  Hash tokenHash(token, token + 8);
							  if (keyExchangeStateMap.find(acc, tokenHash))
							  {
								  KES = acc->second;
							  }

							  else
							  {
								  throw  exception("Token not found in the database");
								  break;
							  }

							  //make the json object and send own identity public key along withh a signature
							  //also do a wrap up
							  //delete
							  keyExchangeStateMap.erase(tokenHash);
							  Hash otherPublicKeyHash(otherPublicKey, otherPublicKey + 32);

							  concurrent_hash_map<Hash, Hash>::accessor acc1;
							  //entry
							  if (!exchangedKey.find(acc1, tokenHash))
							  {
								  exchangedKey.insert(make_pair(otherPublicKeyHash, KES.sharedSecret));
							  }
							  //else ignore


							  value jsonToSend;
							  jsonToSend[L"packetType"] = json::value::number(0x20);
							  return jsonToSend;
					}


					default:
					{
							   throw exception("Violation happen in the keyexchange");
					}

					}

				}
			}

			else
			{
				cout << "KEY EXCHANGE COMPLETE";
			}
		}

		else
		{
			throw exception("null JSON value");
		}

	}

	catch (exception &e)
	{
		cout << "Exception : " << e.what() << endl;
		return NULL;
	}
}

RPCKeyExchange::RPCKeyExchange(State _state)
{
	state = _state;
}

