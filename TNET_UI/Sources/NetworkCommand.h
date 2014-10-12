
// @Author : Arpan Jati
// @Date: 12th Oct 2014

#ifndef NetworkCommand_H
#define NetworkCommand_H

#include <stdint.h>
#include "Hash.h"

struct NetworkCommand
{
	// IP:PORT for the client
	string IPPort;

	// Public Key of the Sender
	vector <unsigned char> Sender;

	// Command
	string Command;

	// Data content of the request
	vector <unsigned char> Data;

	NetworkCommand(){};

	NetworkCommand(string ip_port, vector <unsigned char> sender, uint8_t command, vector <unsigned char> data)
	{
		IPPort = ip_port;
		Sender = Sender;
		Command = command;
		Data = data;
	}
};

#endif