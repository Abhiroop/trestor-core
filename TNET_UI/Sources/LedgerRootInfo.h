/*
*
*  @Author: Aritra Dhar + Arpan Jati
*  @Version: 1.0
*/
#include "RootDataType.h"
#include <stdint.h>

#ifndef LedgerRootInfo_H
#define LedgerRootInfo_H

class LedgerRootInfo : public RootDataType
{
public:	
	
	Hash LedgerHash;
	Hash LastLedgerHash;
	int64_t LCLTime;
	int64_t SequenceNumber;

	LedgerRootInfo();
	LedgerRootInfo(Hash LedgerHash, Hash LastLedgerhash, int64_t LCLTime, int64_t SequenceNumber);

	Hash getID();

};

#endif