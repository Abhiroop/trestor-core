
//
//  @Author: Arpan Jati
//  @Date: October 2015
//
//  Dynamically scales closetime frequency and calculates next timestep.
//  Manages voting event timings
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
        public static readonly int DEFAULT_TIMER_FASTSTEP = 45;
        public static readonly int DEFAULT_TIMER_TIMESTEP = 500;
        public static readonly int DEFAULT_TIMER_MAXSTEP = 5000;

        /// <summary>
        /// Timesteps in MS
        /// </summary>
        int[] Resolutions = new int[] { 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000 };

        public int currentTimeElapsed = 0;

        public bool IsNextStepSet { get; set; } = false;

        int NextTimeStep { get; set; } = 500;

        public int CurrentResolution { get; private set; } = 3;

        public TimeStep()
        {

        }

        public void IncreaseResolution()
        {
            if ((CurrentResolution + 1) < Resolutions.Length)
                CurrentResolution++;
        }

        public void DecreaseResolution()
        {
            if (CurrentResolution > 0)
                CurrentResolution--;
        }
        
        public void Initalize()
        {
            IsNextStepSet = false;
        }

        public void ResetTimeStepIfNotSet()
        {
            currentTimeElapsed = 0;

            if (!IsNextStepSet)
                SetNextTimeStep(DEFAULT_TIMER_TIMESTEP);
        }
        
        public void SetNextTimeStep(int timestepMs)
        {
            if (timestepMs >= DEFAULT_TIMER_FASTSTEP &&
                timestepMs <= DEFAULT_TIMER_MAXSTEP)
            {
                NextTimeStep = timestepMs;
                IsNextStepSet = true;
            }
        }

        public void Step(int StepTime)
        {
            currentTimeElapsed += StepTime;            
        }

        /// <summary>
        /// Returns true if the current elapsed time exceeds the set next time step;
        /// </summary>
        /// <returns></returns>
        public bool IsComplete()
        {
            if (currentTimeElapsed > NextTimeStep)
            {
                currentTimeElapsed = 0;
                return true;
            }

            return false;
        }

    }
}
