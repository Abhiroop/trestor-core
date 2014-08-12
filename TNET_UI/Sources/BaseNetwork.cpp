
#include "BaseNetwork.h"

//int (*receive)(const char *, int);

int BaseNetwork::init_receive(int(*rec)(const char * data, int length, string ip_port))
{

	return 0;
}


int BaseNetwork::send(unsigned char* data, int length, string ip_port)
{

	return 0;
}


int BaseNetwork::disconnect(string ip_port)
{

	return 0;
}