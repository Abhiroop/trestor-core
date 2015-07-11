using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TNetD.Network.Networking;
using TNetD.Nodes;

namespace TNetD
{
    public enum Verbosity { NoDisplay, Errors, Warning, Info, ExtraInfo };

    class TNetUtils
    {
        public static Hash GenerateNewToken()
        {
            byte[] randBytes = new byte[Common.NETWORK_TOKEN_LENGTH];            
            Common.rngCsp.GetBytes(randBytes);
            return new Hash(randBytes);
        }

        public static void WaitForExit()
        {
            bool exit = false;
            do
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.X && cki.Modifiers == ConsoleModifiers.Control)
                {
                    exit = true;
                }
                Thread.Sleep(100);
            }
            while (!exit);
        }

        public static string GetNodeConnectionInfoString(IEnumerable<Node> nodes)
        {
            StringBuilder connData = new StringBuilder();

            foreach (var nd in nodes)
            {
                connData.AppendLine("\n\n\n NODE ID " + nd.nodeConfig.NodeID + "   KEY: " + nd.PublicKey);
                connData.AppendLine(" Ledger Hash : " + nd.nodeState.Ledger.GetRootHash());

                connData.AppendLine(" ---- ConnectedValidators ---- ");

                foreach (var conn in nd.nodeState.ConnectedValidators)
                {
                    connData.AppendLine("\t" + conn.Key.ToString() + "  " +
                        ((conn.Value.Direction == ConnectionDirection.Incoming) ? "<--" : "-->") + "  " +
                        (conn.Value.IsTrusted ? "Trusted" : ""));
                }
            }

            return connData.ToString();
        }

    }
}
