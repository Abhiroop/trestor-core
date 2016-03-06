
/*
*  @Author: Arpan Jati
*  @Version: 1.0
*/

#include "Utils.h"
#include "Conversions.h"

vector<byte> Conversions::Int64ToVector(int64_t data)
{
	vector<byte> out = vector<byte>(8);

	for (int i = 0; i < 8; i++)
	{
		out[i] = (((data >> (8 * i)) & 0xFF));
	}

	return out;
}

vector<byte> Conversions::Int16ToVector(int16_t data)
{
	vector<byte> out = vector<byte>(2);

	for (int i = 0; i < 2; i++)
	{
		out[i] = (((data >> (8 * i)) & 0xFF));
	}

	return out;
}

vector<byte> Conversions::Int32ToVector(int32_t data)
{
	vector<byte> out = vector<byte>(4);

	for (int i = 0; i < 4; i++)
	{
		out[i] = (((data >> (8 * i)) & 0xFF));
	}

	return out;
}

vector<byte> Conversions::FloatToVector(float data)
{
	unsigned char * p = reinterpret_cast<unsigned char *>(&data);
	vector<byte> out(p, p + 4);
	return out;
}

vector<byte> Conversions::DoubleToVector(double data)
{
	unsigned char * p = reinterpret_cast<unsigned char *>(&data);
	vector<byte> out(p, p + 8);
	return out;
}


int64_t Conversions::VectorToInt64(vector<byte> data)
{
		uint64_t result;
		result = (uint64_t)data[0];
		result |= ((uint64_t)data[1]) << 8;
		result |= ((uint64_t)data[2]) << 16;
		result |= ((uint64_t)data[3]) << 24;
		result |= ((uint64_t)data[4]) << 32;
		result |= ((uint64_t)data[5]) << 40;
		result |= ((uint64_t)data[6]) << 48;
		result |= ((uint64_t)data[7]) << 56;
		return result;
}

int16_t Conversions::VectorToInt16(vector<byte> data)
{
	int16_t result;
	result = (int16_t)data[0];
	result |= ((int16_t)data[1]) << 8;
	return result;
}

int32_t Conversions::VectorToInt32(vector<byte> data)
{
	int32_t result;
	result = (int32_t)data[0];
	result |= ((int32_t)data[1]) << 8;
	result |= ((int32_t)data[2]) << 16;
	result |= ((int32_t)data[3]) << 24;
	return result;
}

float Conversions::VectorToFloat(vector<byte> data)
{
	float* floatArray = reinterpret_cast<float*>(data.data());
	return floatArray[0];
}

double Conversions::VectorToDouble(vector<byte> data)
{
	double* doubleArray = reinterpret_cast<double*>(data.data());
	return doubleArray[0];
}