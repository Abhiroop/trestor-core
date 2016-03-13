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
    class VotingFailureHandler<K,V,Z>:FailureHandler<K,V,Z> where V : struct, IConvertible
                                                            where Z : struct, IConvertible
    {
        NetworkPacketSwitch networkPacketSwitch;
        VotingStates currentVotingState;

        public VotingFailureHandler(VotingStates currentVotingState)
        {
            this.currentVotingState = currentVotingState;
        }

        public override async Task HandleFail(ConcurrentDictionary<K, V> map1, ConcurrentDictionary<K, Z> map2,NetworkPacketSwitch networkPacketSwitch)
        {
            this.networkPacketSwitch = networkPacketSwitch;
            await Task.FromResult(default(object));
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
