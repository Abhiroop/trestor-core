

/*
*  @Author: Arpan Jati + Aritra Dhar
*  @Version: 1.0
*  @Date : 8th Aug 2014
*  @Description: Hash class for use as Key in Maps
*      Uses a simple cooked up HashCode function, works in general.
*/

#include "Hash.h"
#include "Utils.h"

#include "Constants.h"

Hash::Hash()
{
}

bool Hash::operator==(const Hash &mc) const {

	if ((*this).size() == mc.size())
	{
		bool equal = true;
		for (int i = 0; i < (int)(*this).size(); i++)
			if ((*this)[i] != mc[i])
			{
			equal = false;
			break;
			}
		return equal;
	}
	return false;
}

bool Hash::operator<(const Hash &mc) const {

	if ((*this).size() == mc.size())
	{
		for (int i = 0; i < (int)(*this).size(); i++)
			if ((*this)[i] < mc[i])
			{
			return true;
			}
	}
	return false;
}

Hash::operator size_t() const { return (*this).size(); };

size_t Hash::hash() const {

	int len = (int)(*this).size();
	int hc = 0x12345678;
	for (int i = 0; i < len; i += 4)
	{
		hc ^= ((*this)[i] << 0) + ((*this)[i + 1] << 8) + ((*this)[i + 2] << 16) + ((*this)[i + 3] << 24);
	}
	return hc;

}

size_t Hash::operator()(const Hash &mc) const {
	return mc.hash();
}

bool Hash::operator()(const Hash &mc1, const Hash &mc2) const {
	return (mc1 < mc2);
}

ostream& operator<<(ostream &os, Hash &mc)
{
	for (int i = 0; i < (int)mc.size(); i++)
	{
		os << (int)(mc[i]) << " ";
	}
	return os;
}

string Hash::ToString() const
{	
	int length = (int)(*this).size();

	if (length > 0)
	{
		vector<char> finalStr = vector<char>();
		finalStr.reserve(length << 1);
		for (int i = 0; i < length; i++)
		{
			finalStr.push_back((char)Constants::hexChars[(((*this)[i]) >> 4) & 0xF]);
			finalStr.push_back((char)Constants::hexChars[(*this)[i] & 0xF]);
		}

		return string(finalStr.begin(), finalStr.end());
	}

	return "NULL_DATA_BAD_BAD_BAD";
}

