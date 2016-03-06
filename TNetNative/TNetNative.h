/*
*  @Author: Arpan Jati
*  @Version: 1.0
*  @Date: 29-30 Jan 2015
*/

#pragma once

public enum ReturnTypes
{
	RET_SUCCESS = 0,
	RET_ERROR_TIME_LESS = 101,
	RET_ERROR_COST_LESS = 102,
	RET_ERROR_COST_MORE = 103,
	RET_ERROR_SALTLEN_INVALID = 104,
	RET_ERROR_COST_MULTIPLE = 105,
	RET_ERROR_INVALID_OUT_HLEN = 106,

	RET_ERROR_INVALID_ARGUMENTS = 200
};

#include "rig.h"
#include "ed25519\ed25519.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace TNetNative {

	public ref class Rig2
	{
	public:

		static ReturnTypes Compute(array< Byte >^ % Output, array< Byte >^ input, array< Byte >^ salt, unsigned int t_cost, unsigned int m_cost)
		{
			Output = gcnew array< Byte >(64);

			unsigned char hsh[64];

			unsigned char* _in = (unsigned char*)malloc(input->Length);
			unsigned char* _salt = (unsigned char*)malloc(salt->Length);

			Marshal::Copy(input, 0, (IntPtr)_in, input->Length);
			Marshal::Copy(salt, 0, (IntPtr)_salt, salt->Length);

			int ret = PHS_FULL(hsh, 64, _in, input->Length, _salt, salt->Length, t_cost, m_cost);

			Marshal::Copy((IntPtr)hsh, Output, 0, 64);

			free(_in);

			return (ReturnTypes)ret;
		}


	};

	/*
	void ED25519_DECLSPEC ed25519_create_keypair(unsigned char *public_key, unsigned char *private_key, const unsigned char *seed);
	void ED25519_DECLSPEC ed25519_sign(unsigned char *signature, const unsigned char *message, size_t message_len, const unsigned char *public_key, const unsigned char *private_key);
	int ED25519_DECLSPEC ed25519_verify(const unsigned char *signature, const unsigned char *message, size_t message_len, const unsigned char *public_key);
	void ED25519_DECLSPEC ed25519_add_scalar(unsigned char *public_key, unsigned char *private_key, const unsigned char *scalar);
	void ED25519_DECLSPEC ed25519_key_exchange(unsigned char *shared_secret, const unsigned char *public_key, const unsigned char *private_key);
	*/

	public ref class Ed25519_Native
	{
	public:

		static void Sign(array< Byte >^ % signature, array< Byte >^ message, array< Byte >^ publicKey, array< Byte >^ expandedPrivateKey)
		{
			if (signature == nullptr)
				throw gcnew ArgumentNullException("signature.Array");
			if (signature->Length != 64)
				throw gcnew ArgumentException("signature.Count");
			if (expandedPrivateKey == nullptr)
				throw gcnew ArgumentNullException("expandedPrivateKey.Array");
			if (expandedPrivateKey->Length != 64)
				throw gcnew ArgumentException("expandedPrivateKey.Count");
			if (message == nullptr)
				throw gcnew ArgumentNullException("message.Array");

			unsigned char _signature[64];

			unsigned char* _message = (unsigned char*)malloc(message->Length);
			Marshal::Copy(message, 0, (IntPtr)_message, message->Length);

			unsigned char* _publicKey = (unsigned char*)malloc(publicKey->Length);
			Marshal::Copy(publicKey, 0, (IntPtr)_publicKey, publicKey->Length);

			unsigned char* _expandedPrivateKey = (unsigned char*)malloc(expandedPrivateKey->Length);
			Marshal::Copy(expandedPrivateKey, 0, (IntPtr)_expandedPrivateKey, expandedPrivateKey->Length);

			ed25519_sign(_signature, _message, message->Length, _publicKey, _expandedPrivateKey);

			Marshal::Copy((IntPtr)_signature, signature, 0, 64);

			free(_message);
			free(_publicKey);
			free(_expandedPrivateKey);
		}

		static array< Byte >^ Sign(array< Byte >^ message, array< Byte >^ publicKey, array< Byte >^ expandedPrivateKey)
		{
			array< Byte >^ signature = gcnew array< Byte >(64);
			Sign(signature, message, publicKey, expandedPrivateKey);
			return signature;
		}

	};


}
