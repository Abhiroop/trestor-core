// TnetServerPrototype.cpp : main project file.

#include "stdafx.h"

#include "Network.h"

#include "Constants2.h"

using namespace System;

int main(array<System::String ^> ^args)
{

	global_db.open("D:\\work\\Trestor Foundation\\TNetUI\\TnetServerPrototype\\db\\ledger.dat");

    Console::WriteLine(L"Starting Server");

	NetworkClient^ nc = gcnew NetworkClient();

	while (true)
	{
		nc->UpdateEvents();
		Thread::Sleep(100);
	}


    return 0;
}
