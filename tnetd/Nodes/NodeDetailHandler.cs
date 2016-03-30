
// @Author : Arpan Jati
// @Date: March 29, 2016

using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace TNetD.Nodes
{
    /// <summary>
    /// Handles saving and loading of node statistics to and from a JSON file.
    /// </summary>
    class NodeDetailHandler
    {
        NodeConfig nodeConfig;
        NodeState nodeState;

        public NodeDetailHandler(NodeConfig nodeConfig, NodeState nodeState)
        {
            this.nodeConfig = nodeConfig;
            this.nodeState = nodeState;
        }

        /// <summary>
        /// Save 'NodeDetails' to file.
        /// </summary>
        /// <returns></returns>
        public async Task Save()
        {
            try
            {
                using (FileStream fs = new FileStream(nodeConfig.Path_NodeDetails,
                    FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        var json = JsonConvert.SerializeObject(nodeState.NodeInfo.NodeDetails,
                            Common.JSON_SERIALIZER_SETTINGS);

                        await sw.WriteAsync(json);

                        await sw.FlushAsync();
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Load 'NodeDetails' from file.
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {
            try
            {
                using (FileStream fs = new FileStream(nodeConfig.Path_NodeDetails,
                    FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        string json = await sr.ReadToEndAsync();

                        if (json.Length != 0)
                        {
                            nodeState.NodeInfo.NodeDetails.Deserialize(json);
                        }
                    }
                }
            }
            catch { }
        }
    }
}
