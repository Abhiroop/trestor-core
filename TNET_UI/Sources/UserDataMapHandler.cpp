//#include "UserDataMapHandler.h"
//
//static void validation(string str)
//{
//	const char* un = str.c_str();
//	int counter = 0;
//	for (uint32_t i = 0; i < str.length(); i++)
//	{
//		if ((un[i] <= 57 && un[i] >= 48) || (un[i] <= 90 && un[i] >= 65) || (un[i] <= 122 && un[i] >= 97) || (un[i] == 46) || (un[i] == 95))
//			counter++;
//	}
//	if (counter <= 4 || counter != str.length())
//		throw exception("Invalid username format", 1);
//}
//
//int UserDataMapHandler::setUserDataMap(string username, string userdata, string* peer_list, int peer_count, string peer_list_signature)
//{
//	validation(username);
//
//	if (!global_db.tableExists("UserDataPeerMapTable"))
//		global_db.execDML("create table UserDataPeerMapTable (UserName varchar(1024) PRIMARY KEY, UserData varchar(1024)), PeerList varchar(1024), PeerListSignature varchar(1024);");
//	string qry = "select UserData from UserDataTable where UserName = '";
//	qry.append(username);
//	qry.append("';");
//	CppSQLite3Query q = global_db.execQuery(qry.c_str());
//
//	int row_counter = 0;
//	while (!q.eof())
//		throw exception("User already exists", 1);
//
//	//peerlist, : separated, ends with the peer nos
//	string cong_peer;
//	for (int i = 0; i < peer_count; i++)
//	{
//		cong_peer.append(peer_list[i]);
//		if (i < peer_count - 1)
//			cong_peer.append(":");
//	}
//	//string s_no = std::to_string(peer_count);
//	//cong_peer.append(s_no);
//
//	
//	CppSQLite3Statement stmt = global_db.compileStatement("insert into UserDataPeerMapTable values(@un, @ud, @pl, @pls);");
//	
//	stmt.bind("@un", username.c_str());
//	stmt.bind("@ud", userdata.c_str());
//	stmt.bind("@pl", cong_peer.c_str());
//	stmt.bind("@pls", peer_list_signature.c_str());
//
//	int n_rows = stmt.execDML();
//	stmt.reset();
//	return n_rows;
//}
//
//
///// FIX STUPID STRING* HACKS< MAY NOT WORK< JUST CI+OMPILES 
//string* UserDataMapHandler::getUserDataMap(string username)
//{
//	validation(username);
//
//	string qry = "select PeerList from UserDataPeerMapTable where UserName = '";
//	qry.append(username).append("';");
//	
//	string** out =  new string*;
//
//	CppSQLite3Query q = global_db.execQuery(qry.c_str());
//	char* output;
//
//	while (!q.eof())
//	{
//		string peer_list = q.fieldValue(0);
//
//		string delim = ":";
//		auto start = 0U;
//		auto end = peer_list.find(delim);
//		
//		int i = 0;
//		while (end != std::string::npos)
//		{
//			*out[i++] = peer_list.substr(start, end - start);
//			start = end + delim.length();
//			end = peer_list.find(delim, start);
//		}
//
//		return *out;
//
//	}
//	throw exception("No user user exists", 1);
//	return nullptr;
//}
//
//string UserDataMapHandler::getUserDataMapSignature(string username)
//{
//	validation(username);
//
//	string qry = "select PeerListSignature from UserDataPeerMapTable where UserName = '";
//	qry.append(username).append("';");
//
//	string* out;
//
//	CppSQLite3Query q = global_db.execQuery(qry.c_str());
//
//	while (!q.eof())
//	{
//		string peer_list_signature = q.fieldValue(0);
//		return peer_list_signature;
//	}
//	throw exception("No user user exists", 1);
//	return nullptr;
//}