#include <cpprest/http_listener.h>
#include <cpprest/http_client.h>
#include <cpprest/json.h>
#include <iostream>
#include <map>
#include <set>
#include <string>
#include <vector>

using namespace web;
using namespace web::http;
using namespace web::http::experimental::listener;
using namespace web::json;
using namespace std;

#define TRACE(msg)            wcout << msg
#define TRACE_ACTION(a, k, v) wcout << a << L" (" << k << L", " << v << L")\n"

typedef web::json::value(*ScriptFunction)(void); // function pointer type
typedef std::map<std::string, ScriptFunction> script_map;

script_map m;

map<utility::string_t, utility::string_t> dictionary;

void handle_get(http_request request)
{
	TRACE(L"\nhandle GET\n");
	std::vector<std::pair<utility::string_t, web::json::value> > answer;
	for (auto const & p : dictionary)
	{
		//if (p.first == request.extract_string)
		answer.push_back(make_pair(json::value(p.first).as_string(), json::value(p.second)));
	}

	request.reply(status_codes::OK, json::value::object(answer));
}

void handle_request(http_request request)
{
	//implemented stl::map of function calls

	const string &a = "account";

	script_map::const_iterator iter = m.find(a);	//look up for function
	if (iter == m.end())
	{
		// not found
		TRACE("\n not found\n");
	}

	request.reply(status_codes::OK, (*iter->second)());
}

void handle_post(http_request request)
{
	TRACE("\nhandle POST\n");
	handle_request(request);

	//wcout << request. << endl;

	//displayed how we can iterate over a json object, the parser is already there,so finding a field is easy
	json::value obj;

	obj[L"name"] = json::value::string(U("Trestor"));
	obj[L"key2"] = json::value::number(44);
	obj[L"key3"] = json::value::number(43.6);
	obj[L"key4"] = json::value::string(U("str"));

	for (auto iter = obj.as_object().cbegin(); iter != obj.as_object().cend(); ++iter)
	{
		const utility::string_t &str = iter->first;
		const json::value &v = iter->second;
		wcout << L"String: " << str << L", Value: " << v.serialize() << endl;
	}

}

web::json::value account(void)
{
	json::value obj;

	obj[L"name"] = json::value::string(U("Trestor"));
	obj[L"key2"] = json::value::number(44);
	obj[L"key3"] = json::value::number(43.6);
	obj[L"key4"] = json::value::string(U("str"));

	return obj;
}

int main()
{
	utility::string_t s = L"a";
	utility::string_t s1 = L"a";

	dictionary[s]= s1;


	http_listener listener(L"http://*:80/restdemo");

	listener.support(methods::GET, handle_get);
	listener.support(methods::POST, handle_post);
	/*listener.support(methods::PUT, handle_put);
	listener.support(methods::DEL, handle_del);*/


	m.insert(std::make_pair("account", &account));          //register your functions like this corresponding to string
	//which we are planning to extract from json field "method"
	try
	{
		listener
			.open()
			.then([&listener](){TRACE(L"\nstarting to listen\n"); })
			.wait();

		while (true);
	}
	catch (exception const & e)
	{
		wcout << e.what() << endl;
	}

	getchar();

	return 0;
}
