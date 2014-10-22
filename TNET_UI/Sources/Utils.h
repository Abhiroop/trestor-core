//@Author : Aritra Dhar + Arpan Jati

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

bool ByteArrayEquals(vector<byte> x, int xOffset, vector<byte> y, int yOffset, int length);

bool CompareCharString(unsigned char* i, unsigned char* j, int len1, int len2);

byte GetNibble_(Hash val, int NibbleIndex);

void split(vector<string> &tokens, const string &text, char sep);

string ToBase64String(Hash data);

uint64_t getCurrentUTFtime();

Hash Base64ToHash(string data);

int CompareHexString(vector<char> i, vector<char> j);

int getIndex(char c);

char getNibbleSingle(int index);

Hash GenerateNewToken32();

/// <summary>
/// Generates nRandom Numbers
/// </summary>
/// <param name="maxNumber">Exclusive upperbound, lowerbound = 0</param>
/// <param name="Count">Number of elements to be generated</param>
/// <returns></returns>
vector<int> GenerateNonRepeatingDistribution(int maxNumber, int Count, int self);

enum PacketTypes {
	TPT_HELLO = 0x00, TPT_DISCONNECT = 0x01, TPT_KEEPALIVE = 0x02,
	TPT_KEY_EXCHANGE_1 = 0x10, TPT_KEY_EXCHANGE_2 = 0x11, TPT_KEY_EXCHANGE_DONE = 0x12,
		
	// TPT_TRANS_REQUEST : DATA [TransactionContent] or a single TransactionContent request.
	TPT_TRANS_REQUEST = 0x20,
	
	TPT_TRANS_FORWARDING = 0x21,

	TPT_CONS_STATE = 0x30, TPT_CONS_CURRENT_SET = 0x31, TPT_CONS_REQUEST_TC_TX = 0x32, TPT_CONS_RESP_TC_TX = 0x33,

	TPT_CONS_VOTES = 0x34, TPT_CONS_TIME_SYNC = 0x35, TPT_CONS_DOUBLESPENDERS = 0x36,

	TPT_LSYNC_FETCH_ROOT = 0x40, TPT_LSYNC_FETCH_LAYER_INFO = 0x41, TPT_LSYNC_FETCH_LAYER_DATA = 0x42,



};


struct NetworkPacket
{
	// Network layer security must make sure that this value is correct.
	// Must check signature.
	Hash PublicKey_Src;
	byte Type;
	vector<byte> Data;
	Hash Token;
	NetworkPacket(Hash publicKey_Src, byte type, vector<byte> data, Hash token) : PublicKey_Src(publicKey_Src), Type(type), Data(data), Token(token) { }
	NetworkPacket() {}
};

struct NetworkPacketQueueEntry
{
	Hash PublicKey_Dest;
	NetworkPacket Packet;
	NetworkPacketQueueEntry(Hash publicKey_Dest, NetworkPacket packet) : PublicKey_Dest(publicKey_Dest), Packet(packet) { }
	NetworkPacketQueueEntry() {}
	
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
