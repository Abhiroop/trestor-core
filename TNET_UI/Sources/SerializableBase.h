
// @Author : Arpan Jati
// @Date : 17th Aug 2014

#ifndef SerializableBase_H
#define SerializableBase_H

#include "Types.h"
#include <vector>

using namespace std;

class SerializableBase
{
public:
	virtual vector<byte> Serialize();
	virtual void Deserialize(vector<byte> Data);
};


#endif
