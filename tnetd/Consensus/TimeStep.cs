
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
using System.Reactive.Linq;
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
        /// 50 Milliseconds
        /// </summary>
        public static readonly TimeSpan DEFAULT_TIMER_FASTSTEP = TimeSpan.FromMilliseconds(25);

        /// <summary>
        /// 500 Milliseconds
        /// </summary>
        public static readonly TimeSpan DEFAULT_TIMER_TIMESTEP = TimeSpan.FromMilliseconds(1000);

        /// <summary>
        /// 5000 Milliseconds
        /// </summary>
        public static readonly TimeSpan DEFAULT_TIMER_MAXSTEP = TimeSpan.FromMilliseconds(5000);

        /// <summary>
        /// Timesteps in MS
        /// </summary>
        int[] resolutions = new int[] { 100, 200, 300, 500, 1000, 1500, 2000, 3000, 5000, 10000 };

        public double currentTimeElapsed = 0;

        public bool IsNextStepSet { get; set; } = false;

        TimeSpan NextTimeStep { get; set; } = TimeSpan.FromMilliseconds(1000);

        DateTime LastTick;
        DateTime NextTime;

        /// <summary>
        /// Current resolution in Milliseconds
        /// </summary>
        public int CurrentResolution
        {
            get { return resolutions[CurrentResolutionIndex]; }
        }

        public int CurrentResolutionIndex { get; private set; } = 3;

        bool timerLockFree = true;

        DateTime InitialValue;

        NodeState nodeState;
        DateTime minNextTickTime = DateTime.UtcNow;

        public TimeStep(NodeState nodeState)
        {
            this.nodeState = nodeState;

            InitialValue = nodeState.CurrentNetworkTime;

            DateTime dt = nodeState.CurrentNetworkTime;

            minNextTickTime = nodeState.CurrentNetworkTime;

            TimeSpan tsp;

            Observable.Interval(DEFAULT_TIMER_FASTSTEP)
                .Subscribe(async x => await TimerVoting_FastElapsed(x));

            //NextTime = nodeState.CurrentNetworkTime.Add(new TimeSpan())
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

        public void SetNextTimeStep(TimeSpan timeStep)
        {
            if (timeStep >= DEFAULT_TIMER_FASTSTEP &&
                timeStep <= DEFAULT_TIMER_MAXSTEP)
            {
                NextTimeStep = timeStep;
                IsNextStepSet = true;
            }
        }

        void StepTime(TimeSpan stepTime)
        {
            currentTimeElapsed += stepTime.TotalMilliseconds;
        }

        async Task TimerInnerEvent()
        {
            if (EventEnabled && timerLockFree)
            {
                StepTime(DEFAULT_TIMER_FASTSTEP);

                if (TimeSpan.FromMilliseconds(currentTimeElapsed) > NextTimeStep)
                {
                    currentTimeElapsed = 0;

                    timerLockFree = false;

                    LastTick = nodeState.CurrentNetworkTime;

                    //LastTick.

                    await Step?.Invoke();

                    timerLockFree = true;
                }
            }
        }

        async Task TimerVoting_FastElapsed(object sender)
        {
            // Creates a tick at every timer resolution match.

            long resolutionTicks = CurrentResolution * TimeSpan.TicksPerMillisecond;

            long closenessTicks = (long)(DEFAULT_TIMER_FASTSTEP.Ticks * 2);

            DateTime timeNow = nodeState.CurrentNetworkTime;

            long timeTicks = timeNow.Ticks;

            if (((timeTicks % resolutionTicks) < closenessTicks))
            {
                if ((minNextTickTime < timeNow))
                {
                    // Next event can be atleast halfway from now and the next expected resolution event
                    minNextTickTime = timeNow.Add(TimeSpan.FromMilliseconds(CurrentResolution / 2));

                    // DisplayUtils.Display("+ " + timeNow.ToLongTimeString());
                    
                    if(EventEnabled)
                        await Step?.Invoke();
                    
                    //await TimerInnerEvent();
                }
                else
                {
                    // DisplayUtils.Display("-");
                }
            }
            else
            {
                //  DisplayUtils.Display(".");
            }
        }

        public void Initalize()
        {
            IsNextStepSet = false;
        }
    }
}
