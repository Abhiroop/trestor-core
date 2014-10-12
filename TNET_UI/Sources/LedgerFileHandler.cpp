
//@Author : Aritra Dhar + Arpan Jati

#include "LedgerFileHandler.h"
#include <string>
#include <hash_map>
#include <functional>
#include "Utils.h"
#include "AccountInfo.h"
#include "Constants.h"
#include "LedgerRootInfo.h"


LedgerFileHandler::LedgerFileHandler()
{

}

LedgerFileHandler::LedgerFileHandler(HashTree< AccountInfo, LedgerRootInfo > accountTree, string LedgerDB_FileName)
{
	AccountTree = accountTree;
	ledger_db.open(LedgerDB_FileName.data());

	VerifyDatabase();
}

int LedgerFileHandler::VerifyDatabase()
{
	if (!ledger_db.tableExists("Ledger"))
	{
		try
		{
			ledger_db.execDML("create table Ledger (PublicKey blob PRIMARY KEY, UserName varchar(64), Balance integer, IsBlocked int, LastTransaction integer);");
		}

		catch (exception)
		{
			cout << "Problem in making new table";
			return 0;
		}
	}
	if (!ledger_db.tableExists("LedgerInfo"))
	{
		try
		{
			ledger_db.execDML("create table LedgerInfo (LedgerHash blob PRIMARY KEY, LastLedgerHash blob, LCLTime integer, SequenceNumber integer);");
		}

		catch (exception)
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

int LedgerFileHandler::UpdateLedgerInfo(Hash LedgerHash, Hash LastLedgerHash, int64_t LCLTime, int64_t SequenceNumber)
{
	VerifyDatabase();

	CppSQLite3Statement stmt = ledger_db.compileStatement("update LedgerInfo set LedgerHash = @u1, LastLedgerHash = @u2,LCLTime = @u3,SequenceNumber = @u4 where 1=1;");
	stmt.bind("@u1", LedgerHash.data(), LedgerHash.size());
	stmt.bind("@u2", LastLedgerHash.data(), LastLedgerHash.size());
	stmt.bind("@u3", LCLTime);
	stmt.bind("@u4", SequenceNumber);

	int row = stmt.execDML();

	if (row > 0)
		return 1;

	return 0;
}

int LedgerFileHandler::SaveToDB()
{
	VerifyDatabase();


	//bind(&Node::Receive, *NewNode, _1));

	LedgerRootInfo ledgerRootInfo = AccountTree.GetRootInfo();
	
	Hash LedgerHash = ledgerRootInfo.LedgerHash;
	Hash LastLedgerHash =  ledgerRootInfo.LastLedgerHash;
	int64_t LCLTime = ledgerRootInfo.LCLTime;
	int64_t SequenceNumber = ledgerRootInfo.SequenceNumber;

	/*
	CppSQLite3Statement stmt = ledger_db.compileStatement("DELETE FROM LedgerInfo");
	int rows = stmt.execDML();
	if (rows < 1)
	{
		cout << "problem in removing entry from LedgerInfo table";
	}

	stmt.reset();
	stmt = ledger_db.compileStatement("insert into LedgerInfo values(@u1,@u2,@u3,@u4);");
	stmt.bind("@u1", LedgerHash.data(), LedgerHash.size());
	stmt.bind("@u2", LastLedgerHash.data(), LastLedgerHash.size());
	stmt.bind("@u3", LCLTime);
	stmt.bind("@u4", SequenceNumber);
	*/
	UpdateLedgerInfo(LedgerHash, LastLedgerHash, LCLTime, SequenceNumber);

	ledger_db.execDML("BEGIN TRANSACTION");

	AccountTree.TraverseTreeAndFetch_do(bind(&LedgerFileHandler::SaveToDB_Callback, *this, std::placeholders::_1));

	ledger_db.execDML("END TRANSACTION");

	return 1;
}

int LedgerFileHandler::treeToDB(Hash accountID, int64_t money, string name, int64_t lastTransactionTime)
{		
	CppSQLite3Statement stmt = ledger_db.compileStatement("select PublicKey from Ledger where PublicKey = @u1");
	stmt.bind("@u1", accountID.data(), accountID.size());

	CppSQLite3Query q = stmt.execQuery();

	//int row_counter = 0;
	while (!q.eof())
	{
		string ret = q.fieldValue(0);
		stmt.reset();

		stmt = ledger_db.compileStatement("UPDATE Ledger SET UserName = @u2, Balance = @u3, LastTransaction = @u4 WHERE PublicKey = @u1; ");

		stmt.bind("@u1", accountID.data(), accountID.size());
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
	
	stmt.bind("@u1", accountID.data(), accountID.size());
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

HashTree< AccountInfo, LedgerRootInfo > LedgerFileHandler::DBToTree()
{
	//load ledger info
	CppSQLite3Statement stmt = ledger_db.compileStatement("select * from LedgerInfo");
	CppSQLite3Query q = stmt.execQuery();
	
	LedgerRootInfo _LRI;
	
	int counter = 0;
	while (!q.eof())
	{
		if (counter > 0)
			break;

		counter++;
		int len;
		unsigned char* lh = (unsigned char*) q.getBlobField("LedgerHash", len);
		Hash LedgerHash(lh, lh + len);

		lh = (unsigned char*)q.getBlobField("LastLedgerhash", len);
		Hash LastLedgerHash(lh, lh + len);

		int64_t LCLTime = q.getInt64Field("LCLTime");
		int64_t SequenceNumber = q.getInt64Field("SequenceNumber");

		LedgerRootInfo LRI(LedgerHash, LastLedgerHash, LCLTime, SequenceNumber);
		_LRI = LRI;
	}

	stmt.reset();
	stmt = ledger_db.compileStatement("select * from Ledger");
	q = stmt.execQuery();

	HashTree<AccountInfo, LedgerRootInfo> hashTree(_LRI);

	//int row_counter = 0;
	while (!q.eof())
	{
		int len;
		unsigned char* lh = (unsigned char*)q.getBlobField("PublicKey", len);
		Hash PublicKey(lh, lh + len);

		string UserName = q.fieldValue("UserName");
		int64_t Balance = q.getInt64Field("Balance");
		byte IsBlocked = q.getIntField("IsBlocked");
		int64_t LastTransaction = q.getInt64Field("LastTransaction");


		AccountInfo accountInfo = AccountInfo(PublicKey, Balance, UserName, IsBlocked, LastTransaction);
		hashTree.AddUpdate(accountInfo);
	}


	return hashTree;
	 
}
