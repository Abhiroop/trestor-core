//
//#include "ed25519\SHA512.h"
//
//#include "PrivateKeyManage.h"
//
//USING_NAMESPACE(CryptoPP)
//USING_NAMESPACE(std)
//
//
//unsigned char public_key[32], private_key[64], seed[32], scalar[32];
//unsigned char* password;
//unsigned char nonce[32], symmetic_key[32];
//
//// Blob Format
//// Ek(SEED[32] SIG[64]) NONCE[16]
//
//unsigned char user_data[32 + 64 + 16];
//
//int password_length = 0;
//
//// considers valid nonce
//int PrivateKeyManage::DecryptUserData(void)
//{
//	byte Key[64];
//	byte IV[16];
//	memset(IV, 0, 16);
//	byte Seed[32];
//	byte S[64];
//
//	byte* C = (byte*)user_data;
//	byte* S_ = (byte*)(user_data + 32);
//
//	unsigned char * pass_nonce = (unsigned char*)malloc(sizeof(unsigned char)* (password_length + 16));
//
//	memcpy(pass_nonce, password, password_length);
//	memcpy(pass_nonce + password_length, nonce, 16);
//	sha512(pass_nonce, password_length + 16, Key);
//
//	try
//	{
//		CBC_Mode< AES >::Decryption d;
//		d.SetKeyWithIV(Key, 32, IV);
//		d.ProcessData(Seed, C, 32);
//		d.SetKeyWithIV(Key + 32, 32, IV);
//		d.ProcessData(S, S_, 64);
//	}
//	catch (const CryptoPP::Exception& e)
//	{
//		return 0;
//	}
//
//	ed25519_create_keypair(public_key, private_key, Seed);
//
//	free(pass_nonce);
//
//	return ed25519_verify(S, C, 32, public_key);
//}
//
//
//PrivateKeyManage::PrivateKeyManage()
//{
//	ed25519_create_seed(nonce);
//}
//
//PrivateKeyManage::PrivateKeyManage(byte* user_data_blob, byte* password, int password_length)
//{
//	memcpy(user_data, user_data_blob, 32 + 64 + 16);
//	SetPassword(password, password_length);
//	memcpy(nonce, user_data_blob + 32 + 64, 16);
//	if (!DecryptUserData())
//	{
//		//throw std::exception("Invalid Blob or password", 1);
//		printf("Problem");
//	}
//}
//
//PrivateKeyManage::~PrivateKeyManage()
//{
//	free(password);
//	// SECURE DELETE VARIABLES
//	for (int i = 0; i < 32; i++)
//	{
//		public_key[i] = 0x00;
//		private_key[i] = 0x00;
//		private_key[i + 32] = 0x00;
//		nonce[i] = 0x00;
//		symmetic_key[i] = 0x00;
//	}
//}
//
//void PrivateKeyManage::SetPassword(unsigned char* Password, int Length)
//{
//	password = (unsigned char*)malloc(sizeof(unsigned char)* Length);
//	password_length = Length;
//}
//
//void PrivateKeyManage::setPublicKey(unsigned char pk[])
//{
//	memcpy(public_key, pk, 32);
//}
//void PrivateKeyManage::setPrivateKey(unsigned char sk[])
//{
//	memcpy(private_key, sk, 32);
//}
//void PrivateKeyManage::getPublicKey(unsigned char* pk)
//{
//	memcpy(pk, public_key, 32);
//}
//void PrivateKeyManage::getPrivateKey(unsigned char* sk)
//{
//	memcpy(sk, private_key, 64);
//}
//
//void PrivateKeyManage::getuserData(unsigned char* ud)
//{
//	memcpy(ud, user_data, 32 + 64 + 16);
//}
//
//void PrivateKeyManage::generateKeypairECC(void)
//{
//	ed25519_create_seed(seed);
//	ed25519_create_keypair(public_key, private_key, seed);
//}
//
//void PrivateKeyManage::prepareUserData(void)
//{
//	byte Key[64];
//	byte IV[16];
//	memset(IV, 0, 16);
//
//	generateKeypairECC();
//
//	unsigned char * pass_nonce = (unsigned char*)malloc(sizeof(unsigned char)* (password_length + 16));
//
//	memcpy(pass_nonce, password, password_length);
//	memcpy(pass_nonce + password_length, nonce, 16);
//	sha512(pass_nonce, password_length + 16, Key);
//
//	try
//	{
//		CBC_Mode< AES >::Encryption e;
//		e.SetKeyWithIV(Key, 32, IV);
//
//		byte C[32];
//		byte S_[64];
//		byte S[64];
//
//		e.ProcessData(C, seed, 32);
//
//		ed25519_sign(S, C, 32, public_key, private_key);
//
//		e.SetKeyWithIV(Key + 32, 32, IV);
//
//		e.ProcessData(S_, S, 64);
//
//		memcpy(user_data, C, 32);
//		memcpy(user_data + 32, S_, 64);
//		memcpy(user_data + 96, nonce, 16);
//
//	}
//	catch (const CryptoPP::Exception& e)
//	{
//		cerr << e.what() << endl;
//		//exit(1);
//	}
//
//	free(pass_nonce);
//
//}
