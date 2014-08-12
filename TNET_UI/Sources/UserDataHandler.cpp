//#include "UserDataHandler.h"
//
//using namespace std;
//
//CppSQLite3DB global_db;
//
////username input validation checking
//static void validation(string str)
//{
//	const char* un = str.c_str();
//	int counter = 0;
//	for (int i = 0; i < (int)str.length(); i++)
//	{
//		if ((un[i] <= 57 && un[i] >= 48) || (un[i] <= 90 && un[i] >= 65) || (un[i] <= 122 && un[i] >= 97) || (un[i] == 46) || (un[i] == 95))
//			counter++;
//	}
//	if (counter <= 4 || counter != str.length())
//		throw exception("Invalid username format", 1);
//}
//
//string  UserDataHandler::getData(string username)
//{
//	
//	validation(username);
//	string qry = "select UserData from UserDataTable where UserName = '"; 
//	qry.append(username);
//	qry.append("';");
//	CppSQLite3Query q = global_db.execQuery(qry.c_str());
//
//	//int row_counter = 0;
//	while (!q.eof())
//	{
//		string ret = q.fieldValue(0);
//		
//		return ret;
//	}
//	//if (row_counter == 0)
//	throw exception("No user user exists", 1);
//
//	return nullptr;
//}
//
//int  UserDataHandler::setData(string username, string userdata)
//{
//	
//	if (!global_db.tableExists("UserDataTable"))
//		global_db.execDML("create table UserDataTable (UserName varchar(1024) PRIMARY KEY, UserData varchar(1024));");
//	validation(username);
//
//	string qry = "select UserData from UserDataTable where UserName = '";
//	qry.append(username);
//	qry.append("';");
//	CppSQLite3Query q = global_db.execQuery(qry.c_str());
//
//	int row_counter = 0;
//	while (!q.eof())
//		throw exception("User already exists", 1);
//
//	CppSQLite3Statement stmt = global_db.compileStatement("insert into UserDataTable values(@un,@ud);");
//	stmt.bind("@un", username.c_str());
//	stmt.bind("@ud", userdata.c_str());
//	int n_rows = stmt.execDML();
//	stmt.reset();
//
//	return 0;
//}