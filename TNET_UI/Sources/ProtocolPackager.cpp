
// @Author : Arpan Jati
// @Date : 16th Aug 2014

#include "Conversions.h"
#include "ProtocolPackager.h"

vector<unsigned char> ProtocolPackager::IntToBytes(int paramInt)
{
	vector<unsigned char> arrayOfByte(3);
	arrayOfByte[0] = (paramInt);
	arrayOfByte[1] = (paramInt >> 8);
	arrayOfByte[2] = (paramInt >> 16);
	return arrayOfByte;
}

int ProtocolPackager::BytesToInt(vector<unsigned char> paramInt)
{
	int val = (int)paramInt[0];
	val |= ((int)paramInt[1]) << 8;
	val |= ((int)paramInt[2]) << 16;
	return val;
}

// Make Variable Size Encoding Decoding

vector<byte> ProtocolPackager::PackSingle(ProtocolDataType data)
{
	vector<byte> pack(5);
	vector<byte> SZ = IntToBytes(data.Data.size());
	pack[0] = data.NameType;
	pack[1] = data.DataType;
	pack[2] = SZ[0];
	pack[3] = SZ[1];
	pack[4] = SZ[2];
	pack.insert(pack.end(), data.Data.begin(), data.Data.end());
	return pack;
}

vector<byte> ProtocolPackager::PackRaw(vector<ProtocolDataType> packets)
{
	vector<byte> pack;
	for (vector<ProtocolDataType>::iterator packet = packets.begin(); packet != packets.end(); ++packet)
	{
		vector<byte> F = PackSingle(*packet);
		pack.insert(pack.end(), F.begin(), F.end());
	}
	return pack;
}

vector<ProtocolDataType> ProtocolPackager::UnPackRaw(vector<byte> packedData)
{
	vector<ProtocolDataType> packets;

	int index = 0;
	while (true)
	{
		if (packedData.size() - index >= 5)
		{
			vector<byte> L_Bytes(3);
			byte NameType = packedData[index + 0];
			byte DataType = packedData[index + 1];
			L_Bytes[0] = packedData[index + 2];
			L_Bytes[1] = packedData[index + 3];
			L_Bytes[2] = packedData[index + 4];

			int Length = BytesToInt(L_Bytes);

			if (packedData.size() - index - Length >= 5)
			{
				vector<byte> Data(Length);
				for (int i = 0; i < Length; i++)
				{
					Data[i] = packedData[index + 5 + i];
				}

				ProtocolDataType packet;
				packet.NameType = NameType;
				packet.DataType = DataType;
				packet.Data = Data;
				packets.push_back(packet);
			}
			else
			{
				break;
			}

			index += (Length + 5);
		}
		else
		{
			break;
		}
	}

	return packets;
}

unique_ptr<ProtocolDataType> ProtocolPackager::GenericPack(byte DataType, byte nameType)
{
	unique_ptr<ProtocolDataType> PDType(new ProtocolDataType);
	PDType->DataType = DataType;
	PDType->NameType = nameType;
	return PDType;
}

unique_ptr<ProtocolDataType> ProtocolPackager::Pack(vector<byte> vectorValue, byte nameType)
{
	unique_ptr<ProtocolDataType> PDType = GenericPack(PD_BYTE_VECTOR, nameType);
	PDType->Data = vectorValue;
	return PDType;
}

unique_ptr<ProtocolDataType> ProtocolPackager::Pack(byte byteValue, byte nameType)
{
	unique_ptr<ProtocolDataType> PDType = GenericPack(PD_BYTE, nameType);
	vector<byte> F(1);
	F[0] = byteValue;

	PDType->Data = F;
	return PDType;
}

unique_ptr<ProtocolDataType> ProtocolPackager::Pack(int16_t shortValue, byte nameType)
{
	unique_ptr<ProtocolDataType> PDType = GenericPack(PD_INT16, nameType);	
	PDType->Data = Conversions::Int16ToVector(shortValue);
	return PDType;
}

unique_ptr<ProtocolDataType> ProtocolPackager::Pack(int32_t intValue, byte nameType)
{
	unique_ptr<ProtocolDataType> PDType = GenericPack(PD_INT32, nameType);
	PDType->Data = Conversions::Int32ToVector(intValue);
	return PDType;
}

unique_ptr<ProtocolDataType> ProtocolPackager::Pack(int64_t longValue, byte nameType)
{
	unique_ptr<ProtocolDataType> PDType = GenericPack(PD_INT64, nameType);
	PDType->Data = Conversions::Int64ToVector(longValue);
	return PDType;
}

unique_ptr<ProtocolDataType> ProtocolPackager::Pack(float floatValue, byte nameType)
{
	unique_ptr<ProtocolDataType> PDType = GenericPack(PD_FLOAT, nameType);
	PDType->Data = Conversions::FloatToVector(floatValue);
	return PDType;
}

unique_ptr<ProtocolDataType> ProtocolPackager::Pack(double doubleValue, byte nameType)
{
	unique_ptr<ProtocolDataType> PDType = GenericPack(PD_DOUBLE, nameType);
	PDType->Data = Conversions::DoubleToVector(doubleValue);
	return PDType;
}


unique_ptr<ProtocolDataType> ProtocolPackager::Pack(bool boolValue, byte nameType)
{
	unique_ptr<ProtocolDataType> PDType = GenericPack(PD_BOOL, nameType);
	vector<byte> F(1);
	F[0] = boolValue ? 1 : 0;
	PDType->Data = F;
	return PDType;
}

unique_ptr<ProtocolDataType> ProtocolPackager::Pack(string stringValue, byte nameType)
{
	unique_ptr<ProtocolDataType> PDType = GenericPack(PD_STRING, nameType);
	PDType->Data = vector<byte>(stringValue.begin(), stringValue.end());
	return PDType;
}

////////////////////////////////////////////////////////////////////////////////////////////////

bool ProtocolPackager::UnpackByteVector(ProtocolDataType packedData, byte nameType, vector<byte> & Data)
{
	if ( (nameType == packedData.NameType) && (packedData.DataType == PD_BYTE_VECTOR))
	{
		Data = packedData.Data;
		return true;
	}
	else return false;
}

bool ProtocolPackager::UnpackByteVector_s(ProtocolDataType packedData, byte nameType, int Size, vector<byte> & Data)
{
	if (packedData.Data.size() == Size && (nameType == packedData.NameType) && (packedData.DataType == PD_BYTE_VECTOR))
	{
		Data = packedData.Data;
		return true;
	}
	else return false;
}

bool ProtocolPackager::UnpackByte(ProtocolDataType packedData, byte nameType, byte & Data)
{
	if ((packedData.Data.size() == 1) && (nameType == packedData.NameType) && (packedData.DataType == PD_BYTE))
	{
		Data = packedData.Data[0];
		return true;
	}
	else return false;
}

bool ProtocolPackager::UnpackInt16(ProtocolDataType packedData, byte nameType, int16_t &  Data)
{
	if (packedData.Data.size() == 2 && (nameType == packedData.NameType) && (packedData.DataType == PD_INT16))
	{
		Data = Conversions::VectorToInt16(packedData.Data);
		return true;
	}
	else return false;
}

bool ProtocolPackager::UnpackInt32(ProtocolDataType packedData, byte nameType, int32_t &  Data)
{
	if (packedData.Data.size() == 4 && (nameType == packedData.NameType) && (packedData.DataType == PD_INT32))
	{
		Data = Conversions::VectorToInt32(packedData.Data);
		return true;
	}
	else return false;
}

bool ProtocolPackager::UnpackInt64(ProtocolDataType packedData, byte nameType, int64_t &  Data)
{
	if (packedData.Data.size() == 8 && (nameType == packedData.NameType) && (packedData.DataType == PD_INT64))
	{
		Data = Conversions::VectorToInt64(packedData.Data);
		return true;
	}
	else return false;
}

bool ProtocolPackager::UnpackFloat(ProtocolDataType packedData, byte nameType, float & Data)
{
	if (packedData.Data.size() == 4 && (nameType == packedData.NameType) && (packedData.DataType == PD_FLOAT)) // Floats have 4 bytes
	{
		Data = Conversions::VectorToFloat(packedData.Data);
		return true;
	}
	else return false;
}

bool ProtocolPackager::UnpackDouble(ProtocolDataType packedData, byte nameType, double &  Data)
{
	if (packedData.Data.size() == 8 && (nameType == packedData.NameType) && (packedData.DataType == PD_DOUBLE)) // Doubles have 8 bytes
	{
		Data = Conversions::VectorToDouble(packedData.Data);
		return true;
	}
	else return false;
}

bool ProtocolPackager::UnpackBool(ProtocolDataType packedData, byte nameType, bool & Data)
{
	if (packedData.Data.size() == 1 && (nameType == packedData.NameType) && (packedData.DataType == PD_BOOL))
	{
		Data = (packedData.Data[0] == 0) ? false : true;
		return true;
	}
	else return false;
}

bool ProtocolPackager::UnpackString(ProtocolDataType packedData, byte nameType, string &  Data)
{
	if (packedData.Data.size() >= 0 && (nameType == packedData.NameType) && (packedData.DataType == PD_STRING))
	{
		Data = string(packedData.Data.begin(), packedData.Data.end());
		return true;
	}
	else return false;
}