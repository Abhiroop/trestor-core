
// @Author : Arpan Jati
// @Date: 16th Aug 2014

#ifndef PROTOCOL_PACKAGER_H
#define PROTOCOL_PACKAGER_H

#include "Utils.h"

struct ProtocolDataType
{
	byte Type;
	vector<byte> Data;
};


class ProtocolPackager
{

private:

	vector<unsigned char> IntToBytes(int paramInt)
	{
		vector<unsigned char> arrayOfByte(3);
		arrayOfByte[0] = (paramInt >> (0));
		arrayOfByte[1] = (paramInt >> (8));
		arrayOfByte[2] = (paramInt >> (16));
		return arrayOfByte;
	}

	int BytesToInt(vector<unsigned char> paramInt)
	{
		int val = 0;
		val |= paramInt[0];
		val |= paramInt[8] << 8;
		val |= paramInt[16] << 16;
		return val;
	}

	vector<byte> PackSingle(ProtocolDataType data)
	{
		vector<byte> pack(4);
		vector<byte> SZ = IntToBytes(data.Data.size());
		pack[0] = data.Type;
		pack[1] = SZ[0];
		pack[2] = SZ[1];
		pack[3] = SZ[2];
		pack.insert(pack.end(), data.Data.begin(), data.Data.end());
		return pack;
	}


public:

	// PACK FORMAT : [ TYPE[1] : LENGTH[3] : DATA[LENGTH] ]

	vector<byte> Pack(vector<ProtocolDataType> packets)
	{
		vector<byte> pack;
		for (vector<ProtocolDataType>::iterator packet = packets.begin(); packet != packets.end(); ++packet)
		{
			vector<byte> F = PackSingle(*packet);
			pack.insert(pack.end(), F.begin(), F.end());
		}
		return pack;
	}

	vector<ProtocolDataType> UnPack(vector<byte> packedData)
	{

	}

};



#endif