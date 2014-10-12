#include "Base64_2.h"
#include "Network.h"
#include "ProtocolPackager.h"

#include "TransactionContent.h"
#include "TransactionEntity.h"

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

struct CommandRequest
{
	// IP:PORT for the client
	string User;

	// Public Key of the Sender
	vector <unsigned char> Sender;

	// Command
	string Command;

	// Data content of the request
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
concurrent_queue<CommandRequest> CommandQueue;

void transEvent(string user, string message)
{
	MessageQueue.push(MessageData(user, message, "TRX_RESP"));
}

void emptyEvent(string user, string message)
{
	//MessageQueue.push(MessageData(user, message, "TRX_RESP"));
}

void balanceEvent(string user, string message)
{
	MessageQueue.push(MessageData(user, message, "BAL_RESP"));
}

void NetworkClient::ProcessMessages(Object^ obj)
{
	while (NetworkAlive)
	{

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

		Thread::Sleep(20);
	}
}

void NetworkClient::ProcessCommands(Object^ obj)
{
	while (NetworkAlive)
	{
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

				if (cr.Command == "TRX")
				{
					TransactionContent tc;

					tc.Deserialize(cr.Data);

					string sender = base64_encode_2((char const*)tc.PublicKey.data(), tc.PublicKey.size());

					int64_t total_money = 0;
					for (int i = 0; i < (int)tc.Destinations.size(); i++)
					{
						total_money += tc.Destinations[i].Amount;
					}

					BalanceType bt = lH2->getBalance(base64_encode_2((char const*)cr.Sender.data(), cr.Sender.size()), 0, cr.User, emptyEvent);
					int64_t bal = bt.getBalance();
					
					if (total_money > bal)
					{
						transEvent(cr.User, "Unsufficient Sender Balance");
					}
					else
					{
						int Trans = 0;

						for (int i = 0; i < (int)tc.Destinations.size(); i++)
						{
							string receiver = base64_encode_2((char const*)tc.Destinations[i].PublicKey_Sink.data(), tc.Destinations[i].PublicKey_Sink.size());
							int64_t money = tc.Destinations[i].Amount;

							for (int j = 0; j < 10;j++)
								Trans += lH2->transaction(sender, receiver, money, cr.User, emptyEvent);

						}

						transEvent(cr.User, to_string(Trans) + " transaction(s) complete.");
					}
				}
			}
		}

		Thread::Sleep(20);
	}
}

NetworkClient::NetworkClient()
{
	tList->Start();

	ParameterizedThreadStart^ pts = gcnew ParameterizedThreadStart(this, &NetworkClient::ProcessMessages);
	Thread^ thr = gcnew Thread(pts);
	thr->Start();

	ParameterizedThreadStart^ pts2 = gcnew ParameterizedThreadStart(this, &NetworkClient::ProcessCommands);
	Thread^ thr2 = gcnew Thread(pts2);
	thr2->Start();
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

			cli::array<unsigned char>^ TOTDATA = Convert::FromBase64String(data);

			std::vector<unsigned char> raw(TOTDATA->Length);
			System::Runtime::InteropServices::Marshal::Copy(TOTDATA, 0, IntPtr(&raw[0]), TOTDATA->Length);

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

		String^ remoteHost = TC->Client->RemoteEndPoint->ToString();

		TCPClientData^ TCD = gcnew TCPClientData(TC);
		ConnDict->Add(remoteHost, TCD);
		ParameterizedThreadStart^ pts = gcnew ParameterizedThreadStart(this, &NetworkClient::HandleClient);
		Thread^ thr = gcnew Thread(pts);
		thr->Start(TCD);

		Console::WriteLine("Accepted Connection : " + remoteHost);
	}

	Updating = false;
}

void NetworkClient::UpdateEvents(Object^ data)
{
	if (!Updating) InternalUpdate();
}


