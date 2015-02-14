
// @Author : Arpan Jati
// @Date: 13th Feb 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Network;
using TNetD.Network.Networking;

namespace TNetD.Nodes
{
    class NetworkHandlers
    {
        NodeConfig nodeConfig;
        NodeState nodeState;
        GlobalConfiguration globalConfiguration;

        SecureNetwork network = default(SecureNetwork);

        public NetworkHandlers(NodeConfig nodeConfig, NodeState nodeState, GlobalConfiguration globalConfiguration)
        {
            this.nodeConfig = nodeConfig;
            this.nodeState = nodeState;
            this.globalConfiguration = globalConfiguration;

            network = new SecureNetwork(nodeConfig);
            network.PacketReceived += network_PacketReceived;

            network.Initialize();

        }

        async public Task InitialConnectAsync()
        {
            await Task.Run(async () =>
            {
                // Connect to TrustedNodes
                List<Task> tasks = new List<Task>();

                foreach (KeyValuePair<Hash, NodeSocketData> kvp in globalConfiguration.TrustedNodes)
                {
                    // Make sure we are not connecting to self !!
                    if (kvp.Key != nodeConfig.PublicKey) 
                    {
                        if (!network.IsConnected(kvp.Key))
                        {
                            tasks.Add(SendInitialize(kvp.Key));
                        }
                    }
                }

                await Task.WhenAll(tasks);

            });
        }
        
        async Task SendInitialize(Hash publicKey)
        {
            await Task.Delay(Common.random.Next(500, 1000)); // Wait a random delay before connecting.
            NetworkPacketQueueEntry npqe = new NetworkPacketQueueEntry(publicKey, 
                new NetworkPacket(nodeConfig.PublicKey, PacketType.TPT_HELLO, new byte[0]));

            network.AddToQueue(npqe);
        }


        void network_PacketReceived(Hash publicKey, NetworkPacket packet)
        {
            DisplayUtils.Display(" Packet: " + packet.Type + " | From: " + publicKey + " | Data Length : " + packet.Data.Length);

            //packet.PublicKey_Src

        }

        public void Stop()
        {
            network.Stop();
        }

    }
}
