#ifndef Network_H
#define Network_H




#include "Network.h"

using namespace System;
using namespace System::Threading;
using namespace System::Net::Sockets;
using namespace System::Net;
using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Text;

#include "TCPClientData.h"

public ref class NetworkClient
{
	TcpClient^ tc = gcnew TcpClient();
	TcpListener^ tList = gcnew TcpListener(IPAddress::Any, 5050);
	Dictionary<String^, TCPClientData^>^ ConnDict = gcnew Dictionary<String^, TCPClientData^>();

public:

	NetworkClient();

	void MarshalString(String ^ s, string& os);

	void HandleClient(System::Object^ _TCD);

	void UpdateEvents();

};

#endif

