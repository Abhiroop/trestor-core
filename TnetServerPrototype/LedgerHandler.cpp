
#include "LedgerHandler.h"

int LedgerHandler::transaction(string senderPublickey, string receiverPublicKey, int64_t transactionAmount)
{
	time_t rawtime;
	struct tm * timeinfo;

	time(&rawtime);
	timeinfo = localtime(&rawtime);
	string current_time = asctime(timeinfo);

	//check sender
	mtx.lock();
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

		q.nextRow();
	}
	if (!senderExists)
	{
		mtx.unlock();
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

		q.nextRow();
	}

	//if there is no receiver than make a new receiver account
	if (!receiverExists)
	{
		int64_t senderNewBalance = sender_balance - transactionAmount;

		CppSQLite3Statement stmt = global_db.compileStatement("UPDATE Ledger set Balance = @u1 where PublicKey = @u2");
		stmt.bind("@u1", senderNewBalance);
		stmt.bind("@u2", senderPublickey.c_str());
		int n_rows = stmt.execDML();

		if (n_rows == 1)
		{
			stmt.reset();
			stmt = global_db.compileStatement("insert into Ledger values(@u1,@u2,@u3,@u4,@u5,@u6);");
			stmt.bind("@u1", receiverPublicKey.c_str());
			stmt.bind("@u4", transactionAmount);

			//bt default user is unbblocked
			stmt.bind("@u5", 0);
			//get the current system time here
			stmt.bind("@u6", current_time.c_str());
			int upate_rows = stmt.execDML();
			
			//update transaction history
			stmt.reset();
			
			stmt = global_db.compileStatement("insert into TransactionHistory values(@u1,@u2,@u3,@u4,@u5,@u6);");
			stmt.bind("@u2", senderPublickey.c_str());
			stmt.bind("@u3", receiverPublicKey.c_str());
			stmt.bind("@u4", transactionAmount);	
			stmt.bind("@u5", current_time.c_str());
			//successful transaction
			stmt.bind("@u6", 1);
			mtx.unlock();
			return 1;
		}

		else
		{
			senderNewBalance += transactionAmount;
			stmt.reset();
			stmt = global_db.compileStatement("UPDATE Ledger set Balance = @u1 where PublicKey = @u2");
			stmt.bind("@u1", senderNewBalance);
			stmt.bind("@u2", senderPublickey.c_str());
			stmt.bind("@u6", current_time.c_str());

			int n_rows = stmt.execDML();
			
			//update transaction history
			stmt.reset();

			stmt = global_db.compileStatement("insert into TransactionHistory values(@u1,@u2,@u3,@u4,@u5,@u6);");
			stmt.bind("@u2", senderPublickey.c_str());
			stmt.bind("@u3", receiverPublicKey.c_str());
			stmt.bind("@u4", transactionAmount);
			stmt.bind("@u5", current_time.c_str());
			//unsuccessful transaction
			stmt.bind("@u6", 0);
			
			mtx.unlock();
			return 0;
		}
		mtx.unlock();
		return 0;
		
	}


	//update new account balances of sender and receiver
	else
	{
		int64_t senderNewBalance = sender_balance - transactionAmount;
		int64_t receiverNewBalance = receiver_balance + transactionAmount;

		//update sender
		CppSQLite3Statement stmt = global_db.compileStatement("UPDATE Ledger set Balance = @u1, LastTransaction = @u6 where PublicKey = @u2");
		stmt.bind("@u1", senderNewBalance);
		stmt.bind("@u2", senderPublickey.c_str());
		stmt.bind("@u6", current_time.c_str());

		int ex_row = stmt.execDML();


		//update receiver

		if (ex_row == 1)
		{
			stmt = global_db.compileStatement("UPDATE Ledger set Balance = @u1, LastTransaction = @u6 where PublicKey = @u2");
			
			stmt.bind("@u1", receiverNewBalance);
			stmt.bind("@u2", receiverPublicKey.c_str());
			stmt.bind("@u6", current_time.c_str());

			stmt.execDML();

			//update transaction history
			stmt.reset();

			stmt = global_db.compileStatement("insert into TransactionHistory values(@u1,@u2,@u3,@u4,@u5,@u6);");
			stmt.bind("@u2", senderPublickey.c_str());
			stmt.bind("@u3", receiverPublicKey.c_str());
			stmt.bind("@u4", transactionAmount);
			stmt.bind("@u5", current_time.c_str());
			//successful transaction
			stmt.bind("@u6", 1);

			mtx.unlock();
			return 1;
		}
		else
		{
			senderNewBalance += transactionAmount;
			stmt.reset();
			
			stmt = global_db.compileStatement("UPDATE Ledger set Balance = @u1, LastTransaction = @u6 where PublicKey = @u2");
			
			stmt.bind("@u1", senderNewBalance);
			stmt.bind("@u2", senderPublickey.c_str()); 
			stmt.bind("@u6", current_time.c_str());

			stmt.execDML();

			//update transaction history
			stmt.reset();

			stmt = global_db.compileStatement("insert into TransactionHistory values(@u1,@u2,@u3,@u4,@u5,@u6);");
			stmt.bind("@u2", senderPublickey.c_str());
			stmt.bind("@u3", receiverPublicKey.c_str());
			stmt.bind("@u4", transactionAmount);
			stmt.bind("@u5", current_time.c_str());
			//unsuccessful transaction
			stmt.bind("@u6", 0);
			mtx.unlock();
			return 0;
		}
		mtx.unlock();
		return 0;
		
	}
	mtx.unlock();
	return 0;
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