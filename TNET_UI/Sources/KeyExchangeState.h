#ifndef KeyExchangeState_H
#define KeyExchangeState_H

#include "Hash.h"

class KeyExchangeState
{

public:
	int state;
	Hash privateKey;
	Hash publicKey;
	Hash sharedSecret;

	KeyExchangeState(int _state, Hash _privateKey, Hash _publicKey);
};

#endif