
// ARPAN MAIN

#include <vector>
#include <iostream>
#include <sstream>
#include <hash_map>
#include <winsock2.h>
#include <winsock.h>
#include <ws2tcpip.h>


#include "AccountInfo.h"
#include "ProtocolPackager.h"
#include "HashTree.h"
#include "Constants.h"
#include "ed25519\sha512.h"
#include "LedgerFileHandler.h"
#include "LedgerRootInfo.h"
#include"NTPClient.h"
#include "ConsensusMap.h"

#pragma comment(lib, "advapi32.lib")

#pragma comment(lib, "Ws2_32.lib")
	



using namespace std;

void dns_lookup(const char *host, sockaddr_in *out)
{
	struct addrinfo *result;
	int ret = getaddrinfo(host, "ntp", NULL, &result);
	for (struct addrinfo *p = result; p; p = p->ai_next)
	{
		if (p->ai_family != AF_INET)
			continue;

		memcpy(out, p->ai_addr, sizeof(*out));
	}
	freeaddrinfo(result);
}


void ntpdate() {
	char    *hostname = "time-a.nist.gov";//"tick.usno.navy.mil";
	int portno = 123;     //NTP is port 123
	int maxlen = 1024;        //check our buffers
	long i;          // misc var i
	char msg[48] = { 010, 0, 0, 0, 0, 0, 0, 0, 0 };    // the packet we send
	char  *buf = new char[1024]; // the buffer we get back
	//struct in_addr ipaddr;        //  
	struct protoent *proto;     //
	struct sockaddr_in server_addr;
	SOCKET s;  // socket
	time_t tmit;   // the time -- This is a time_t sort of


	//=====================================================================================
	//THIS IS WHAT IS MISSING MAJORILY  
	//=====================================================================================
	//Initialise the winsock stack
	WSADATA wsaData;
	BYTE wsMajorVersion = 1;
	BYTE wsMinorVersion = 1;
	WORD wVersionRequested = MAKEWORD(wsMinorVersion, wsMajorVersion);
	if (WSAStartup(wVersionRequested, &wsaData) != 0)
	{
		printf("Failed to load winsock stack\n");
		WSACleanup();
		return;
	}
	if (LOBYTE(wsaData.wVersion) != wsMajorVersion || HIBYTE(wsaData.wVersion) != wsMinorVersion)
	{
		printf("Winsock stack does not support version which this program requires\n");
		WSACleanup();
		return;
	}
	//=====================================================================================



	//use Socket;
	//
	//#we use the system call to open a UDP socket
	//socket(SOCKET, PF_INET, SOCK_DGRAM, getprotobyname("udp")) or die "socket: $!";
	proto = getprotobyname("udp");
	int err = GetLastError();
	s = socket(PF_INET, SOCK_DGRAM, proto->p_proto);
	if (s) {
		perror("asd");
		printf("socket=%d\n", s);
	}
	//
	//#convert hostname to ipaddress if needed
	//$ipaddr   = inet_aton($HOSTNAME);
	memset(&server_addr, 0, sizeof(server_addr));
	server_addr.sin_family = AF_INET;
	server_addr.sin_addr.s_addr = inet_addr(hostname);
	//argv[1] );
	//i   = inet_aton(hostname,&server_addr.sin_addr);
	server_addr.sin_port = htons(portno);
	//printf("ipaddr (in hex): %x\n",server_addr.sin_addr);

	/*
	* build a message.  Our message is all zeros except for a one in the
	* protocol version field
	* msg[] in binary is 00 001 000 00000000
	* it should be a total of 48 bytes long
	*/

	// send the data
	printf("sending data..\n");
	i = sendto(s, msg, sizeof(msg), 0, (struct sockaddr *)&server_addr, sizeof(server_addr));

	int iResult = -1;
	// Receive until the peer closes the connection
	//do {

	iResult = recv(s, buf, 1024, 0);
	if (iResult > 0)
		printf("Bytes received: %d\n", iResult);
	else if (iResult == 0)
		printf("Connection closed\n");
	else
		printf("recv failed: %d\n", WSAGetLastError());

	//} while( iResult > 0 );

	/*
	* The high word of transmit time is the 10th word we get back
	* tmit is the time in seconds not accounting for network delays which
	* should be way less than a second if this is a local NTP server
	*/

	tmit = ntohl((time_t)buf[10]);    //# get transmit time
	//printf("tmit=%d\n",tmit);

	/*
	* Convert time to unix standard time NTP is number of seconds since 0000
	* UT on 1 January 1900 unix time is seconds since 0000 UT on 1 January
	* 1970 There has been a trend to add a 2 leap seconds every 3 years.
	* Leap seconds are only an issue the last second of the month in June and
	* December if you don't try to set the clock then it can be ignored but
	* this is importaint to people who coordinate times with GPS clock sources.
	*/

	tmit -= 2208988800U;
	//printf("tmit=%d\n",tmit);
	/* use unix library function to show me the local time (it takes care
	* of timezone issues for both north and south of the equator and places
	* that do Summer time/ Daylight savings time.
	*/


	//#compare to system time
	printf("Time: %s", ctime(&tmit));
	i = time(0);
	//printf("%d-%d=%d\n",i,tmit,i-tmit);
	printf("System time is %d seconds off\n", i - tmit);
}


void GetTime()
{
	char *hostname = (char *)"time-a.nist.gov"; // NTP server
	int portno = 123;
		const int maxlen = 1024;
		int i;
		SOCKET s;
	char msg[48] = { 010, 0, 0, 0, 0, 0, 0, 0, 0 };  // Buffer for sending
	char  buf[maxlen];
	struct protoent *proto;
	struct sockaddr_in server_addr;
	time_t tmit;

	////////////////////////////////////////////////
	WSADATA wsaData;
	BYTE wsMajorVersion = 1;
	BYTE wsMinorVersion = 1;
	WORD wVersionRequested = MAKEWORD(wsMinorVersion, wsMajorVersion);
	if (WSAStartup(wVersionRequested, &wsaData) != 0)
	{
		printf("Failed to load winsock stack\n");
		WSACleanup();
		return;
	}
	if (LOBYTE(wsaData.wVersion) != wsMajorVersion || HIBYTE(wsaData.wVersion) != wsMinorVersion)
	{
		printf("Winsock stack does not support version which this program requires\n");
		WSACleanup();
		return;
	}
	////////////////////////////////////////////////////

	proto = getprotobyname("udp");
	s = socket(PF_INET, SOCK_DGRAM, proto->p_proto);

	memset(&server_addr, 0, sizeof(server_addr));
	server_addr.sin_family = AF_INET;
	server_addr.sin_addr.s_addr = inet_addr(hostname);
	server_addr.sin_port = htons(portno);
	i = sendto(s, msg, sizeof(msg), 0, (struct sockaddr *)&server_addr, sizeof(server_addr));
	struct sockaddr saddr;
	socklen_t saddr_l = sizeof (saddr);
	i = recvfrom(s, buf, 48, 0, &saddr, &saddr_l);
	tmit = ntohl((time_t)buf[4]);
	tmit -= 2208988800U;

	printf("Time: %s", ctime(&tmit));
	i = time(0);
	//printf("%d-%d=%d\n",i,tmit,i-tmit);
	printf("System time is %d seconds off\n", i - tmit);
}

int main()
{
	/*WSADATA wsaData;
	DWORD ret = WSAStartup(MAKEWORD(2, 0), &wsaData);

	char *host = "time-b.nist.gov"; // Don't distribute stuff pointing here, it's not polite. 
	//char *host = "time.nist.gov"; // This one's probably ok, but can get grumpy about request rates during debugging. 

	NTPMessage msg;
	
	msg.clear();
	msg.version = 3;
	msg.mode = 3 ;
	int i = 0;
	NTPMessage response;
	response.clear();

	int sock = socket(PF_INET, SOCK_DGRAM, IPPROTO_UDP);
	sockaddr_in srv_addr;
	memset(&srv_addr, 0, sizeof(srv_addr));
	dns_lookup(host, &srv_addr); 

	msg.sendto(sock, &srv_addr);
	response.recv(sock);

	time_t t = response.tx.to_time_t();
	char *s = ctime(&t);
	printf("The time is %s.", s);

	WSACleanup();

	*/

	//ntpdate();

	GetTime();

	cout << "TEST 3.0" << endl;

	HashTree< AccountInfo, LedgerRootInfo > ht, ht1;
	
	//lfh.loadledger();
	
	HCRYPTPROV hProvider = 0;

	if (!::CryptAcquireContextW(&hProvider, 0, 0, PROV_RSA_FULL, CRYPT_VERIFYCONTEXT | CRYPT_SILENT))
		return 1;

	byte PKS[32] = { 128, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
		0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF };

	Hash h_PKS = Hash(PKS, PKS + 32);


	vector<VoteType> votes;

	uint64_t Taka = 0;

	int MAX_ITR = 256*64;
	//for (int k = 0; k < 4; k++)
/*	for (int j = 0; j < 64; j++)
	for (int i = 0; i < 256; i++)
	{

		byte PK[32] = { i, j, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
			0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF };
				
		
		//byte PK[32];

		//if (!::CryptGenRandom(hProvider, 32, PK))
		//{
		//	::CryptReleaseContext(hProvider, 0);
		//	return 1;
		//}

		//long TAKA = 1000;

		Hash h = Hash(PK, PK + 32);

		VoteType vt(h, i % 2 == 0 ? true : false);

		votes.push_back(vt);

		h_PKS = h;

		string name = "a" + to_string(i) +"-"+ to_string(j);
		int64_t lastdate = 0;
		AccountInfo si = AccountInfo(h, Taka++, name, 1, lastdate);

		//Hash HH = si.GetHash();

		///for (int i = 0; i < 32; i++)
		//{
		//	printf("%02x, ", HH[i]);
		//}

		//Hash h3 = ht.GetRootHash();

		ht.AddUpdate(si);
		
	}
	*/

	ConsensusMap cm;

	for (int j = 0; j < 500; j++)
	{
		//byte PKS[32];

		byte PKS[32] = { j, 6, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
			0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF };

		//CryptGenRandom(hProvider, 32, PKS);		

		Hash hs = Hash(PKS, PKS + 32);

		for (int i = 0; i < 2; i++)
		{
			//byte PK[32];

			byte PK[32] = { 1 & 0xf, 5, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
				0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF };

			//CryptGenRandom(hProvider, 32, PK);
			
			Hash h = Hash(PK, PK + 32);

			VoteType vt(h, i % 2 == 0 ? true : false);

			votes.push_back(vt);

			//h_PKS = h;
		}

		cm.updateVote(votes, hs);

		votes.clear();

	}

	cout << "DONE";
	



	
	//vector<TreeLevelDataType> vc = ht.TraverseLevelOrderDepth(3);

	//cout << "Vector: " << vc.size()<<endl;

	/*vector<TreeSyncData> LSD = ht.TraverseLevelOrderDepthSync(3);

	vector<TreeSyncData> in;
	vector<AccountInfo> ac;

	for (int x = 0; x < MAX_ITR/100; x++)
	{
		TreeSyncData tmp(LSD[x], true);
		LSD.push_back(tmp);
	}
	
	ht.GetSyncNodeInfo(LSD, in, ac);
	cout << in.size() << endl << ac.size() << endl;
	
	for (int x = 0; x < ac.size(); x++)
	{
		cout << x << ": " << ac[x].AccountID.ToString() << ": " << ac[x].Name << endl;
	}*/




	/*
	for (int i = 0; i < 20; i++)
	{
		byte PK[32];

		if (!::CryptGenRandom(hProvider, 32, PK))
		{
			::CryptReleaseContext(hProvider, 0);
			return 1;
		}

		Hash h = Hash(PK, PK + 32);

		h_PKS = h;

		string name = "a";
		int64_t lastdate = 0;
		AccountInfo si = AccountInfo(h, Taka++, name, 1, lastdate);


		ht1.AddUpdate(si);

	}*/



	/*Open the ledger db file to instantiate
	*/

	/*
	ledger_db.open("LedgerT.db");
	

	LedgerFileHandler lh(ht);
	lh.MakeVerifyLedgerTree();

	lh.SaveToDB();

	//cout << "FCALL:" << LedgerFileHandler::fCall;

	vector<TreeLevelDataType> vc = ht.TraverseLevelOrderDepth(55);

	vector<ProtocolDataType> PDTs;


	byte search[] = { 0xA, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
		0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF };
	
	Hash h(search,search + 32);

	stack<TreeNodeX*> treeNodeStack, bp;
	ht.getStack_Itr(h, treeNodeStack);

	ht.DeleteData(h);

	ht.getStack_Itr(h, bp);

	for (int i = 0; i < (int)vc.size(); i++)
	{			
		PDTs.push_back(*ProtocolPackager::Pack(vc[i].address, 0));
		//PDTs.push_back(*ProtocolPackager::Pack(Vote, 1));
		vector<byte> dat =  ProtocolPackager::PackRaw(PDTs);

		cout << i << "Address : ";

		string s(vc[i].address.begin(), vc[i].address.end());

		cout << s << endl;

		cout << i << "HASH   ";

		for (int j = 0; j < 32; j++)
		{
			char aL = vc[i].treeNodeX->ID[j] & 0xF;
			char aH = (vc[i].treeNodeX->ID[j] >> 4) & 0xF;

			cout << Constants::hexChars[aH] << Constants::hexChars[aL] << "";
		}

		cout << endl << endl;
	}
	cout << vc.size() << endl;

	*/


	/*vector<TreeLevelDataType> hamba = ht.GetDifference(ht.TraverseLevelOrderDepth(2), ht1.TraverseLevelOrderDepth(2));


	for (int i = 0;i<hamba.size() ;i++)
	cout << "Hamba : " << i << ": " << string(hamba[i].address.begin(), hamba[i].address.end()) << endl;*/

	//
	//	LedgerFileHandler lfh = LedgerFileHandler(ht);
	//
	//	lfh.storeLedger();
	//
	//	//si.Money = 100000;
	//
	//	//ht.AddUpdate(si);
	//
	//	AccountInfo hh;
	//	ht.GetNodeData(h_PKS, hh);
	//
	//	printf("\n\n TOTAL MONEY ITERS: %d ", Taka);
	//
	//	printf("\n\n MONEY: %d ", hh.Money);
	//
	//	ht.TraverseNodes();
	//
	//	printf("\n\n TOTAL NODES: %lu ", ht.TotalNodes());
	//	printf("\n\n TOTAL MONEY: %llu ", ht.TotalMoney);
	//
	//
	//
	getchar();


	return 0;
}