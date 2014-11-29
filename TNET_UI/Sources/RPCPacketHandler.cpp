#include "RPCPacketHandler.h"
#include "RPCKeyExchange.h"

/*
The constructors will take care of all the packet types
*/
enum PACKET_TYPE
{
	VALIDATOR_KEY_EXCHANGE = 0x20,
	VALIDATOR_LEDGER_UPDATE = 0x21,
	VALIDATOR_CONSENSUS_PACKET = 0x22,
	VALIDATOR_VOTE_PACKET = 0x23,

	USER_REGISTRATION_REQUEST = 0x50,
	USER_BALANCE_UPDATE = 0x51
};

RPCPackerHandler::RPCPackerHandler()
{

}

RPCPackerHandler::RPCPackerHandler(http_request request, State _state)
{
	state = _state;

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
												   RPCKeyExchange RPCKE(state);
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