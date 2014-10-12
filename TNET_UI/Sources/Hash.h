
#ifndef HASH_H
#define HASH_H

#include <vector>
#include <algorithm>
#include <map>
#include <array>

using namespace std;

typedef vector<unsigned char> HashData;

class Hash : public HashData
{

private:

	const uint8_t hexs[16] = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

public:

	static const size_t bucket_size = 5;
	static const size_t min_buckets = 8;

	Hash(vector<unsigned char> hIn) : HashData(hIn) { };

	Hash(unsigned char* start, unsigned char* end) : HashData(start, end) { };

	Hash();

	bool operator==(const Hash &mc) const;

	bool operator<(const Hash &mc) const;

	operator size_t() const;

	size_t hash() const;

	size_t operator()(const Hash &mc) const;

	bool operator()(const Hash &mc1, const Hash &mc2) const;

	friend ostream& operator<<(ostream &os, Hash &mc);

	string ToString() const;

};

#endif