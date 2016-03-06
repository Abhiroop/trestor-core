#ifndef Salsa20_H
#define Salsa20_H

#include <vector>

using namespace std;

class Salsa20
{
public:

	bool Salsa20::Process(vector<unsigned char> Data, vector<unsigned char> Key, vector<unsigned char> IV, vector<unsigned char> & ProcessedData);

	Salsa20();
};

#endif