#ifndef MoneyInOutFlow_H
#define MoneyInOutFlow_H

#include<inttypes.h>
#include "Hash.h"

class MoneyInOutFlow
{
public:
	MoneyInOutFlow();
	MoneyInOutFlow(int64_t inFlow, int64_t outFlow);

	int64_t inFlow;
	int64_t outFlow;

};

#endif