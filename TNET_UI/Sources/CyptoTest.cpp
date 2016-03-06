//
//
//#include "SQLiteCpp\CppSQLite3.h"
//#include <stdio.h>
//#include "PrivateKeyManage.h"
//#include "Base64.h"
//#include "UserDataHandler.h"
//#include "Constants.h"
//
//using namespace std;
//
//CppSQLite3DB global_db;
////const char* dbFile = "D:\\work\\Trestor Foundation\\Trestor Crypto\\TrestorCrypto\\TrestorCrypto\\test.db";
//
//const char* byteToString(unsigned char* data, int length)
//{
//	char *ch = (char*)malloc(sizeof(char)*length);
//	for (int i = 0; i < length; i++)
//	{
//		int in = (int) data[i];
//		ch[i] = in;
//		//printf("%02x  ",data[i]);
//	}
//	printf("\n");
//	return ch;
//}
//
//unsigned char* stringToByte(const char* data, int length)
//{
//	unsigned char *byte = (unsigned char*)malloc(sizeof(unsigned char)*length);
//	for (int i = 0; i < length; i++)
//	{
//		int in = (int)*(data + i);
//		byte[i] = in;
//	}
//	return byte;
//}
//
//int main3()
//{
//	
//	PrivateKeyManage pkm = PrivateKeyManage();
//	pkm.SetPassword((unsigned char*)"1234",4);
//	pkm.prepareUserData();
//	
//	unsigned char ud[16 + 32 + 64];
//	pkm.getuserData(ud);
//
//	unsigned char pk[32], sk[64];
//	pkm.getPublicKey(pk);
//	pkm.getPrivateKey(sk);
//
//	PrivateKeyManage pkm1 = PrivateKeyManage(ud,(unsigned char*)"1234", 4);
//	unsigned char pk1[32], sk1[64];
//	pkm1.getPublicKey(pk1);
//	pkm1.getPrivateKey(sk1);
//
//	unsigned char* out;
//	out = stringToByte(byteToString(ud, 112), 112);
//
//	size_t length = 0;
//	const char* out_p = Base_64::encode(out,112,&length);
//
//	printf("%s\n", out_p);
//	printf("--------------------------------------------------------\n");
//
//	//for (int i = 0; i < 112; i++)
//		//printf("%02x %02x\n", ud[i],out[i]);
//
//	//printf("%s\n", byteToString(ud, 112));
//	printf("--------------------------------------------------------\n");
//
//	/////////////////////////////////////////////////////////////////////////////////////////////////////////////
//	
//	CppSQLite3Query q;
//
//	/*
//	cout << "SQLite Header Version: " << CppSQLite3DB::SQLiteHeaderVersion() << endl;
//	cout << "SQLite Library Version: " << CppSQLite3DB::SQLiteLibraryVersion() << endl;
//	cout << "SQLite Library Version Number: " << CppSQLite3DB::SQLiteLibraryVersionNumber() << endl;
//	*/
//
//	global_db.open(dbFile);
//	global_db.open(dbFile);
//
//	UserDataHandler udh;
//	try
//	{
//		//udh.setData("abcdefg", out_p)
//		printf("%s", udh.getData("Arpan").c_str());
//	}
//	catch (exception &e)
//	{
//		printf(e.what());
//	}
//
//
//	/*
//	if (!global_db.tableExists("UserDataTable"))
//	{
//		printf("table not exists\n");
//		global_db.execDML("create table UserDataTable (UserName varchar(1000) PRIMARY KEY, UserData varchar(1000));");
//		string s1 = "insert into UserDataTable values('aritra','";
//		const char* stmt = out_p;
//		s1.append(stmt);
//		string s2 = "');";
//		s1.append(s2);
//		
//		printf("%s\n", s1.c_str());
//
//		int nRows = global_db.execDML(s1.c_str());
//		nRows = global_db.execDML("insert into UserDataTable values('asd','1234');");
//	}
//
//	q = global_db.execQuery("select * from UserDataTable;");
//
//
//
//	while (!q.eof())
//	{
//		cout << q.fieldValue(0) << "|";
//		cout << q.fieldValue(1) << "|" << endl;
//		q.nextRow();
//	}
//	*/
//
//
//	getchar();
//	return 1;
//}