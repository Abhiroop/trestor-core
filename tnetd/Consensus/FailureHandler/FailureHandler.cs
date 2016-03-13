using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using TNetD.Helpers;
using TNetD.Nodes;

namespace TNetD.Consensus.FailureHandler
{
    abstract class FailureHandler<K,V,Z>: Printer where V:struct, IConvertible 
                                                  where Z:struct, IConvertible
    {
        public virtual Task HandleFail(ConcurrentDictionary<K,V> map1, ConcurrentDictionary<K, Z> map2, NetworkPacketSwitch networkPacketSwitch)
        {
            Print("Faster node");
            return Task.FromResult(default(object));
        }
    }
}
