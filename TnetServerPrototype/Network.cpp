#include "Base64_2.h"
#include "Network.h"
#include "ProtocolPackager.h"

#include "TCPClientData.h"
#include "LedgerHandler.h"
#include "..\TNET_UI\Sources\TBB\tbb\concurrent_queue.h"

using namespace tbb;


struct MessageData
{
	string User;
	string Message;
	string MessageType;

	MessageData(){};

	MessageData(string user, string message, string messageType)
	{
		User = user;
		Message = message;
		MessageType = messageType;
	}
};

struct TransactionRequest
{
	// IP PORT for the client
	string User;

	// SenderPublic
	string Sender;

	// Receiver Public
	string Receiver;
	// Money / trests
	int64_t Money;

	// Signature
	string Signature;

	// Transaction Time
	int64_t Time;

	TransactionRequest(){};

	TransactionRequest(string user, string sender, string receiver, int64_t money, string signature, int64_t time)
	{
		user = user;
		Sender = Sender;
		Receiver = receiver;
		Money = money;
		Signature = signature;
		Time = time;
	}
};

struct CommandRequest
{
	// IP PORT for the client
	string User;

	// SenderPublic
	vector <unsigned char> Sender;

	// Command
	string Command;

	// Command
	vector <unsigned char> Data;

	CommandRequest(){};

	CommandRequest(string user, vector <unsigned char> sender, string command, vector <unsigned char> data)
	{
		User = user;
		Sender = Sender;
		Command = command;
		Data = data;
	}
};

//shared_ptr<LedgerHandler> lH2;

shared_ptr<LedgerHandler> lH2(new LedgerHandler);

concurrent_queue<MessageData> MessageQueue;
concurrent_queue<TransactionRequest> TransactionQueue;
concurrent_queue<CommandRequest> CommandQueue;

void transEvent(string user, string message)
{
	MessageQueue.push(MessageData(user, message, "TRANS_RESP"));
}

void balanceEvent(string user, string message)
{
	MessageQueue.push(MessageData(user, message, "BAL_RESP"));
}

NetworkClient::NetworkClient(/*shared_ptr<LedgerHandler> _lH*/)
{
	//lH2 = _lH;
	tList->Start();
}

void NetworkClient::ReplyToClient(string s)
{

}

void NetworkClient::MarshalString(String ^ s, string& os) {
	using namespace Runtime::InteropServices;
	const char* chars = (const char*)(Marshal::StringToHGlobalAnsi(s)).ToPointer();
	os = chars;
	Marshal::FreeHGlobal(IntPtr((void*)chars));
}

void NetworkClient::HandleClient(System::Object^ _TCD)
{
	TCPClientData^ TCD = (TCPClientData^)_TCD;

	StreamReader^ sr = gcnew StreamReader(TCD->Tc->GetStream());
	StreamWriter^ sw = gcnew StreamWriter(TCD->Tc->GetStream());

	while (TCD->Tc->Connected)
	{
		try
		{
			String^ data = sr->ReadLine();

			if (data == nullptr)
			{
				Console::WriteLine("User Disconnected : " + TCD->Tc->Client->RemoteEndPoint->ToString());
				break;
			}

			cli::array<String^>^ parts = data->Split('|');

			if (parts->Length == 2)
			{
				String^ FLAG = parts[0];
				String^ DATA = parts[1];

				String^ dd = Encoding::UTF8->GetString(Convert::FromBase64String(DATA));

				if (FLAG == "TRAN")
				{
					cli::array<String^>^ parts2 = dd->Split('|');

					if (parts2->Length == 3)
					{
						std::string SENDER_PK;
						std::string RECEIVER_PK;
						std::string CONNECTED_USER;

						MarshalString(parts2[0], SENDER_PK);
						MarshalString(parts2[1], RECEIVER_PK);
						MarshalString(TCD->Tc->Client->RemoteEndPoint->ToString(), CONNECTED_USER);

						int64_t MONEY = Int64::Parse(parts2[2]);

						MarshalString(parts2[1], RECEIVER_PK);

						TransactionRequest tr;
						tr.Sender = SENDER_PK;
						tr.Receiver = RECEIVER_PK;
						tr.Money = MONEY;
						tr.User = CONNECTED_USER;

						TransactionQueue.push(tr);
					}
				}
			}

			if (parts->Length == 2)
			{
				String^ FLAG = parts[0];
				String^ DATA = parts[1];

				if (FLAG == "COMMAND")
				{
					cli::array<unsigned char>^ dd = Convert::FromBase64String(DATA);

					std::vector<unsigned char> raw(dd->Length);
					System::Runtime::InteropServices::Marshal::Copy(dd, 0, IntPtr(&raw[0]), dd->Length);

					vector<ProtocolDataType> datas = ProtocolPackager::UnPackRaw(raw);

					vector <unsigned char> PK;
					std::string COMM;
					vector <unsigned char> DATA;
					std::string CONNECTED_USER;

					for each (ProtocolDataType var in datas)
					{
						if (var.NameType == 0)					
							ProtocolPackager::UnpackByteVector(var, 0, PK);							

						if (var.NameType == 1)						
							ProtocolPackager::UnpackString(var, 1, COMM);						

						if (var.NameType == 2)						
							ProtocolPackager::UnpackByteVector(var, 2, DATA);	
					}

					if (PK.size() == 32 && COMM.size() > 2)
					{
						MarshalString(TCD->Tc->Client->RemoteEndPoint->ToString(), CONNECTED_USER);

						CommandRequest cr;
						cr.Sender = PK;
						cr.User = CONNECTED_USER;
						cr.Command = COMM;
						cr.Data = DATA;

						CommandQueue.push(cr);
					}
				}
			}
		}
		catch (Exception^ ex)
		{
			Console::WriteLine("User Disconnected : " + TCD->Tc->Client->RemoteEndPoint->ToString() + " : " + ex->Message);
		}
	}

	Console::WriteLine("User Disconnected : " + TCD->Tc->Client->RemoteEndPoint->ToString());
}


void NetworkClient::InternalUpdate()
{
	Updating = true;

	while (tList->Pending())
	{
		TcpClient^ TC = tList->AcceptTcpClient();

		TC->ReceiveBufferSize = 64;
		TC->SendBufferSize = 64;

		TCPClientData^ TCD = gcnew TCPClientData(TC);
		ConnDict->Add(TC->Client->RemoteEndPoint->ToString(), TCD);
		ParameterizedThreadStart^ pts = gcnew ParameterizedThreadStart(this, &NetworkClient::HandleClient);
		Thread^ thr = gcnew Thread(pts);
		thr->Start(TCD);
	}

	MessageData md;
	while (MessageQueue.try_pop(md))
	{
		String ^ dest = gcnew String(md.User.c_str());
		String ^ msg = gcnew String(md.Message.c_str());
		String ^ type = gcnew String(md.MessageType.c_str());

		if (ConnDict->ContainsKey(dest))
		{
			StreamWriter ^ sw = gcnew StreamWriter(ConnDict[dest]->Tc->GetStream());

			sw->WriteLine(type + "|" + Convert::ToBase64String(Encoding::UTF8->GetBytes(msg)));
			sw->FlushAsync();

			Console::WriteLine(DateTime::Now.ToShortTimeString() + " | " + dest + ": " + type + " : " + msg);
		}
	}


	TransactionRequest tq;
	while (TransactionQueue.try_pop(tq))
	{
		String ^ user = gcnew String(tq.User.c_str());
		if (ConnDict->ContainsKey(user))
		{
			StreamWriter ^ sw = gcnew StreamWriter(ConnDict[user]->Tc->GetStream());

			//transEvent(tq.User, "DIE BITCH DIE");

			lH2->transaction(tq.Sender, tq.Receiver, tq.Money, tq.User, transEvent);
		}
	}

	CommandRequest cr;
	while (CommandQueue.try_pop(cr))
	{
		String ^ user = gcnew String(cr.User.c_str());
		if (ConnDict->ContainsKey(user))
		{
			StreamWriter ^ sw = gcnew StreamWriter(ConnDict[user]->Tc->GetStream());
			if (cr.Command == "BAL")
			{
				BalanceType bt = lH2->getBalance(base64_encode_2((char const*)cr.Sender.data(), cr.Sender.size()), 0, cr.User, balanceEvent);
				int64_t bal = bt.getBalance();
				balanceEvent(cr.User, to_string(bal));
			}
		}
	}

	Updating = false;
}

void NetworkClient::UpdateEvents(Object^ data)
{

	if (!Updating) InternalUpdate();

}


