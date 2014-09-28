/*
*
*  @Author: Aritra Dhar + Arpan Jati
*  @Version: 1.0
*/
#include"RootDataType.h"
#include<stdint.h>

#ifndef LedgerRootInfo_H
#define LedgerRootInfo_H

class LedgerRootInfo : public RootDataType
{
	Hash LedgerHash;
	Hash LastLedgerHash;
	int64_t LCLTime;
	int64_t SequenceNumber;

public:

	LedgerRootInfo(Hash LedgerHash, Hash LastLedgerhash, int64_t LCLTime, int64_t SequenceNumber);

	Hash getLedgerHash();
	Hash getLastLedgerHash();
	int64_t getLCLTime();
	int64_t getSequenceNumber();

};

#endif