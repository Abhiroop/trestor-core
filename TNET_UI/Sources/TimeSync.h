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
	int64_t current_time;

	void SendTimeRequest();
	void SetTime(Hash publicKey, Hash token);
};

#endif