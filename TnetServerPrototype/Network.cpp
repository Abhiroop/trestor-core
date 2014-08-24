

#include "Network.h"

#include "TCPClientData.h"
#include "LedgerHandler.h"

shared_ptr<LedgerHandler> lH;

NetworkClient::NetworkClient(shared_ptr<LedgerHandler> _lH)
{
	lH = _lH;

	tList->Start();
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

					MarshalString(parts2[0], SENDER_PK);
					MarshalString(parts2[1], RECEIVER_PK);

					int64_t PAISA = Int64::Parse(parts2[2]);


					lH->transaction(SENDER_PK, RECEIVER_PK, PAISA);

				}
			}
		}

		//Thread::Sleep(100);
	}
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
}


