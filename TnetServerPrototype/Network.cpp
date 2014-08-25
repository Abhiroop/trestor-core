
#include "Network.h"

#include "TCPClientData.h"
#include "LedgerHandler.h"
#include "..\TNET_UI\Sources\TBB\tbb\concurrent_queue.h"

using namespace tbb;

struct MessageData
{
	string User;
	string Message;

	MessageData(){};

	MessageData(string user, string message)
	{
		User = user;
		Message = message;
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

shared_ptr<LedgerHandler> lH2;

concurrent_queue<MessageData> MessageQueue;
concurrent_queue<TransactionRequest> TransactionQueue;

void transEvent(string user, string message)
{
	MessageQueue.push(MessageData(user, message));
}

NetworkClient::NetworkClient(shared_ptr<LedgerHandler> _lH)
{
	lH2 = _lH;
	tList->Start();
}

void NetworkClient::ReplyToClient(string s)
{

}


void NetworkClient::MarshalString(String ^ s, string& os) {
	using namespace Runtime::InteropServices;
	const char* chars =
		(const char*)(Marshal::StringToHGlobalAnsi(s)).ToPointer();
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

		}		
		catch (Exception^ ex)
		{
			Console::WriteLine("User Disconnected : " + TCD->Tc->Client->RemoteEndPoint->ToString() + " : " + ex->Message);
		}
	}

	Console::WriteLine("User Disconnected : " + TCD->Tc->Client->RemoteEndPoint->ToString());
}




void NetworkClient::UpdateEvents()
{
	while (tList->Pending())
	{
		TcpClient^ TC = tList->AcceptTcpClient();
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

		if (ConnDict->ContainsKey(dest))
		{
			StreamWriter ^ sw = gcnew StreamWriter(ConnDict[dest]->Tc->GetStream());

			sw->WriteLine("TRANS_RESP|" + Convert::ToBase64String(Encoding::UTF8->GetBytes(msg)));
			sw->Flush();


			Console::WriteLine(dest + ": " + msg);
		}
	}


	TransactionRequest tq;
	while (TransactionQueue.try_pop(tq))
	{
		String ^ user = gcnew String(tq.User.c_str());		
		if (ConnDict->ContainsKey(user))
		{
			StreamWriter ^ sw = gcnew StreamWriter(ConnDict[user]->Tc->GetStream());
			lH2->transaction(tq.Sender, tq.Receiver, tq.Money, tq.User, transEvent);			
		}
	}


}


