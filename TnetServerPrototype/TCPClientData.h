#ifndef TCPClientData_H
#define TCPClientData_H

#include <vector>

using namespace std;
using namespace System;
using namespace System::Net::Sockets;
using namespace System::Net;

public ref class TCPClientData
{
public :

	TcpClient^ Tc = gcnew TcpClient();
	cli::array<Byte^>^ PublicKey;

	TCPClientData(TcpClient^ tc, cli::array<Byte^>^ publicKey);

	TCPClientData(TcpClient^ tc);

};



#endif