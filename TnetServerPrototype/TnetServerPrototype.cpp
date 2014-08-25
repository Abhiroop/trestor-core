// TnetServerPrototype.cpp : main project file.

//#include "stdafx.h"
//
//#include "Network.h"
//
//#include "Constants2.h"
//
//using namespace System;
//using namespace Threading;
//
//shared_ptr<LedgerHandler> lH(new LedgerHandler);
//
//
//void ThreadProc(Object^ data)
//{
//	NetworkClient^ nc = gcnew NetworkClient(lH);
//
//	TimerCallback^ tcb = gcnew TimerCallback(nc, &NetworkClient::UpdateEvents);
//
//	Timer^ stateTimer = gcnew Timer(tcb, nullptr, 30, 30);
//
//	while (true)
//	{
//		Thread::Sleep(30);
//	}
//}
//
//int main(array<System::String ^> ^args)
//{
//	global_db.open(".\\db\\ledger.dat");
//
//    Console::WriteLine(L"Server Started ....");
//
//	ParameterizedThreadStart^ pst = gcnew ParameterizedThreadStart(ThreadProc);
//	Thread^ t = gcnew Thread(pst);
//	
//	t->Start();
//
//	t->Join();
//
//	Console::WriteLine(L"Server Stopped ....");
//
//	Console::ReadKey();
//
//
//    return 0;
//}
