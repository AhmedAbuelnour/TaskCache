using Sharpnado.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TaskCache
{
    public class TaskCaching
    {
        public static TaskCaching Current;
        static TaskCaching()
        {
            Current = new TaskCaching();
        }
        private TimeSpan InvokeForEach = TimeSpan.FromSeconds(5);
        private int NumberOfTries = 3;
        private Action<int> CurrentTriesProgress = (e) => {
            Debug.WriteLine(e);
        };
        private Action<bool> CurrentIsSuccessProgress = (e) =>
        {
            Debug.WriteLine(e);
        };
        public void DefaultInitializer(TimeSpan invokeForEach, int numberOfTries, Action<int> currentTriesProgress, Action<bool> currentIsSuccessProgress)
        {
            InvokeForEach = invokeForEach;
            NumberOfTries = numberOfTries;
            CurrentTriesProgress = currentTriesProgress; 
            CurrentIsSuccessProgress = currentIsSuccessProgress; 
        }
        private ActionScheduler Scheduler;
        private Queue<TaskWrapper> CachingStore;
        public TaskCaching()
        {
            CachingStore = new Queue<TaskWrapper>();
            TaskMonitorConfiguration.ConsiderCanceledAsFaulted = true;
            Scheduler = new ActionScheduler(InvokeForEach, async () =>
            {
                if (!IsAllSuccess)
                {
                    await WrapTaskAsync(GetFaultedTask());
                }
                else
                {
                    // All Tasks are done
                    Scheduler.Stop();
                }
            });
        }

        public async Task WrapTaskAsync(TaskWrapper taskWrapper)
        {
            TaskMonitor taskMonitorResult = TaskMonitor.Create(taskWrapper.WrappedTask, whenFaulted: (e) =>
            {
                CurrentIsSuccessProgress(false);
                if (++taskWrapper.NumberOfTries < NumberOfTries)
                {
                    CurrentTriesProgress(taskWrapper.NumberOfTries);
                    CachingStore.Enqueue(taskWrapper);
                    if (!Scheduler.IsStarted) Scheduler.Start();
                }
                else
                {
                    if (IsAllSuccess)
                    {
                        Scheduler.Stop();
                    }
                }
            }, whenSuccessfullyCompleted: (e) =>
            {
                CurrentIsSuccessProgress(true);
            });
            await taskMonitorResult.TaskCompleted;
        }
        private TaskWrapper GetFaultedTask() => CachingStore.Dequeue();
        public void RemoveFaultedTasks() => CachingStore.Clear();
        public bool IsAllSuccess { get => CachingStore.Count == 0; }
    }
}
