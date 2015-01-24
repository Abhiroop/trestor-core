using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TNetD.Nodes;

namespace TNetD.Time
{
    class TimeSync
    {
        NodeState nodeState;
        ConcurrentDictionary<Int64, Int64> timeMachine = new ConcurrentDictionary<Int64, Int64>();
        public TimeSync(NodeState nodeState)
        {
            this.nodeState = nodeState;
        }    

        public void SendTimeRequest()
        {
            ConcurrentBag<Hash> ConnectedValidators = nodeState.ConnectedValidators;

            Hash[] arr = ConnectedValidators.ToArray();

	        for (int i = 0; i < (int) arr.Length; i++)
	        {
		        Hash ValidatorPK = arr[i];
		        Hash token =  TNetUtils.GenerateNewToken();
		

		        if (!nodeState.timeMap.ContainsKey(ValidatorPK))
		        {
                    TimeStruct ts;
                    ts.sendTime = nodeState.system_time;
			        ts.receivedTime = 0;
			        ts.TimeFromValidator = 0;
			        ts.timeDifference = 0;
			        ts.token = token;
                    nodeState.timeMap.AddOrUpdate(ValidatorPK, ts, (oldkey, oldvalue) => ts);
		        }
	         }
        }
    }
}
