using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Consensus.FailureHandler
{
    class DefaultFailureHandler<K,V,Z>:FailureHandler<K,V,Z> where V : struct, IConvertible
                                                             where Z : struct, IConvertible
    {
    }
}
