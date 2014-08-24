
#include "LedgerHandler.h"
#include "BalanceType.h"
#include "Base64_2.h"
#include <mutex>

std::mutex mtx;



int LedgerHandler::transaction(string senderPublickey, string receiverPublicKey, int64_t transactionAmount, function<void(string)> transactionEvent)
{
	const __int64 current_time = time(0);

	//check sender
	mtx.lock();
	/*
	string qry = "select Balance from Ledger where PublicKey = '";
	qry.append(senderPublickey);
	qry.append("';");
	CppSQLite3Query q = global_db.execQuery(qry.c_str());
	*/

	CppSQLite3Statement stmt =global_db.compileStatement("select Balance from Ledger where PublicKey = @u1");
	stmt.bind("@u1", senderPublickey.c_str());
	CppSQLite3Query q = stmt.execQuery();


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
	/*
	qry = "select Balance from Ledger where PublicKey = '";
	qry.append(receiverPublicKey);
	qry.append("';");
    q = global_db.execQuery(qry.c_str());
	*/

	stmt.reset();

    stmt = global_db.compileStatement("select Balance from Ledger where PublicKey = @u1");
	stmt.bind("@u1", receiverPublicKey.c_str());
	q = stmt.execQuery();

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


BalanceType LedgerHandler::getBalance(string PublicKey, const __int64 queryTime, function<void(string)> transactionEvent)
{
	//make a balance type and modify it

	BalanceType balance_type;
	
	/*
	string qry = "select Balance from Ledger where PublicKey = '";
	qry.append(PublicKey);
	qry.append("';");
	CppSQLite3Query q = global_db.execQuery(qry.c_str());
	*/

	CppSQLite3Statement stmt = global_db.compileStatement("select Balance from Ledger where PublicKey = @u1");
	stmt.bind("@u1", PublicKey.c_str());
	CppSQLite3Query q = stmt.execQuery();

	//int row_counter = 0;
	while (!q.eof())
	{
		string baltemp = q.fieldValue(0);
		int64_t balance = atoll(baltemp.c_str());

		balance_type.setBalance(balance);

		//retrive history
		CppSQLite3Statement stmt = 
			global_db.compileStatement("select * from TransactionHistory where ( ((Sender = @u1) OR (Receiver = @u1)) AND (Time > @u2))");

		stmt.bind("@u1", PublicKey.c_str());
		stmt.bind("@u2", queryTime);

		CppSQLite3Query q = stmt.execQuery();

		vector<string> transactionHistory;
		while (!q.eof())
		{
			string tran;

			/*
			get string from database
			convert them to base64 format
			then append them, individual rows will be separated by :
			put them in a string vector
			*/
			string ID = q.fieldValue(0);
			string ID_64 = base64_encode_2(ID.c_str(), ID.length());
			
			tran = tran.append(ID_64);

			string Sender = q.fieldValue(1);
			string Sender_64 = base64_encode_2(Sender.c_str(), Sender.length());
			
			tran = tran.append(":").append(Sender_64);

			string Receiver = q.fieldValue(2);
			string Receiver_64 = base64_encode_2(Receiver.c_str(), Receiver.length());

			tran = tran.append(":").append(Receiver_64);

			string Amount = q.fieldValue(3);
			string Amount_64 = base64_encode_2(Amount.c_str(), Amount.length());

			tran = tran.append(":").append(Amount_64);

			string Time = q.fieldValue(4);
			string Time_64 = base64_encode_2(Time.c_str(), Time.length());

			tran = tran.append(":").append(Time_64);

			string IsSuccess = q.fieldValue(5);
			string IsSuccess_64 = base64_encode_2(IsSuccess.c_str(), IsSuccess.length());

			tran = tran.append(":").append(IsSuccess_64);

			transactionHistory.push_back(tran);

			q.nextRow();
		}
		//vector set
		balance_type.setTransactionHistory(transactionHistory);

		return balance_type;
	}
	transactionEvent("No such Sender exists");


	return balance_type;
}