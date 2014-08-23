

#include "TCPClientData.h"


TCPClientData::TCPClientData(TcpClient^ tc, cli::array<Byte^>^ publicKey)
{
	Tc = tc;
	PublicKey = publicKey;
}



TCPClientData::TCPClientData(TcpClient^ tc)
{
	Tc = tc;
}
