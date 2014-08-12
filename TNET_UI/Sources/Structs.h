#ifndef STRUCTS_H
#define STRUCTS_H
#include "PrivateKeyManage.h"
#include <string>

using namespace std;

typedef struct onion_peer_entry
{
	//preloaded
	unsigned char* peer_public_key;
	string peer_address_port;
	//filled by OnionConnectionEstablish class
	//unsigned char* peer_exchanged_key;// 32
	unsigned char* peer_transport_key;//32
	unsigned char* peer_authentication_key;//32

} onion_peer_entry;

enum NodeType { NODE_CLIENT='C', NODE_SERVER='S' };

typedef struct global_node_entry
{
	unsigned char* peer_public_key;
	string peer_address_port;
	int reputation;
	NodeType nodeType;
} global_peer_entry;


#endif