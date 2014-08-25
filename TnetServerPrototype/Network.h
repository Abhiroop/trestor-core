#ifndef Network_H
#define Network_H


using namespace System::Threading;
using namespace System::Net::Sockets;
using namespace System::Net;
using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Text;

#include "TCPClientData.h"
#include "LedgerHandler.h"
#include <memory>

public ref class NetworkClient
{
	TcpClient^ tc = gcnew TcpClient();
	TcpListener^ tList = gcnew TcpListener(IPAddress::Any, 5050);
	Dictionary<String^, TCPClientData^>^ ConnDict = gcnew Dictionary<String^, TCPClientData^>();
	
public:

	bool Updating = false;

	NetworkClient(/*shared_ptr<LedgerHandler> _lH*/);

	void MarshalString(String ^ s, string& os);

	void HandleClient(System::Object^ _TCD);

	void ReplyToClient(string s);

	void UpdateEvents(Object^ data);

	void InternalUpdate();

};

#endif

