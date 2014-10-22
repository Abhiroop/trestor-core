//@Author : Aritra Dhar + Arpan Jati

#include <ctime>
#include "Utils.h"
#include "Base64.h"

char* s = "0123456789ABCDEF";
int getIndex(char c)
{
	for (int i = 0; i < 16; i++)
	{
		if (s[i] == c)
			return i;
	}
	throw exception("Bal hex char hoeche!");
}

char getNibbleSingle(int index)
{
	return s[index];
}

int CompareHexString(vector<char> i, vector<char> j)
{
	for (int t = 0; t < (int)i.size(); t++)
	{
		if (getIndex(i[t]) > getIndex(j[t]))
			return 1;

		if (getIndex(i[t]) < getIndex(j[t]))
			return -1;
	}
	return 0;
}

bool CompareCharString(unsigned char* i, unsigned char* j, int len1, int len2)
{
	if (len1 != len2)
		return false;

	for (int t = 0; t < len1; t++)
	{
		if (i[t] != j[t])
			return false;
	}
	return true;
}

bool ByteArrayEquals(vector<byte> x, int xOffset, vector<byte> y, int yOffset, int length)
{
	if (x.size() == 0) return false;
	if (y.size() == 0) return false;

	if (length < 0) return false;
	if (xOffset < 0 || (int)x.size() - xOffset < length) return false;
	if (yOffset < 0 || (int)y.size() - yOffset < length) return false;

	int DiffBytes = 0;

	for (int i = 0; i < length; i++)
	{
		DiffBytes += (x[i + xOffset] != y[i + yOffset]) ? 1 : 0;
	}

	return (DiffBytes == 0) ? true : false;
}

byte GetNibble_(Hash val, int NibbleIndex)
{
	int Address = NibbleIndex >> 1;
	int IsLow = NibbleIndex & 1;
	return (byte)((IsLow == 0) ? ((val[Address] >> 4) & 0xF) : (val[Address] & 0xF));
}

void split(vector<string> &tokens, const string &text, char sep)
{
	int start = 0, end = 0;
	while ((end = text.find(sep, start)) != string::npos) {
		tokens.push_back(text.substr(start, end - start));
		start = end + 1;
	}
	tokens.push_back(text.substr(start));
}

string ToBase64String(Hash data)
{
	unsigned char* newStr;

	newStr = data.data();

	size_t lens;
	string output = Base_64::encode(newStr, data.size(), &lens);

	return output;
}

Hash Base64ToHash(string data)
{
	size_t lens;
	byte* decoded = Base_64::decode(data.data(), data.length(), &lens);
	Hash h(decoded, decoded + lens);

	return h;
}


/// <summary>
/// Generates Non-Repeating sequence of Random Numbers
/// </summary>
/// <param name="maxNumber">Exclusive upperbound, lowerbound = 0</param>
/// <param name="Count">Number of elements to be generated</param>
/// <returns></returns>
vector<int> GenerateNonRepeatingDistribution(int maxNumber, int Count, int self)
{
	if (maxNumber < Count) throw new exception("maxNumber < Count");

	std::default_random_engine generator;
	generator.seed(rand());
	std::uniform_int_distribution<int> distribution(0, maxNumber - 1);
	vector<byte> data;

	hash_set<int> ints;
	ints.reserve(Count);
	vector<int> intsV;
	intsV.reserve(Count);

	while ((int)ints.size() < Count)
	{
		int Rand = distribution(generator);
		int v = ints.count(Rand);
		if ((v == 0) && (self != Rand))
		{
			ints.insert(Rand);
			intsV.push_back(Rand);
		}
	}

	return intsV;
}


Hash GenerateNewToken32()
{
	unsigned char dp[32];
	RandomFillBytes(dp, 32);
	return Hash(dp, dp + 32);
}

//there may be some petalevel bullshit 
uint64_t getCurrentUTFtime()
{
	time_t rawtime;
	struct tm * ptm = 0;
	time(&rawtime);

#ifdef _WIN32
	gmtime_s(ptm, &rawtime);
#else
	ptm = gmtime(&rawtime);
#endif

	uint64_t tm = mktime(ptm);
	return tm;
}

#ifdef __cplusplus_cli

#include <msclr\marshal_cppstd.h>
#include <Windows.h>
#include <Wincrypt.h>

std::wstring StringUtils::stows(std::string s)
{
	std::wstring ws;
	ws.assign(s.begin(), s.end());
	return ws;
}

std::string StringUtils::wstos(std::wstring ws)
{
	std::string s;
	s.assign(ws.begin(), ws.end());
	return s;
}

System::String ^StringUtils::stops(std::string s)
{
	return gcnew System::String(stows(s).c_str());
}

std::string StringUtils::pstos(System::String^ ps)
{
	msclr::interop::marshal_context context;
	std::string standardString = context.marshal_as<std::string>(ps);
	return standardString;
}

System::String^ StringUtils::atops(const char *text)
{
	return StringUtils::stops(std::string(text));
}



int RandomFillBytes(unsigned char *seed, int length)
{
	HCRYPTPROV prov;

	if (!CryptAcquireContext(&prov, NULL, NULL, PROV_RSA_FULL, CRYPT_VERIFYCONTEXT))  {
		return 1;
	}

	if (!CryptGenRandom(prov, 32, seed))  {
		CryptReleaseContext(prov, 0);
		return 1;
	}

	CryptReleaseContext(prov, 0);

	return 0;
}



#endif

#ifdef __cplusplus_winrt

std::wstring StringUtils::stows(std::string s)
{
	std::wstring ws;
	ws.assign(s.begin(), s.end());
	return ws;
}

std::string StringUtils::wstos(std::wstring ws)
{
	std::string s;
	s.assign(ws.begin(), ws.end());
	return s;
}

Platform::String ^StringUtils::stops(std::string s)
{
	return ref new Platform::String(stows(s).c_str());
}

std::string StringUtils::pstos(Platform::String^ ps)
{
	return wstos(std::wstring(ps->Data()));
}

Platform::String^ StringUtils::atops(const char *text)
{
	return StringUtils::stops(std::string(text));
}

double StringUtils::ToDouble(Platform::String^ platformString)
{
	return _wtof(platformString->Data());
}

int StringUtils::ToInt(Platform::String^ platformString)
{
	return _wtoi(platformString->Data());
}

int64_t StringUtils::ToInt64(Platform::String^ platformString)
{
	return _wtoi64(platformString->Data());
}

#include <Windows.h>
#include <Wincrypt.h>

#include <inttypes.h>
#include <winstring.h>
#include <roapi.h>
#include <windows.security.cryptography.h>
#include <windows.storage.h>
#include <windows.storage.streams.h>
#include <inspectable.h>

using namespace ABI::Windows::Security::Cryptography;
using namespace ABI::Windows::Storage;
using namespace ABI::Windows::Storage::Streams;

int RandomFillBytes(unsigned char *seed, int length)
{
	static const WCHAR *className = L"Windows.Security.Cryptography.CryptographicBuffer";
	const UINT32 clen = wcslen(className);

	HSTRING hClassName = NULL;
	HSTRING_HEADER header;
	HRESULT hr = WindowsCreateStringReference(className, clen, &header, &hClassName);
	if (hr) {
		WindowsDeleteString(hClassName);
		return 1;
	}

	ICryptographicBufferStatics *cryptoStatics = NULL;
	hr = RoGetActivationFactory(hClassName, IID_ICryptographicBufferStatics, (void**)&cryptoStatics);
	WindowsDeleteString(hClassName);

	if (hr)
		return 1;

	IBuffer *buffer = NULL;
	hr = cryptoStatics->GenerateRandom(length, &buffer);
	if (hr)
		return 1;
	UINT32 olength;
	unsigned char *rnd = NULL;
	hr = cryptoStatics->CopyToByteArray(buffer, &olength, (BYTE**)&rnd);
	memcpy(seed, rnd, length);

	return 0;
}


#endif

#ifdef METRO




#endif

