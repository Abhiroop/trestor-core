
#include "LedgerHandler.h"
#include "GetBalanceType.h"
#include <mutex>

std::mutex mtx;



int LedgerHandler::transaction(string senderPublickey, string receiverPublicKey, int64_t transactionAmount, function<void(string)> transactionEvent)
{
	const __int64 current_time = time(0);

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
			transactionEvent("Unsufficient Sender Balance");

		q.nextRow();
	}
	if (!senderExists)
	{
		mtx.unlock();
		transactionEvent("Sender Missing");
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
			stmt.bind("@u6", current_time);
			int upate_rows = stmt.execDML();
			
			//update transaction history
			stmt.reset();
			
			stmt = global_db.compileStatement("insert into TransactionHistory values(@u1,@u2,@u3,@u4,@u5,@u6);");
			stmt.bind("@u2", senderPublickey.c_str());
			stmt.bind("@u3", receiverPublicKey.c_str());
			stmt.bind("@u4", transactionAmount);	
			stmt.bind("@u5", current_time);
			//successful transaction
			stmt.bind("@u6", 1);
			//execute
			stmt.execDML();

			mtx.unlock();

			transactionEvent("Transaction success");

			return 1;
		}

		else
		{
			senderNewBalance += transactionAmount;
			stmt.reset();
			stmt = global_db.compileStatement("UPDATE Ledger set Balance = @u1 where PublicKey = @u2");
			stmt.bind("@u1", senderNewBalance);
			stmt.bind("@u2", senderPublickey.c_str());
			stmt.bind("@u6", current_time);

			int n_rows = stmt.execDML();
			
			//update transaction history
			stmt.reset();

			stmt = global_db.compileStatement("insert into TransactionHistory values(@u1,@u2,@u3,@u4,@u5,@u6);");
			stmt.bind("@u2", senderPublickey.c_str());
			stmt.bind("@u3", receiverPublicKey.c_str());
			stmt.bind("@u4", transactionAmount);
			stmt.bind("@u5", current_time);
			//unsuccessful transaction
			stmt.bind("@u6", 0);
			//execute
			stmt.execDML();
			mtx.unlock();

			transactionEvent("Transaction failure");

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
		stmt.bind("@u6", current_time);

		int ex_row = stmt.execDML();


		//update receiver

		if (ex_row == 1)
		{
			stmt = global_db.compileStatement("UPDATE Ledger set Balance = @u1, LastTransaction = @u6 where PublicKey = @u2");
			
			stmt.bind("@u1", receiverNewBalance);
			stmt.bind("@u2", receiverPublicKey.c_str());
			stmt.bind("@u6", current_time);

			stmt.execDML();

			//update transaction history
			stmt.reset();

			stmt = global_db.compileStatement("insert into TransactionHistory values(@u1,@u2,@u3,@u4,@u5,@u6);");
			stmt.bind("@u2", senderPublickey.c_str());
			stmt.bind("@u3", receiverPublicKey.c_str());
			stmt.bind("@u4", transactionAmount);
			stmt.bind("@u5", current_time);
			//successful transaction
			stmt.bind("@u6", 1);

			//execute
			stmt.execDML();

			mtx.unlock();

			transactionEvent("Transaction success");

			return 1;
		}
		else
		{
			senderNewBalance += transactionAmount;
			stmt.reset();
			
			stmt = global_db.compileStatement("UPDATE Ledger set Balance = @u1, LastTransaction = @u6 where PublicKey = @u2");
			
			stmt.bind("@u1", senderNewBalance);
			stmt.bind("@u2", senderPublickey.c_str()); 
			stmt.bind("@u6", current_time);

			stmt.execDML();

			//update transaction history
			stmt.reset();

			stmt = global_db.compileStatement("insert into TransactionHistory values(@u1,@u2,@u3,@u4,@u5,@u6);");
			stmt.bind("@u2", senderPublickey.c_str());
			stmt.bind("@u3", receiverPublicKey.c_str());
			stmt.bind("@u4", transactionAmount);
			stmt.bind("@u5", current_time);
			//unsuccessful transaction
			stmt.bind("@u6", 0);

			//execute
			stmt.execDML();

			mtx.unlock();

			transactionEvent("Transaction failure");

			return 0;
		}
		mtx.unlock();

		transactionEvent("Transaction failure");

		return 0;
		
	}
	mtx.unlock();
	transactionEvent("Transaction failure");

	return 0;
}


GetBalanceType LedgerHandler::getBalance(string PublicKey, const __int64 queryTime, function<void(string)> transactionEvent)
{
	//make a balance type and modify it

	GetBalanceType balance_type;

	string qry = "select Balance from Ledger where PublicKey = '";
	qry.append(PublicKey);
	qry.append("';");
	CppSQLite3Query q = global_db.execQuery(qry.c_str());

	//int row_counter = 0;
	while (!q.eof())
	{
		string baltemp = q.fieldValue(0);
		int64_t balance = atoll(baltemp.c_str());

		balance_type.setBalance(balance);

		//retrive history
		CppSQLite3Statement stmt = global_db.compileStatement("select * from TransactionHistory where ( ((Sender = @u1) OR (Receiver = @u1)) AND (Time > @u2))");
		stmt.bind("@u1", PublicKey.c_str());
		stmt.bind("@u2", queryTime);

		CppSQLite3Query q = stmt.execQuery();

		while (!q.eof())
		{
			string ID = q.fieldValue(0);
			//string sender = 

			q.nextRow();
		}
		return balance_type;
	}
	transactionEvent("No such Sender exists");


	return balance_type;
}