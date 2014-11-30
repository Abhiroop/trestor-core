#include "RPCPacketHandler.h"
#include "RPCKeyExchange.h"

#define TRACE(msg)            wcout << msg
#define TRACE_ACTION(a, k, v) wcout << a << L" (" << k << L", " << v << L")\n"

//void handle_post1(http_request request);

enum PACKET_TYPE
{
	VALIDATOR_KEY_EXCHANGE = 20,
	VALIDATOR_LEDGER_UPDATE = 21,
	VALIDATOR_CONSENSUS_PACKET = 22,
	VALIDATOR_VOTE_PACKET = 23,

	USER_REGISTRATION_REQUEST = 50,
	USER_BALANCE_UPDATE = 51
};

State __state;

void handle_post(http_request request)
{
	TRACE("\nhandle_post\n");

	value jvalue = request.extract_json().get();
	string packetType;

	// the JSON object which is to be sent 
	value toSend;

	try
	{

		if (!jvalue.is_null())
		{
			if (jvalue.has_field(L"packetType"))
			{
				value v = jvalue[L"packetType"];
	
				if (v.is_integer())
				{
					int PT = v.as_integer();
					cout << PT;

					switch (PT)
					{
					case VALIDATOR_KEY_EXCHANGE:
					{
												   RPCKeyExchange RPCKE(__state);
												   value toSend = RPCKE.handleKetExchange(jvalue);

												   if (toSend == NULL)
													   cout << "Invalid JSON" << endl;
												   break;
					}
					case VALIDATOR_LEDGER_UPDATE:
					{
													break;
					}
					case VALIDATOR_CONSENSUS_PACKET:
					{
													   break;
					}
					case VALIDATOR_VOTE_PACKET:
					{
												  break;
					}
					case USER_REGISTRATION_REQUEST:
					{
													  break;
					}
					case USER_BALANCE_UPDATE:
					{
												break;
					}
					default:
						throw exception("BAD packet type");
					}
				}
				else
				{
					throw exception("packetType is not integer");
				}

			}
			else
			{
				throw exception("packetType not exists in JSON");
			}
		}
		else
		{
			TRACE("\nNull JSON\n");
		}
	}

	catch (exception& e)
	{
		cout << e.what() << endl;
		request.reply(status_codes::OK);
	}

	request.reply(status_codes::OK, toSend);
}

RPCPackerHandler::RPCPackerHandler()
{

}


RPCPackerHandler::RPCPackerHandler(State _state)
{

	http_listener listener(L"http://*:80/restdemo");
	listener.support(methods::POST, handle_post);

	cout << "started" << endl;

	state = _state;
	__state = _state;

	try
	{
		listener
			.open()
			.then([&listener](){TRACE(L"\nlistening for incoming JSON\n"); })
			.wait();

		while (true);
	}
	catch (exception& e)
	{
		wcout << e.what() << endl;
	}
}