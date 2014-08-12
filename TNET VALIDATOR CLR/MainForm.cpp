#include "MainForm.h"

int Value;

TimerX::Timer tmr = TimerX::Timer(1000, 1000, [](){

	Value++;

});

/*
void Callback()
{
	Value++;
}
*/
