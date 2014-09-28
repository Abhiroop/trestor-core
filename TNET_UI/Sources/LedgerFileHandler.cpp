
//@Author : Aritra Dhar + Arpan Jati

#include "LedgerFileHandler.h"
#include <string>
#include <hash_map>
#include <functional>
#include "Utils.h"
#include "AccountInfo.h"
#include "Constants.h"
#include "LedgerRootInfo.h"


LedgerFileHandler::LedgerFileHandler(HashTree< AccountInfo > accountTree)
{
	AccountTree = accountTree;
	//fCall = 0;
}

int LedgerFileHandler::MakeVerifyLedgerTree()
{
	if (!ledger_db.tableExists("Ledger"))
	{
		try
		{
			ledger_db.execDML("create table Ledger (PublicKey varchar(128) PRIMARY KEY, UserName varchar(64), Balance integer, IsBlocked int, LastTransaction integer);");
			return 1;
		}

		catch (exception &ex)
		{
			cout << "Problem in making new table";
			return 0;
		}
	}
	return 1;
}

int LedgerFileHandler::SaveToDB_Callback(AccountInfo leaf)
{
	AccountInfo ai = (AccountInfo) leaf;

	//LedgerFileHandler::fCall++;

	//cout << "$$$$$$$$$$$$$$" << LedgerFileHandler::fCall << endl;
	
	return treeToDB(ai.AccountID, ai.Money, ai.Name, ai.LastTransactionTime);
}

int LedgerFileHandler::SaveToDB()
{
	//bind(&Node::Receive, *NewNode, _1));

	AccountTree.TraverseTreeAndFetch_do(bind(&LedgerFileHandler::SaveToDB_Callback, *this, std::placeholders::_1));

	return 1;
}

int LedgerFileHandler::treeToDB(Hash accountID, int64_t money, string name, int64_t lastTransactionTime)
{
	string AccountID = ToBase64String(accountID);
		
	CppSQLite3Statement stmt = ledger_db.compileStatement("select PublicKey from Ledger where PublicKey = @u1");
	stmt.bind("@u1", AccountID.c_str());


	CppSQLite3Query q = stmt.execQuery();

	//int row_counter = 0;
	while (!q.eof())
	{
		string ret = q.fieldValue(0);
		stmt.reset();

		stmt = ledger_db.compileStatement("UPDATE Ledger SET UserName = @u2, Balance = @u3, LastTransaction = @u4 WHERE PublicKey = @u1; ");

		stmt.bind("@u1", AccountID.c_str());
		stmt.bind("@u3", money);
		stmt.bind("@u2", name.c_str());
		stmt.bind("@u4", lastTransactionTime);

		int n_rows = stmt.execDML();
		if (n_rows != 1)
		{
			cout << "Problem in updating";
			return 0;
		}
		return 1;
	}
	stmt.reset();

	stmt = ledger_db.compileStatement("insert into Ledger values(@u1,@u2,@u3,@u4,@u5);");
	
	stmt.bind("@u1", AccountID.c_str());
	stmt.bind("@u3", money);
	stmt.bind("@u2", name.c_str());
	stmt.bind("@u4", 0);
	stmt.bind("@u5", lastTransactionTime);

	int n_rows = stmt.execDML();
	if (n_rows != 1)
	{
		cout << "Problem in inserting";
		return 0;
	}
	return 1;
}

/*
From scratch
*/

HashTree< AccountInfo > LedgerFileHandler::DBToTree()
{
	//load ledger info
	CppSQLite3Statement stmt = ledger_db.compileStatement("select * from LedgerInfo");
	CppSQLite3Query q = stmt.execQuery();

	while (!q.eof())
	{
		int len;
		unsigned char* lh = (unsigned char*) q.getBlobField("LedgerHash", len);
		Hash LedgerHash(lh, lh + len);

		lh = (unsigned char*)q.getBlobField("LastLedgerhash", len);
		Hash LastLedgerHash(lh, lh + len);

		int64_t LCLTime = q.getInt64Field("LCLTime");
		int64_t SequenceNumber = q.getInt64Field("SequenceNumber");

		LedgerRootInfo LRI(LedgerHash, LastLedgerHash, LCLTime, SequenceNumber);
	}

	stmt.reset();
	stmt = ledger_db.compileStatement("select * from Ledger");
	q = stmt.execQuery();

	HashTree<AccountInfo> hashTree;

	//int row_counter = 0;
	while (!q.eof())
	{
		string pkBase64 = q.fieldValue("PublicKey");
		Hash PublicKey = Base64ToHash(pkBase64);

		string UserName = q.fieldValue("UserName");
		int64_t Balance = q.getInt64Field("Balance");
		byte IsBlocked = q.getIntField("IsBlocked");
		int64_t LastTransaction = q.getInt64Field("LastTransaction");


		AccountInfo accountInfo = AccountInfo(PublicKey, Balance, UserName, IsBlocked, LastTransaction);
		hashTree.AddUpdate(accountInfo);
	}


	return hashTree;
	 
}
