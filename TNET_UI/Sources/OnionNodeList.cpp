#include "OnionNodeList.h"
#include "Base64.h"
#include <vector>
#include <sstream>
#include <hash_map>

/*

hash_map<string, global_node_entry> global_hm;

vector<string> &split(const string &s, char delim, vector<string> &elems) {
	stringstream ss(s);
	string item;
	while (getline(ss, item, delim)) {
		elems.push_back(item);
	}
	return elems;
}

vector<string> split(const string &s, char delim) {
	vector<string> elems;
	split(s, delim, elems);
	return elems;
}

hash_map<string, global_node_entry> GetHM(std::istream* is)
{
	hash_map<string, global_node_entry> hs;

	string file_line;
	while (getline(*is, file_line))
	{
		vector<string> str = split(file_line, ',');
		if (str.size() == 4)
		{
			if ((hs.count(str[1].c_str()) == 0))
			{
				global_node_entry gne;
				
				gne.nodeType = (str[0][0] == 'S') ? NODE_SERVER : NODE_CLIENT;
				gne.peer_address_port = str[1].c_str();
				size_t olen;
				gne.peer_public_key = Base_64::decode(str[2].c_str(), str[2].length(), &olen);
				gne.reputation = atoi(str[3].c_str());

				hs[gne.peer_address_port] = gne;
			}
			else
			{
				printf("Node %s exists in the list..", str[1].c_str());
			}
		}
	}

	


	return hs;
}

int ConsolidateTpGlobalHM(hash_map<string, global_node_entry> hm)
{
	for (hash_map<string, global_node_entry>::iterator iterator = hm.begin(); iterator != hm.end(); iterator++) 
	{
		if (global_hm.count(iterator->first.c_str()) == 0) 
		{
			global_hm[iterator->first.c_str()] = iterator->second;
		}
	}

	return 0;
}

int OnionNodeList::updateHMFromFile()
{
	// C/S[0], IP:PORT[1], PK[32][2] (in Base64), reputation[3]
	
	ifstream  fin("userlist.csv");
	std::istream *is = dynamic_cast<std::istream *>(&fin);
	hash_map<string, global_node_entry> hm = GetHM(is);
	fin.close();
	

	ConsolidateTpGlobalHM(hm);

	return 0;
}


int OnionNodeList::getAndUpdateList(string server_ip, int port)
{
	//network call which gives a hashmap back
	char* respPeerList; // This is the reply from the peer, containg his latest user; 

	istringstream input(respPeerList);

	std::istream *is = dynamic_cast<std::istream *>(&input);
	hash_map<string, global_node_entry> hm = GetHM(is);

	ConsolidateTpGlobalHM(hm);

	//
	return 0;
}

int OnionNodeList::writeConsolodatedList()
{
	ofstream  fout("userlist2.csv", ios::out);
	for (hash_map<string, global_node_entry>::iterator iterator = global_hm.begin(); iterator != global_hm.end(); iterator++)
	{
		global_node_entry gne = iterator->second;
		size_t length = 0;
		const char* out_p = Base_64::encode(gne.peer_public_key, 112, &length);

		fout << (char)gne.nodeType << "," << gne.peer_address_port << "," << out_p << "," << gne.reputation << endl;
	}
	fout.close();

	return 0;
}


int main33()
{
	cout << "PUSHTEST 2.0" << endl;

	OnionNodeList onl;
	onl.updateHMFromFile();

	for (hash_map<string, global_node_entry>::iterator iterator = global_hm.begin(); iterator != global_hm.end(); iterator++)
	{
		global_node_entry gne = iterator->second;
		cout << "\n" << (char)gne.nodeType << "," << gne.peer_address_port << "," << gne.peer_public_key << "," << gne.reputation;
	}

	onl.writeConsolodatedList();

	getchar();

	return 0;

}

*/