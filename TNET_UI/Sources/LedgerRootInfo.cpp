


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
	return Hash();
}

Hash LedgerRootInfo::getLedgerHash()
{
	return LedgerHash;
}

Hash LedgerRootInfo::getLastLedgerHash()
{
	return LastLedgerHash;
}

int64_t LedgerRootInfo::getLCLTime()
{
	return LCLTime;
}

int64_t LedgerRootInfo::getSequenceNumber()
{
	return SequenceNumber;
}