#ifndef RPCPacketHandler_H
#define RPCPacketHandler_H

#include <cpprest/http_listener.h>
#include <cpprest/http_client.h>
#include <cpprest/json.h>

#include "State.h"

using namespace web;
using namespace web::http;
using namespace web::http::experimental::listener;
using namespace web::json;
using namespace std;

class RPCPackerHandler
{
	State state;

public:
	RPCPackerHandler();
	RPCPackerHandler(State _state);
};

#endif