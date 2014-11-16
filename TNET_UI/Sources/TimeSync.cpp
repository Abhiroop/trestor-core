#include"TimeSync.h"
#include"Utils.h"
#include <random>

typedef concurrent_hash_map<Hash, TimeStruct> HM;

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
		
		HM::accessor acc;

		if (!state.timeMap.find(acc, ValidatorPK))
		{
			TimeStruct ts;
			ts.sendTime = state.system_time;
			ts.receivedTime = 0;
			ts.TimeFromValidator = 0;
			ts.timeDifference = 0;
			ts.token = token;
			state.timeMap.insert(make_pair(ValidatorPK, ts));
		}
	}
}

/*if everything is ok then 0
else 1
*/
bool TimeSync::SetTime(Hash PublicKey, Hash token, int64_t time)
{
	HM::accessor acc;

	if (state.timeMap.find(acc, PublicKey))
	{
		TimeStruct* ts = &(acc->second);
		Hash _token = ts->token;
		if (token != _token)
			return false;

		ts->receivedTime = state.system_time;

		int64_t RTT_one_way = (state.system_time - ts->sendTime) / 2;
		int64_t RTT_corrrected_time = time + RTT_one_way;

		ts->TimeFromValidator = RTT_corrrected_time;
		ts->timeDifference = (RTT_corrrected_time - state.system_time);

		return true;
	}
	
	return false;
}

int64_t TimeSync::CalculateAvgTime()
{
	int64_t total_diff = 0;
	int counter = 0;
	for (HM::iterator it = state.timeMap.begin(); it != state.timeMap.end(); ++it)
	{
		++counter;
		Hash PK = it->first;
		TimeStruct* ts = &(it->second);

		total_diff += ts->timeDifference;
	}

	return (total_diff / counter);
}