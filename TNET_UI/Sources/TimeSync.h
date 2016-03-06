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
	/*
	update my time and others time here. 
	My time is the receiving time of the other time
	*/
	concurrent_hash_map<int64_t, int64_t> TimeMachine;

	State state;
	TimeSync(State state);

	void SendTimeRequest();
	
	/*if everything is ok the 0
	else 1
	*/
	bool SetTime(Hash publicKey, Hash token, int64_t time);

	int64_t GetGlobalAvgTime();

	int64_t CalculateAvgTime();
};

#endif