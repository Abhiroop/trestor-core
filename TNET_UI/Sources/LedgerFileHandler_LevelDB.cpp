
/*
*
*  @Author: Arpan Jati
*  @Version: 1.0
*  @Date: December 2014
*/

#include "LedgerFileHandler_LevelDB.h"
#include "Base64.h"

LedgerFileHandler_LevelDB::LedgerFileHandler_LevelDB()
{
	
}

LedgerRootInfo LedgerFileHandler_LevelDB::LoadLRIinfo()
{	
	LedgerRootInfo lri;

	CSimpleIniA headerIni;
	headerIni.SetUnicode();
	headerIni.LoadFile((LedgerDB_FileName + ".ini").data());

	bool bHasMultipleValues = true;

	headerIni.SetValue("LastCloseInfo", "LedgerHash", ToBase64String(lri.LedgerHash).data());
	headerIni.SetValue("LastCloseInfo", "LastLedgerHash", ToBase64String(lri.LastLedgerHash).data());

	char _lcl_data[30];
	sprintf(_lcl_data, "%ulld", lri.LCLTime);

	char _SequenceNumber_data[30];
	sprintf(_SequenceNumber_data, "%ulld", lri.SequenceNumber);

	headerIni.SetValue("LastCloseInfo", "LastLedgerHash", _lcl_data);
	headerIni.SetValue("LastCloseInfo", "SequenceNumber", _SequenceNumber_data);

	return lri;
}

void LedgerFileHandler_LevelDB::SaveLRIinfo(LedgerRootInfo lri)
{	
	CSimpleIniA headerIni;
	headerIni.SetUnicode();
	headerIni.LoadFile((LedgerDB_FileName + ".ini").data());

	bool bHasMultipleValues = true;

	lri.LedgerHash = Base64ToHash(headerIni.GetValue("LastCloseInfo", "LedgerHash", NULL, &bHasMultipleValues));
	lri.LastLedgerHash = Base64ToHash(headerIni.GetValue("LastCloseInfo", "LastLedgerHash", NULL, &bHasMultipleValues));
	lri.LCLTime = atoll(headerIni.GetValue("LastCloseInfo", "LCLTime", NULL, &bHasMultipleValues));
	lri.SequenceNumber = atoll(headerIni.GetValue("LastCloseInfo", "SequenceNumber", NULL, &bHasMultipleValues));	
}

LedgerFileHandler_LevelDB::LedgerFileHandler_LevelDB(HashTree< AccountInfo, LedgerRootInfo > accountTree, string ledgerDB_FileName)
{
	LedgerDB_FileName = ledgerDB_FileName;

	options.create_if_missing = true;
	leveldb::Status status = leveldb::DB::Open(options, "/" + LedgerDB_FileName, &db);

	

	assert(status.ok());
}

HashTree< AccountInfo, LedgerRootInfo > LedgerFileHandler_LevelDB::DBToTree()
{
	LedgerRootInfo _LRI = LoadLRIinfo();
	
	HashTree<AccountInfo, LedgerRootInfo> hashTree(_LRI);





	return hashTree;
}