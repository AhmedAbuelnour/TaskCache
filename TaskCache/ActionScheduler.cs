using System;
using System.Timers;

namespace TaskCache
{
    internal class ActionScheduler
    {
        public bool IsStarted { get; set; }
        private readonly Timer CurrentTimer;
        private readonly Action _currentAction;
        public ActionScheduler(TimeSpan InvokeForEach, Action CurrentAction)
        {
            _currentAction = (CurrentAction ?? throw new ArgumentNullException("Action", "There is no action to run"));
            CurrentTimer = new System.Timers.Timer(InvokeForEach.TotalMilliseconds);
        }
        public void Start()
        {
            CurrentTimer.Elapsed += CurrentTimer_Elapsed;
            CurrentTimer.Start();
            IsStarted = true;
        }
        private void CurrentTimer_Elapsed(object sender, ElapsedEventArgs e)
            => _currentAction();
        public void Stop()
        {
            CurrentTimer.Stop();
            IsStarted = false;
        }
    }

}
