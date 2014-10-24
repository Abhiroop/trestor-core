#include"TimeSync.h"
#include"Utils.h"
#include <random>

TimeSync::TimeSync(State _state)
{
	state = _state;
}


void TimeSync::SendTimeRequest()
{
	concurrent_vector<Hash> ConnectedValidators = state.ConnectedValidators;

	for (int i = 0; i < ConnectedValidators.size(); i++)
	{
		Hash ValidatorPK = ConnectedValidators.at(i);
		Hash token = GenerateNewToken();

		concurrent_hash_map<Hash, Hash>::accessor acc;

		if (!state.tokenPKMap.find(acc, token))
		{
			state.tokenPKMap.insert(make_pair(token, ValidatorPK));
		}
	}
}

void TimeSync::SetTime(Hash PublicKey, Hash token)
{
	concurrent_hash_map<Hash, Hash>::accessor acc;

	if (state.tokenPKMap.find(acc, token))
	{
		Hash PublicKeyInMap = acc->second;
	}
}