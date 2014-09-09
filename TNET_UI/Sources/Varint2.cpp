
// @Author : Arpan Jati
// @Date : 7th Sept 2014

#include "Varint2.h"

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

vector<byte> Varint2::Encode(int64_t value)
{
	int64_t val = value;

	int bitLength = GetBitLength(value);

	int fullDataBytesNeeded = (byte)(bitLength >> 3); // Divide by 8 to get number of bytes
	int extraPackedBits = (byte)(bitLength - (fullDataBytesNeeded << 3));

	if (extraPackedBits <= 4) // MSB's in Length Byte
	{
		int totalBytesNeeded = fullDataBytesNeeded + 1;
		vector<byte> EncodedBytes(totalBytesNeeded);

		EncodedBytes[0] = (byte)((((byte)(totalBytesNeeded)& 0xF) << 4) | ((byte)(val >> (fullDataBytesNeeded << 3) & 0xF)));

		for (int i = 1; i <= fullDataBytesNeeded; i++)
		{
			EncodedBytes[i] = ((byte)(val >> ((fullDataBytesNeeded - i) << 3) & 0xFF));
		}

		return EncodedBytes;
	}
	else // Extra byte for MSB's
	{
		int totalBytesNeeded = fullDataBytesNeeded + 2;
		vector<byte> EncodedBytes(totalBytesNeeded);

		EncodedBytes[0] = (byte)(((byte)(totalBytesNeeded)& 0xF) << 4);

		for (int i = 0; i <= fullDataBytesNeeded; i++)
		{
			EncodedBytes[i + 1] = ((byte)(val >> ((fullDataBytesNeeded - i) << 3) & 0xFF));
		}

		return EncodedBytes;
	}
}

bool Varint2::Decode(vector<byte> value, int startIndex, int & length, int64_t & result)
{
	if (value.size() - startIndex > 0)
	{
		int bytes = (byte)((value[startIndex] >> 4) & 0xF);
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

bool Varint2::Decode(vector<byte> value, int64_t result)
{
	int outlen = 0;
	return Decode(value, 0, outlen, result);
}