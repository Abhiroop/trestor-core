//
// @Author : Arpan Jati
// @Date: 22th Oct 2014
//

#ifndef State_H
#define State_H

#include "Hash.h"
#include "tbb/concurrent_vector.h"
#include "tbb/concurrent_hash_map.h"
#include <stdint.h>

using namespace tbb;

struct TimeStruct
{
	int64_t sendTime;
	int64_t receivedTime;
	int64_t TimeFromValidator;
	Hash token;
	int64_t timeDifference; //my time - other time
};

class State
{

public:

	State();

	Hash PublicKey;
	Hash PrivateKey;


	//Potential bokapatha warning!
	concurrent_vector<Hash> GlobalBlacklistedValidators;

	// Users sending bad transaction requests.
	concurrent_vector<Hash> GlobalBlacklistedUsers;
	
	concurrent_hash_map<Hash, TimeStruct> timeMap;


	concurrent_vector<Hash> ConnectedValidators;

	int64_t system_time, network_time;
};




#endif

