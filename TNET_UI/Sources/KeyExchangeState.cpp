#include "KeyExchangeState.h"


KeyExchangeState::KeyExchangeState(int _state, Hash _privateKey)
{
	state = _state;
	privateKey = _privateKey;
}