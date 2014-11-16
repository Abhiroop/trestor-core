#ifndef TimeSync_H
#define TimeSync_H

#include"tbb\concurrent_hash_map.h"
#include "Hash.h"
#include"State.h"
#include<stdint.h>

using namespace tbb;

class TimeSync
{
public:
	State state;
	TimeSync(State state);

	void SendTimeRequest();
	
	/*if everything is ok the 0
	else 1
	*/
	bool SetTime(Hash publicKey, Hash token, int64_t time);

	int64_t CalculateAvgTime();
};

#endif