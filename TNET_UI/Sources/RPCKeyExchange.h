#ifndef RPCKeyExchange_H
#define RPCKeyExchange_H

#include <cpprest/http_listener.h>
#include <cpprest/http_client.h>
#include <cpprest/json.h>
#include "ed25519\ed25519.h"
#include "ed25519\sha512.h"
#include "tbb/concurrent_hash_map.h"	
#include "Hash.h"
#include "KeyExchangeState.h"
#include"Utils.h"
#include "Base64_2.h"

using namespace web;
using namespace web::http;
using namespace web::http::experimental::listener;
using namespace web::json;
using namespace tbb;
using namespace std;


class RPCKeyExchange
{
private:

	unsigned char publicKey[32];
	unsigned char privateKey[64];
	unsigned char seed[32];
	unsigned char sharedKey[32];

	//other's session key and my extended private key
	concurrent_hash_map<Hash, KeyExchangeState> keyExchangeStateMap;
	//after the key exchange is done keep the other identity public key and the shared secret
	concurrent_hash_map<Hash, Hash> exchangedKey;

public:
	RPCKeyExchange();
	~RPCKeyExchange();

	void generate_shared_secret(unsigned char* other_public_key);

	web::json::value handleKetExchange(http_request request);
	web::json::value initExchange();

	void updateExchangedKey(Hash publicKey);
};


#endif