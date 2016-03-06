
#include <string>

using namespace std;

class BaseNetwork
{
	// This is to attach a callback function, whenever you receive any data for the specified 
	// ip_port { Example: "192.168.1.1:8080" }
	// you should call the pointed finction with the associated data and length
	// The data should be complete before this function is called
	// @param data : The received data, its binary, ISO8859-1
	// @param length : Length of data in bytes
	// @param ip_port : as described above.
	// @return : 0 if success, negative if failed, rest you think.
	int init_receive(int(*rec)(const char * data, int length, string ip_port));


	int send(unsigned char* data, int length, string ip_port);

	int disconnect(string ip_port);


};