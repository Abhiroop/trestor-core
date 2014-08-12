//
//
//// g++ -g3 -ggdb -O0 -DDEBUG -Wall -Wextra cryptopp-test.cpp -o cryptopp-test.exe -lcryptopp -lpthread
//// g++ -g -O2 -DNDEBUG -Wall -Wextra cryptopp-test.cpp -o cryptopp-test.exe -lcryptopp -lpthread
//#include "dll.h"
//
//USING_NAMESPACE(CryptoPP)
//USING_NAMESPACE(std)
//
//#include <iostream>
//using std::cout;
//using std::cerr;
//using std::endl;
//#include<time.h>
//#include <string>
//using std::string;
//
//#include <stdexcept>
//using std::runtime_error;
//
//#include <cstdlib>
//using std::exit;
//
//using CryptoPP::AutoSeededRandomPool;
//using CryptoPP::ECP;
//using CryptoPP::ECDSA;
//
//
//using CryptoPP::SHA1;
//
//using CryptoPP::ByteQueue;
//
//#include "cryptopp/oids.h"
//using CryptoPP::OID;
//
//// ASN1 is a namespace, not an object
//
//using namespace CryptoPP::ASN1;
//using CryptoPP::Integer;
//
//int main3(int, char**) {
//
//	AutoSeededRandomPool prng;
//	ByteQueue privateKey, publicKey;
//
//	string message = "Ghanta";
//
//	//////////////////////////////////////////////////////
//
//	
//	bool result = 0;
//	clock_t init = clock();
//	for (int i = 0; i < 500; i++)
//	{
//		//////////////////////////////////////////////////////    
//		// Generate private key
//		ECDSA<ECP, SHA1>::PrivateKey privKey;
//		privKey.Initialize(prng, secp256r1());
//		privKey.Save(privateKey);
//
//		// Create public key
//		ECDSA<ECP, SHA1>::PublicKey pubKey;
//		privKey.MakePublicKey(pubKey);
//		pubKey.Save(publicKey);
//		// Load private key (in ByteQueue, PKCS#8 format)
//		ECDSA<ECP, SHA1>::Signer signer(privateKey);
//
//		// Determine maximum size, allocate a string with that size
//		size_t siglen = signer.MaxSignatureLength();
//		string signature(siglen, 0x00);
//
//		// Sign, and trim signature to actual size
//		siglen = signer.SignMessage(prng, (const byte*)message.data(), message.size(), (byte*)signature.data());
//		signature.resize(siglen);
//
//		//////////////////////////////////////////////////////    
//
//		// Load public key (in ByteQueue, X509 format)
//		ECDSA<ECP, SHA1>::Verifier verifier(publicKey);
//
//
//		result = verifier.VerifyMessage((const byte*)message.data(), message.size(), (const byte*)signature.data(), signature.size());
//
//	}
//	clock_t final = clock();
//	if (result)
//		cout << "Verified signature on message" << endl;
//	else
//		cerr << "Failed to verify signature on message" << endl;
//
//	printf("%f ms", (float)(final - init));
//	getchar();
//	return 0;
//}
