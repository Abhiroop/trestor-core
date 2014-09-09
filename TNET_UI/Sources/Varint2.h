
// @Author : Arpan Jati
// @Date : 7th Sept 2014

#ifndef VARINT_2
#define VARINT_2

#include "Utils.h"
#include "Constants.h"

/// <summary>
/// A variable length integer with, the following format:
/// 
/// [length in bytes [4 bits], [msb....][........][.......] ..... [.... lsb]
/// 
/// </summary>
class Varint2
{
public:

	static int GetBitLength(int64_t value);

	static vector<byte> Encode(int64_t value);
		
	static bool Decode(vector<byte> value, int startIndex, int & length, int64_t & result);
	
	static bool Decode(vector<byte> value, int64_t result);	

	static void TestVarint2()
	{
		//Random rnd = new Random();

		for (int i = 0; i < 10000000; i++)
		{
			long Long = rand();
			int bitLength = GetBitLength(Long);
			//byte[] enc_old = Encode(Long);
			vector<byte> enc = Encode(Long);

			int reslen;
			int64_t Decoded;
			bool OK = Decode(enc, 0, reslen, Decoded);

			if ((Long != Decoded))
			{
				//Console.Write(Long + " - " + " - " + HexUtil.ToString(enc));
				//printf(" TEST FAIL ---------------------------");
				MessageQueue.push(" TEST FAIL ---------------------------");
				return;
			}

			//Console.Write(Long + " - " + bitLength + " - " + HexUtil.ToString(enc));
			//Console.WriteLine((Long == Decoded) ? " : Match" : " : FAIL ---------------------------");
		}

		MessageQueue.push(" TEST FINISHED ---------------------------");
	}

};


#endif













