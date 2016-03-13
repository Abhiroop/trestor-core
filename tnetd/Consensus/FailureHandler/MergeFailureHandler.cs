using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Network;
using TNetD.Network.Networking;
using TNetD.Nodes;

namespace TNetD.Consensus.FailureHandler
{
    class MergeFailureHandler<K,V,Z> : FailureHandler<K,V,Z> where V : struct, IConvertible
                                                             where Z : struct, IConvertible
    {
        NetworkPacketSwitch networkPacketSwitch;
        public override async Task HandleFail(ConcurrentDictionary<K, V> map1, ConcurrentDictionary<K, Z> map2, NetworkPacketSwitch networkPacketSwitch)
        {
            this.networkPacketSwitch = networkPacketSwitch;
            var nodesBehind = map1.Where(x => (ConsensusStates)(object)x.Value == ConsensusStates.Sync)
                                          .ToDictionary(x => x.Key, x => x.Value)
                                          .Keys
                                          .ToList();
            if (nodesBehind.Count()>=Constants.VOTE_MIN_SYNC_NODES)
            {
                await sendSyncResponse(nodesBehind.Cast<Hash>().ToList());
            }
        }

        async Task sendSyncResponse(List<Hash> friendNodes)
        {

            var tasks = friendNodes.Select(async hash =>
            {
                SyncMessage syncResponse = new SyncMessage();
                Hash token = TNetUtils.GenerateNewToken();
                await networkPacketSwitch.SendAsync(hash, new NetworkPacket()
                {
                    Token = token,
                    PublicKeySource = networkPacketSwitch.nodeConfig.PublicKey,
                    Data = syncResponse.Serialize(),
                    Type = PacketType.TPT_CONS_SYNC_RESPONSE
                });
            });
            await Task.WhenAll(tasks);
            Print("Time Warped!");
        }


    }
}
