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
        ConcurrentDictionary<Int64, Int64> TimeMachine = new ConcurrentDictionary<Int64, Int64>();
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

        public Int64 GetGlobalAvgTime()
        {
            List<Int64> timeVector = new List<Int64>();
            IEnumerator<KeyValuePair<long, long>> it =  TimeMachine.GetEnumerator();
       
            while(it.MoveNext())
            {
                KeyValuePair<long, long> kvp = it.Current;
                long myTime_i = kvp.Key;
                long otherTime_i = kvp.Value;

                long offSet = (nodeState.system_time - myTime_i);
                long offSetBalancedOtherTime_i = offSet + otherTime_i;

                timeVector.Add(offSetBalancedOtherTime_i);
            }

            Int64 timeSum = nodeState.system_time;

            int total = timeVector.Capacity;

            for (int i = 0; i < total; i++)
            {
                timeSum += timeVector[i];
            }

            Int64 avgTime = timeSum / (total + 1);
            return avgTime;
        }


    }
}
