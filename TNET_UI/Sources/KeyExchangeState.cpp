#include "KeyExchangeState.h"


KeyExchangeState::KeyExchangeState(int _state, Hash _privateKey, Hash _publicKey)
{
	state = _state;
	privateKey = _privateKey;
	publicKey = _publicKey;
}