#include "SQLiteCpp\CppSQLite3.h"
#include <stdio.h>
#include "PrivateKeyManage.h"
#include "Base64.h"
#include "Structs.h"


class Onionpack
{
public:
	string makePack(string data, onion_peer_entry* peer_entries, int peer_count, string rec_addr, onion_peer_entry receiver);
};
