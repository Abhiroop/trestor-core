
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
using TNetD.Nodes;

namespace TNetD.Consensus
{
    public delegate Task TickHandler();
    
    class TimeStep
    {
        public event TickHandler Step;

        public bool EventEnabled { get; set; } = false;

        /// <summary>
        /// 45 Milliseconds
        /// </summary>
        public static readonly int DEFAULT_TIMER_FASTSTEP = 45;

        /// <summary>
        /// 500 Milliseconds
        /// </summary>
        public static readonly int DEFAULT_TIMER_TIMESTEP = 500;

        /// <summary>
        /// 5000 Milliseconds
        /// </summary>
        public static readonly int DEFAULT_TIMER_MAXSTEP = 5000;

        /// <summary>
        /// Timesteps in MS
        /// </summary>
        int[] resolutions = new int[] { 100, 200, 300, 500, 1000, 1500, 2000, 3000, 5000, 10000};

        public int currentTimeElapsed = 0;

        public bool IsNextStepSet { get; set; } = false;

        int NextTimeStep { get; set; } = 500;

        DateTime NextTime;

        /// <summary>
        /// Current resolution in Milliseconds
        /// </summary>
        public int CurrentResolution { get { return resolutions[CurrentResolutionIndex]; } }

        public int CurrentResolutionIndex { get; private set; } = 3;

        DateTime InitialValue = DateTime.UtcNow;

        NodeState nodeState;
               
        public TimeStep(NodeState nodeState)
        {
            this.nodeState = nodeState;

            InitialValue = nodeState.CurrentNetworkTime;
            
            DateTime dt = nodeState.CurrentNetworkTime;

            TimeSpan tsp;

            System.Reactive.Linq.Observable.Interval(TimeSpan.FromMilliseconds(DEFAULT_TIMER_FASTSTEP))
                .Subscribe(async x => await TimerVoting_FastElapsed(x));

            //NextTime = nodeState.CurrentNetworkTime.Add(new TimeSpan())
        }
        
        bool timerLockFree = true;

        async Task TimerVoting_FastElapsed(object sender)
        {
            if (EventEnabled && timerLockFree)
            {
                StepTime(DEFAULT_TIMER_FASTSTEP);

                if (IsComplete())
                {
                    timerLockFree = false;

                    await Step?.Invoke();

                    timerLockFree = true;
                }
            }
        }

        public void Initalize()
        {
            IsNextStepSet = false;
        }

        #region Resolution

        public void IncreaseResolution()
        {
            if ((CurrentResolutionIndex + 1) < resolutions.Length)
                CurrentResolutionIndex++;
        }

        public void DecreaseResolution()
        {
            if (CurrentResolutionIndex > 0)
                CurrentResolutionIndex--;
        }

        #endregion
        
        public void ResetTimeStepIfNotSet()
        {
            currentTimeElapsed = 0;

            if (!IsNextStepSet)
                SetNextTimeStep(DEFAULT_TIMER_TIMESTEP);
        }

        public void SetNextTimeStep(int timestepMilliseconds)
        {
            if (timestepMilliseconds >= DEFAULT_TIMER_FASTSTEP &&
                timestepMilliseconds <= DEFAULT_TIMER_MAXSTEP)
            {
                NextTimeStep = timestepMilliseconds;
                IsNextStepSet = true;
            }
        }

        public void StepTime(int stepTimeMilliseconds)
        {
            currentTimeElapsed += stepTimeMilliseconds;
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
