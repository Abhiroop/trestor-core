using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace TNetD.Interop
{
    public class Native
    {

        //int Rig2(void *out, size_t outlen, const void *in, size_t inlen,
	    //const void *salt, size_t saltlen, unsigned int t_cost, unsigned int m_cost)

        [DllImport("TNetNative.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern int Rig2([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]  ref byte[] Out, int outLength, byte[] In, long inLength, 
            byte[] Salt, long saltLength, int t_Cost, int m_Cost);

    }
}
