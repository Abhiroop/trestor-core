
// @Author : Arpan Jati
// @Date: June 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD
{
    public interface ISignableBase
    {
        byte [] GetSignatureData();
        void UpdateSignature(byte[] signature);
    }
}
