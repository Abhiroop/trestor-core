// TnetServerPrototype.cpp : main project file.

#include "stdafx.h"

#include "Network.h"

#include "Constants2.h"

using namespace System;

shared_ptr<LedgerHandler> lH(new LedgerHandler);


int main(array<System::String ^> ^args)
{
	global_db.open(".\\db\\ledger.dat");

    Console::WriteLine(L"Server Started ....");

	NetworkClient^ nc = gcnew NetworkClient(lH);

	while (true)
	{
		nc->UpdateEvents();
		Thread::Sleep(30);
	}

    return 0;
}
