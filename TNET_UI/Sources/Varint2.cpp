
// @Author : Arpan Jati
// @Date : 7th Sept 2014

#include "Varint2.h"

// TODO: Simple optimisation which does this in O(log N) instead of O(N)
int Varint2::GetBitLength(int64_t value)
{
	int length = 0;
	for (int i = 63; i >= 0; i--)
	{
		if (((value >> i) & 1) == 1)
		{
			length = (i + 1);
			break;
		}
	}

	return length;
}

vector<uint8_t> Varint2::Encode(int64_t value)
{
	int64_t val = value;

	int bitLength = GetBitLength(value);

	int fullDataBytesNeeded = (uint8_t)(bitLength >> 3); // Divide by 8 to get number of bytes
	int extraPackedBits = (uint8_t)(bitLength - (fullDataBytesNeeded << 3));

	if (extraPackedBits <= 4) // MSB's in Length Byte
	{
		int totalBytesNeeded = fullDataBytesNeeded + 1;
		vector<uint8_t> EncodedBytes(totalBytesNeeded);

		EncodedBytes[0] = (uint8_t)((((uint8_t)(totalBytesNeeded)& 0xF) << 4) | ((uint8_t)(val >> (fullDataBytesNeeded << 3) & 0xF)));

		for (int i = 1; i <= fullDataBytesNeeded; i++)
		{
			EncodedBytes[i] = ((uint8_t)(val >> ((fullDataBytesNeeded - i) << 3) & 0xFF));
		}

		return EncodedBytes;
	}
	else // Extra byte for MSB's
	{
		int totalBytesNeeded = fullDataBytesNeeded + 2;
		vector<uint8_t> EncodedBytes(totalBytesNeeded);

		EncodedBytes[0] = (uint8_t)(((uint8_t)(totalBytesNeeded)& 0xF) << 4);

		for (int i = 0; i <= fullDataBytesNeeded; i++)
		{
			EncodedBytes[i + 1] = ((uint8_t)(val >> ((fullDataBytesNeeded - i) << 3) & 0xFF));
		}

		return EncodedBytes;
	}
}

bool Varint2::Decode(vector<uint8_t> value, int startIndex, int & length, int64_t & result)
{
	if (value.size() - startIndex > 0)
	{
		int bytes = (uint8_t)((value[startIndex] >> 4) & 0xF);
		if (bytes <= (int)value.size() - startIndex)
		{
			length = bytes;
			result = (value[startIndex] & 0xF);
			for (int i = 1; i < bytes; i++)
			{
				result <<= 8;
				result |= value[i + startIndex];
			}
			return true;
		}
		else return false;		
	}
	else return false;
}

bool Varint2::Decode(vector<uint8_t> value, int64_t result)
{
	int outlen = 0;
	return Decode(value, 0, outlen, result);
}