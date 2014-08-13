#include "MainForm.h"

Simulator sim;

int Value;

TimerX::Timer tmr = TimerX::Timer(1000, 1000, [](){

	Value++;

});
