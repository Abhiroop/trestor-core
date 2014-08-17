#ifndef UTILS_H
#define UTILS_H

// GET RID OF THESE on other platforms
#define _CRT_SECURE_NO_WARNINGS

#include <vector>
#include <random>
#include <algorithm>
#include <map>
#include <hash_set>
#include <hash_map>
#include <stack>
#include <queue>
#include <array>
#include <set>
#include <list>
#include <memory>
#include <stdint.h>
#include "Point2.h"

#include "Hash.h"

#include "Types.h"

using namespace std;

byte GetNibble_(Hash val, int NibbleIndex);

void split(vector<string> &tokens, const string &text, char sep);

string convertVector(Hash data);

uint64_t getCurrentUTFtime();

/// <summary>
/// Generates nRandom Numbers
/// </summary>
/// <param name="maxNumber">Exclusive upperbound, lowerbound = 0</param>
/// <param name="Count">Number of elements to be generated</param>
/// <returns></returns>
vector<int> GenerateNonRepeatingDistribution(int maxNumber, int Count, int self);

enum PacketTypes {
	PT_HELLO = 0x00, PT_DISCONNECT = 0x01, PT_KEEPALIVE = 0x02,
	PT_KEY_EXCHANGE_1 = 0x10, PT_KEY_EXCHANGE_2 = 0x11, PT_KEY_EXCHANGE_DONE = 0x12,
	PT_TRANS_REQUEST = 0x20, PT_TRANS_FORWARDING = 0x21,
	PT_CONS_STATE = 0x30, PT_CONS_CURRENT_SET = 0x31, PT_CONS_REQUEST_TX = 0x32, PT_CONS_RESP_TX = 0x33
};

struct NetworkPacket
{
	Hash PublicKey_Src;
	byte Type;
	vector<byte> Data;
};

struct NetworkPacketQueueEntry
{
	Hash PublicKey_Dest;
	NetworkPacket Packet;
};

#ifdef __cplusplus_cli

class StringUtils
{
public:

	static std::wstring stows(std::string s);
	static std::string wstos(std::wstring ws);
	static System::String ^stops(std::string s);
	static std::string pstos(System::String^ ps);
	static System::String ^atops(const char *text);
};


#endif

#ifdef __cplusplus_winrt

class StringUtils
{
public:

	static std::wstring stows(std::string s);
	static std::string wstos(std::wstring ws);
	static Platform::String ^stops(std::string s);
	static std::string pstos(Platform::String^ ps);
	static Platform::String ^atops(const char *text);

	static double ToDouble(Platform::String^ platformString);

	static int ToInt(Platform::String^ platformString);

	static int64_t ToInt64(Platform::String^ platformString);

};

#endif

int RandomFillBytes(unsigned char *seed, int length);

#endif
