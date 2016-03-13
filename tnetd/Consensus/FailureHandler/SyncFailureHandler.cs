using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Consensus;
using TNetD.Network;
using TNetD.Network.Networking;
using TNetD.Nodes;

namespace TNetD.Consensus.FailureHandler
{
    class SyncFailureHandler<K,V,Z>: FailureHandler<K,V,Z> where V : struct, IConvertible
                                                           where Z : struct, IConvertible
    {
        NetworkPacketSwitch networkPacketSwitch;
        public override async Task HandleFail(ConcurrentDictionary<K, V> map1, ConcurrentDictionary<K, Z> map2, NetworkPacketSwitch networkPacketSwitch)
        {
            this.networkPacketSwitch = networkPacketSwitch;
            var nodesBehind = map1.Where(x => (ConsensusStates)(object)x.Value == ConsensusStates.Vote)
                                          .ToDictionary(x => x.Key, x => x.Value)
                                          .Keys
                                          .ToList();
            if (nodesBehind.Count() >= Constants.VOTE_MIN_SYNC_NODES)
            {
                await sendVoteRequests(nodesBehind.Cast<Hash>().ToList());
            }
        }
        async Task sendVoteRequests(List<Hash> friendNodes)
        {
            var tasks = friendNodes.Select(async hash =>
            {

                VoteResponseMessage voteResponse = new VoteResponseMessage();
                voteResponse.VotingState = VotingStates.STDone;
                voteResponse.ConsensusState = ConsensusStates.Vote;
                await networkPacketSwitch.SendAsync(hash, new NetworkPacket()
                {
                    Token = TNetUtils.GenerateNewToken(),
                    PublicKeySource = networkPacketSwitch.nodeConfig.PublicKey,
                    Data = voteResponse.Serialize(),
                    Type = PacketType.TPT_CONS_VOTE_RESPONSE
                });
            });

            await Task.WhenAll(tasks);
            Print("Time Warped");
        }
    }
}
