#ifndef RPCKeyExchange_H
#define RPCKeyExchange_H

#include <cpprest/http_listener.h>
#include <cpprest/http_client.h>
#include <cpprest/json.h>
#include "ed25519\ed25519.h"
#include "ed25519\sha512.h"
	
using namespace web;
using namespace web::http;
using namespace web::http::experimental::listener;
using namespace web::json;
using namespace std;

class RPCKeyExchange
{
private:

	unsigned char publicKey[32];
	unsigned char privateKey[64];
	unsigned char seed[32];
	unsigned char sharedKey[32];



public:
	RPCKeyExchange();
	~RPCKeyExchange();

	void generate_shared_secret(const vector<unsigned char>& other_public_key);

	void handleKetExchange(http_request request);
};


#endif