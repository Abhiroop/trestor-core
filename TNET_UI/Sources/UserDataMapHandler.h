#include "Base64.h"
#include "SQLiteCpp\CppSQLite3.h"
#include <stdio.h>
#include <stdint.h>
#include <string>
#include "PrivateKeyManage.h"
#include "Constants.h"

using namespace std;

class UserDataMapHandler
{

public:
	int setUserDataMap(string username, string userdata, string* peer_list, int peer_count, string peer_list_signature);
	string* getUserDataMap(string username);
	string getUserDataMapSignature(string username);
};