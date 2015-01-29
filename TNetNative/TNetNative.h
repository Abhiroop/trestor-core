// TNetNative.h

#pragma once

public enum ReturnTypes
{
	RET_SUCCESS = 0,
	RET_ERROR_TIME_LESS = 101,
	RET_ERROR_COST_LESS = 102,
	RET_ERROR_COST_MORE = 103,
	RET_ERROR_SALTLEN_INVALID = 104,
	RET_ERROR_COST_MULTIPLE = 105,
	RET_ERROR_INVALID_OUT_HLEN = 106
};

#include "rig.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace TNetNative {

	public ref class Rig2_Native
	{		

	public:

		ReturnTypes Rig2(array< Byte >^ % Output, array< Byte >^ input, array< Byte >^ salt, unsigned int t_cost, unsigned int m_cost)
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

			return (ReturnTypes) ret;
		}


	};
}
