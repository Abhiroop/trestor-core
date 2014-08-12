//
//#include "LedgerFileHandler.h"
//#include <string>
//#include <hash_map>
//#include "Utils.h"
//#include "AccountInfo.h"
//#include "Constants.h"
//
//void LedgerFileHandler::loadledger()
//{
//	std::fstream ledgerStream;
//	ledgerStream.open(Const_LedgerFileName, ios::in | ios::out);// | ios::binary);	
//	//hash tree 
//		
//	//hash map for name-key binding
//
//	hash_map<string, Hash> name_key_map;
//
//	int line_counter = 0;
//	string line;
//
//	//string lcl_time, ledger_sequence, ledger_hash;
//	
//
//	while (std::getline(ledgerStream, line))
//	{
//		++line_counter;
//
//		//cout << line_counter << endl;
//		
//		if (line_counter == 1)
//			cout<< "LCL time : "<< atol(line.c_str())<< endl ;
//		
//		if (line_counter == 2)
//		{
//			LCLsequence = atol(line.c_str());
//		}
//		
//		//if (line_counter == 3)
//			//ledger_hash = line;
//	    
//		if (line_counter>=4)
//		{
//			Hash hash_vector;
//
//			string hash, name;
//			int64_t balance;
//			int64_t lastdate;
//
//			vector<string> tokens;
//			split(tokens, line, ' ');
//			
//			//super critial stuffs
//			hash = tokens[0].c_str();
//
//			balance = atoi(tokens[1].c_str());
//
//			name = tokens[2].c_str();
//
//			lastdate = atoi(tokens[3].c_str());
//			//hell yah!
//
//			for (int i = 0; i< (int)hash.length(); i++)
//				hash_vector.push_back((unsigned char)hash.at(i));
//			
//
//			name_key_map[name] = hash_vector;
//
//			//cout << hash << "  "<<balance<<" "<<name<< " " <<line_counter<<endl;
//			
//			AccountInfo newAcc = AccountInfo(hash_vector, balance, name, lastdate);
//			//ht.AddUpdate(newAcc);
//		}		
//	}
//	ledgerStream.close();
//	cout << "done   " <<line_counter<< endl;
//}
//
//
//void LedgerFileHandler::storeLedger()
//{
//	fstream ledgerStream;
//	ledgerStream.open(Const_LedgerFileName, ios::out);
//
//	//make ledger heardes
//	LCLtime = getCurrentUTFtime();
//	ledgerStream << LCLtime << "\n";
//
//	//Oh shit! genesis ledger Historial!!!
//	if (LCLsequence == 0)
//	{
//		LCLsequence = 1;
//		cout << "Genesis ledger detected" << endl << endl;
//		ledgerStream << LCLsequence << "\n";
//	}
//
//	//else some boring stuff to do
//	else
//	{
//		LCLsequence += 1;
//		ledgerStream << LCLsequence << "\n\n";
//	}
//
//	ledgerTree.TraverseNodesAndSave(ledgerStream);
//	ledgerStream.close();
//}
//
//
//LedgerFileHandler::LedgerFileHandler(HashTree< AccountInfo > accountTree)
//{
//	ledgerTree = accountTree;
//}