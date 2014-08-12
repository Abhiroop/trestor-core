#include "Base64.h"
#include "SQLiteCpp\CppSQLite3.h"
#include<stdio.h>
#include "PrivateKeyManage.h"
#include "Constants.h"

#include <string>

using namespace std;

class UserDataHandler
{

public:
	int setData(string username, string userdata);
	string getData(string username);
};