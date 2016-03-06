


#include "LedgerRootInfo.h"

LedgerRootInfo::LedgerRootInfo()
{

}

LedgerRootInfo::LedgerRootInfo(Hash _LedgerHash, Hash _LastLedgerHash, int64_t _LCLTime, int64_t _SequenceNumber)
{
	LedgerHash = _LedgerHash;
	LastLedgerHash = _LastLedgerHash;
	LCLTime = _LCLTime;
	SequenceNumber = _SequenceNumber;
}

Hash LedgerRootInfo::getID()
{
	return LedgerHash;
}