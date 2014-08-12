
#include "PrivateKeyManage.h"
#include "Structs.h"
#include <hash_map>

class OnionNodeList
{
public:
	//call this multiple time to the servers to get onion intermediate nodes
	int getAndUpdateList(string server_ip, int port);
	//union multiple nodes to make one consolidate list of peers 
	int updateHMFromFile();

	int writeConsolodatedList();

	
};