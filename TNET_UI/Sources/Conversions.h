
/*
*  @Author: Arpan Jati
*  @Version: 1.0
*/

#ifndef Conversions_H
#define Conversions_H

#include <stdint.h>
#include <vector>
#include "Types.h"

using namespace std;

class Conversions
{
public:
	
	static vector<byte> Int16ToVector(int16_t data);

	static vector<byte> Int32ToVector(int32_t data);

	static vector<byte> Int64ToVector(int64_t data);

	static vector<byte> FloatToVector(float data);

	static vector<byte> DoubleToVector(double data);


	static int16_t VectorToInt16(vector<byte> data);

	static int32_t VectorToInt32(vector<byte> data);

	static int64_t VectorToInt64(vector<byte>  data);

	static float VectorToFloat(vector<byte> data);

	static double VectorToDouble(vector<byte> data);

};

#endif