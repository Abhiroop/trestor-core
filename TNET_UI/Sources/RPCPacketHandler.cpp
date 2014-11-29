#include "RPCPacketHandler.h"
#include "RPCKeyExchange.h"

#define TRACE(msg)            wcout << msg
#define TRACE_ACTION(a, k, v) wcout << a << L" (" << k << L", " << v << L")\n"

//void handle_post1(http_request request);

enum PACKET_TYPE
{
	VALIDATOR_KEY_EXCHANGE = 0x20,
	VALIDATOR_LEDGER_UPDATE = 0x21,
	VALIDATOR_CONSENSUS_PACKET = 0x22,
	VALIDATOR_VOTE_PACKET = 0x23,

	USER_REGISTRATION_REQUEST = 0x50,
	USER_BALANCE_UPDATE = 0x51
};

State __state;

void handle_post(http_request request)
{
	value jvalue = request.extract_json().get();
	string packetType;

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

					switch (PT)
					{
					case VALIDATOR_KEY_EXCHANGE:
					{
												   RPCKeyExchange RPCKE(__state);
												   RPCKE.handleKetExchange(jvalue);

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
					throw exception("BAD packet type in the JSON field");
				}

			}
			else
			{
				throw exception("packetType not exists in JSON");
			}
		}
		else
		{
			throw exception("null json value");
		}
	}

	catch (exception& e)
	{
		cout << e.what() << endl;
	}
}

RPCPackerHandler::RPCPackerHandler()
{

}


RPCPackerHandler::RPCPackerHandler(State _state)
{

	http_listener listener(L"http://*:80/restdemo");
	listener.support(methods::POST, handle_post);

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