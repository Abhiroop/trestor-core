//
// @Author : Arpan Jati
// @Date: 22th Oct 2014
//

#ifndef State_H
#define State_H

#include "Hash.h"
#include "tbb/concurrent_vector.h"
#include "tbb/concurrent_hash_map.h"
#include<stdint.h>

using namespace tbb;

class State
{

public:

	State();

	//Potential bokapatha warning!
	concurrent_vector<Hash> GlobalBlacklistedValidators;

	// Users sending bad transaction requests.
	concurrent_vector<Hash> GlobalBlacklistedUsers;
	
	concurrent_hash_map<Hash, int64_t> timeMap;
	concurrent_hash_map<Hash, Hash> tokenPKMap;

	concurrent_vector<Hash> ConnectedValidators;

	int64_t current_time;
};




#endif

