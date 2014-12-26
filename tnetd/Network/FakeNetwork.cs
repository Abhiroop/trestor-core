using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TNetD.Network
{
    public delegate void PacketReceivedHandler(NetworkPacket packet);

    class FakeNetwork
    {
        
        //public event PacketReceivedHandler PacketReceived;

        ConcurrentDictionary<Hash, PacketReceivedHandler> Listeners = new ConcurrentDictionary<Hash, PacketReceivedHandler>();
        ConcurrentQueue<NetworkPacketQueueEntry> QueuedPackets = new ConcurrentQueue<NetworkPacketQueueEntry>();

        AutoResetEvent autoEvent = new AutoResetEvent(false);
        Timer stateTimer;

        void TimerCallback(Object obj)
        {
            while (QueuedPackets.Count > 0)
            {
                NetworkPacketQueueEntry npqe;
                if (QueuedPackets.TryDequeue(out npqe))
                {
                    if (Listeners.ContainsKey(npqe.PublicKey_Dest))
                    {
                        Listeners[npqe.PublicKey_Dest].Invoke(npqe.Packet);
                    }
                }
                else break;
            }
        }

        public FakeNetwork()
        {
            stateTimer = new Timer(TimerCallback, autoEvent, 100, 25);
        }

        public void SendPacket(NetworkPacketQueueEntry npqe)
        {
            QueuedPackets.Enqueue(npqe);
        }

        public void AttachListener(Hash PublicKey, PacketReceivedHandler handler)
        {
            if (!Listeners.ContainsKey(PublicKey))
            {
                if(!Listeners.TryAdd(PublicKey, handler))
                {
                    throw new Exception("Listener could not be attached.");
                }
            }
            else
            {
                throw new ArgumentException("Element already exists.");
            }
        }
        
    }
}
