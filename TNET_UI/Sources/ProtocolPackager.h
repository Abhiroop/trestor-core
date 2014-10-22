
// @Author : Arpan Jati
// @Date: 16th Aug 2014, 
// Updated to use varint : Sept 7, 2014

#ifndef PROTOCOL_PACKAGER_H
#define PROTOCOL_PACKAGER_H

#include "Utils.h"

struct ProtocolDataType
{
	// Any Identifier based on protocol
	byte NameType;
	// It can be INT, FLOAT, LONG, BYTE[], 
	byte DataType;
	vector<byte> Data;
};

enum ProtocolData { PD_BYTE_VECTOR, PD_BYTE, PD_INT16, PD_INT32, PD_INT64, PD_FLOAT, PD_DOUBLE, PD_BOOL, PD_STRING, PD_VARINT };

class ProtocolPackager
{

private:

	/*static vector<unsigned char> IntToBytes(int paramInt);

	static int BytesToInt(vector<unsigned char> paramInt);*/

	static vector<byte> PackSingle(ProtocolDataType data);

	static unique_ptr<ProtocolDataType> ProtocolPackager::GenericPack(byte DataType, byte nameType);

public:
	// Packaging Format : Supports 16 MB Data per pack.
	//    Index :      {0}      {123}        {4 ..... }
	// PACK FORMAT : [ TYPE[1] : LENGTH[3] : DATA[LENGTH] ]

	/////////////////////////////////////  PACKERS  /////////////////////////////////////

	static vector<byte> PackRaw(vector<ProtocolDataType> packets);

	static vector<ProtocolDataType> UnPackRaw(vector<byte> packedData);

	static unique_ptr<ProtocolDataType> Pack(vector<byte> input, byte nameType);

	static vector<byte> ProtocolPackager::Pack(vector<vector<char>> vectorValue);

	static vector<byte> ProtocolPackager::Pack(vector<vector<unsigned char>> vectorValue);

	static unique_ptr<ProtocolDataType> ProtocolPackager::Pack(vector<char> vectorValue, byte nameType);

	static unique_ptr<ProtocolDataType> Pack(byte byteValue, byte nameType);

	static unique_ptr<ProtocolDataType> Pack(int16_t shortValue, byte nameType);

	static unique_ptr<ProtocolDataType> Pack(int32_t intValue, byte nameType);

	static unique_ptr<ProtocolDataType> Pack(int64_t longValue, byte nameType);

	static unique_ptr<ProtocolDataType> Pack(float floatValue, byte nameType);

	static unique_ptr<ProtocolDataType> Pack(double doubleValue, byte nameType);

	static unique_ptr<ProtocolDataType> PackVarint(int64_t varintValue, byte nameType);

	static unique_ptr<ProtocolDataType> Pack(bool boolValue, byte nameType);

	static unique_ptr<ProtocolDataType> Pack(string stringValue, byte nameType);

	/////////////////////////////////////  UNPACKERS  /////////////////////////////////////

	static bool UnpackByteVector(ProtocolDataType packedData, byte nameType, vector<byte> & Data);

	static bool UnpackVectorVector(vector<byte> rawData, vector<vector<byte>> & Data);

	static bool UnpackVectorVector_s(vector<byte> rawData, int ExpectedSize, vector<vector<byte>> & Data);

	static bool UnpackByteVector_s(ProtocolDataType packedData, byte nameType, int ExpectedSize, vector<byte> & Data);

	static bool UnpackByteVector_char(ProtocolDataType packedData, byte nameType, vector<char> & Data);

	static bool UnpackByte(ProtocolDataType packedData, byte nameType, byte & Data);

	static bool UnpackInt16(ProtocolDataType packedData, byte nameType, int16_t &  Data);

	static bool UnpackInt32(ProtocolDataType packedData, byte nameType, int32_t &  Data);

	static bool UnpackInt64(ProtocolDataType packedData, byte nameType, int64_t &  Data);

	static bool UnpackFloat(ProtocolDataType packedData, byte nameType, float & Data);

	static bool UnpackDouble(ProtocolDataType packedData, byte nameType, double &  Data);

	static bool UnpackVarint(ProtocolDataType packedData, byte nameType, int64_t & Data);

	static bool UnpackBool(ProtocolDataType packedData, byte nameType, bool & Data);

	static bool UnpackString(ProtocolDataType packedData, byte nameType, string &  Data);

#define count__  32

	/*static void TestProtocolPackager()
	{
	vector<ProtocolDataType> PDTs;

	int datas[count__];

	for (int i = 0; i < count__; i++)
	{
	int n = i;// rand();

	datas[i] = n;
	PDTs.push_back(*ProtocolPackager::PackVarint(n, 0));
	}

	vector<byte> packed = ProtocolPackager::PackRaw(PDTs);
	vector<ProtocolDataType> l = ProtocolPackager::UnPackRaw(packed);

	int fIndex = 0;

	for (vector<ProtocolDataType>::iterator it = l.begin(); it != l.end(); it++)
	{
	ProtocolDataType p = *it;

	int64_t R = 0;
	ProtocolPackager::UnpackVarint(p, 0, R);

	if (datas[fIndex] != R)
	{
	MessageQueue.push("FAIL AT : " + to_string(fIndex) + ", Value: " + to_string(R));
	}

	//MessageQueue.push("" + p.DataType.ToString() + " : " + p.NameType + " : " + R);

	fIndex++;
	}

	MessageQueue.push("\nVarint Test Complete ...");
	}*/


};



#endif