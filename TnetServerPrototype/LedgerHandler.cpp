

#include "LedgerHandler.h"
#include "Constants.h"

int LedgerHandler::transaction(string senderPublickey, string receiverPublicKey, int64_t transactionAmount)
{
	//check sender
	string qry = "select Balance from Ledger where PublicKey = '";
	qry.append(senderPublickey);
	qry.append("';");
	CppSQLite3Query q = global_db.execQuery(qry.c_str());

	int64_t sender_balance = 0;
	bool senderExists = 0;
	while (!q.eof())
	{
		string baltemp = q.fieldValue(0);
		sender_balance = atoll(baltemp.c_str());
		
		senderExists = 1;

		if (sender_balance < transactionAmount)
			throw exception("Unsufficient Sender Balance", 1);
	}
	if (!senderExists)
	{
		throw exception("Sender Missing", 1);
		return -1;
	}


	//check receiver
	qry = "select Balance from Ledger where PublicKey = '";
	qry.append(receiverPublicKey);
	qry.append("';");
    q = global_db.execQuery(qry.c_str());

	int64_t receiver_balance = 0;
	bool receiverExists = 0;
	while (!q.eof())
	{
		string baltemp = q.fieldValue(0);
		receiver_balance = atoll(baltemp.c_str());

		receiverExists = 1;
	}

	//if there is no receiver than make a new receiver account
	if (!receiverExists)
	{
		CppSQLite3Statement stmt = global_db.compileStatement("insert into Ledger values(@u1,@u2,@u3,@u4,@u5,@u6);");
		stmt.bind("@u1", receiverPublicKey.c_str());
		stmt.bind("@u4", transactionAmount);
		
		//bt default user is unbblocked
		stmt.bind("@u5", 0);
		//get the current system time here
		stmt.bind("@u6", receiverPublicKey.c_str());
		int n_rows = stmt.execDML();
		stmt.reset();
		return 1;
	}
	//update new account balances of sender and receiver
	else
	{
		int64_t senderNewBalance = sender_balance + transactionAmount;
		int64_t receiverNewBalance = receiver_balance - transactionAmount;

		//update sender
		CppSQLite3Statement stmt = global_db.compileStatement("UPDATE Ledger set Balance = @u1 where PublicKey = @u2");
		stmt.bind("@u1", senderNewBalance);
		stmt.bind("@u2", senderPublickey.c_str());
		stmt.reset();

		//update receiver
		stmt = global_db.compileStatement("UPDATE Ledger set Balance = @u1 where PublicKey = @u2");
		stmt.bind("@u1", receiverNewBalance);
		stmt.bind("@u2", receiverPublicKey.c_str());
		stmt.reset();
		return 1;
	}

}


int64_t LedgerHandler::getBalance(string PublicKey)
{
	string qry = "select Balance from Ledger where PublicKey = '";
	qry.append(PublicKey);
	qry.append("';");
	CppSQLite3Query q = global_db.execQuery(qry.c_str());

	//int row_counter = 0;
	while (!q.eof())
	{
		string baltemp = q.fieldValue(0);
		int64_t balance = atoll(baltemp.c_str());

		return balance;
	}
	//if (row_counter == 0)
	throw exception("No such user exists", 1);

	return -1;
}