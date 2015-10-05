
//
//  @Author: Arpan Jati
//  @Date: October 2015
//
//  Dynamically scales closetime frequency and calculates next timestep.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Consensus
{
    class TimeStep
    {
        /// <summary>
        /// Timesteps in MS
        /// </summary>
        int[] Resolutions = new int[] { 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000 };

        int currentResolution = 3;

        public int CurrentResolution { get { return currentResolution; }}

        public void IncreaseResolution()
        {
            if ((currentResolution + 1) < Resolutions.Length)
                currentResolution++;
        }

        public void DecreaseResolution()
        {
            if (currentResolution > 0)
                currentResolution--;
        }

        public TimeStep()
        {

        }
                

    }
}
