//
// @Author : Arpan Jati
// @Date: 22th Oct 2014
//

#ifndef State_H
#define State_H

#include "Hash.h"
#include "tbb/concurrent_vector.h"

using namespace tbb;

class State
{

public:

	State();

	//Potential bokapatha warning!
	concurrent_vector<Hash> GlobalBlacklistedValidators;

	// Users sending bad transaction requests.
	concurrent_vector<Hash> GlobalBlacklistedUsers;

};




#endif

