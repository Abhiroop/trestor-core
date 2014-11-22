#include"RPCKeyExchange.h"

void RPCKeyExchange::handleKetExchange(http_request request)
{

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

void RPCKeyExchange::generate_shared_secret(const vector<unsigned char>& other_public_key) 
{
	unsigned char shared_secret[32];
	unsigned char temp_hash[64];

	ed25519_key_exchange(shared_secret, other_public_key.data(), privateKey);
	sha512(shared_secret, 32, temp_hash);

	memcpy(sharedKey, temp_hash, 32);

}