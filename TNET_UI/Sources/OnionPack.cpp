//#include "OnionPack.h"
//
//
//string Onionpack::makePack(string data, onion_peer_entry* peer_entries, int peer_count, string rec_addr, onion_peer_entry receiver)
//{
//	string final_onion_packet;
//	//here we need to first do a secure key exchange with the intermediate nodes.
//	//Using the key, we will setup a secure channel inbetwwn the intermediate nodes.
//	//Then using this secure channel we will setup final key exchange between the sender and the final receiver.
//	
//	//do the AES
//	byte IV[16];
//
//	size_t length = 0;
//	//Base_64::decode(rec_exchanged_key, 32, &length);
//
//	memset(IV, 0, 16);
//
//	try
//	{
//		CBC_Mode< AES >::Encryption e;
//		//e.SetKeyWithIV(Key, 32, IV);
//	}
//	catch (const CryptoPP::Exception& e)
//	{
//		cerr << "Error happeded in Onion AES" << endl;
//		cerr << e.what() << endl;
//	}
//	
//	return final_onion_packet;
//}
