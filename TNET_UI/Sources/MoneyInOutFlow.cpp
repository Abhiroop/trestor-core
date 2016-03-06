#include"MoneyInOutFlow.h"

MoneyInOutFlow::MoneyInOutFlow()
{
	inFlow = 0;
	outFlow = 0;
}

MoneyInOutFlow::MoneyInOutFlow(int64_t _inFlow, int64_t _outFlow)
{
	inFlow = _inFlow;
	outFlow = _outFlow;
}