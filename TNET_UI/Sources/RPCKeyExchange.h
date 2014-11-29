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
#include "State.h"

using namespace web;
using namespace web::http;
using namespace web::http::experimental::listener;
using namespace web::json;
using namespace tbb;
using namespace std;


class RPCKeyExchange
{
private:
	State state;
	//other's salt and my extended private key
	concurrent_hash_map<Hash, KeyExchangeState> keyExchangeStateMap;
	//after the key exchange is done keep the other identity public key and the shared secret
	concurrent_hash_map<Hash, Hash> exchangedKey;

public:

	RPCKeyExchange(State _state);

	web::json::value handleKetExchange(value jvalue);
	web::json::value initExchange();

};


#endif