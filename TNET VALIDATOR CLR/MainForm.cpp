#include "MainForm.h"

Simulator sim;

//#include "tbb/concurrent_hash_map.h"

//tbb::concurrent_hash_map<int, int> h;

int Value;

//tbb::concurrent_hash_map<int, int> h;

TimerX::Timer tmr = TimerX::Timer(1000, 1000, [](){

	Value++;

});

/*
void Callback()
{
	Value++;
}
*/
