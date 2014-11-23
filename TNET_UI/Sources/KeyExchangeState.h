#ifndef KeyExchangeState_H
#define KeyExchangeState_H

#include "Hash.h"

class KeyExchangeState
{

public:
	int state;
	Hash privateKey;

	KeyExchangeState(int _state, Hash _privateKey);
};

#endif