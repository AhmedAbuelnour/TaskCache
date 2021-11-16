using Sharpnado.Tasks;
using System;
using System.Collections.Generic;
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
        private TimeSpan InvokeForEach { get; set; } = TimeSpan.FromSeconds(5);
        public void SetInvokerTimeInterval(TimeSpan timeSpan)
        {
            InvokeForEach = timeSpan;
        }
        private ActionScheduler Scheduler;
        private Queue<Func<Task>> CachingStore { get; set; }
        public TaskCaching()
        {
            CachingStore = new Queue<Func<Task>>();
            TaskMonitorConfiguration.ConsiderCanceledAsFaulted = true;
            Scheduler = new ActionScheduler(InvokeForEach, async () =>
            {
                TaskCachingResult taskResult = GetFaultedTask();
                if (taskResult.IsSuccessfulReturn)
                {
                    await WrapTaskAsync(taskResult.CachedTask);
                }
                else
                {
                    // All Tasks are done
                    Scheduler.Stop();
                }
            });
        }


        public async Task<bool> WrapTaskAsync(Func<Task> task)
        {
            TaskMonitor taskMonitorResult = TaskMonitor.Create(task, whenFaulted: (e) =>
            {
                CachingStore.Enqueue(task);
                if (!Scheduler.IsStarted) Scheduler.Start();
            });
            await taskMonitorResult.TaskCompleted;
            return taskMonitorResult.IsSuccessfullyCompleted;
        }
        public async Task<(T Result,bool IsSuccessful)> WrapTaskAsync<T>(Func<Task<T>> task)
        {
            TaskMonitor<T> taskMonitorResult = TaskMonitor<T>.Create(task, whenFaulted: (e) =>
            {
                CachingStore.Enqueue(task);
                if (!Scheduler.IsStarted) Scheduler.Start();
            });
            return (await taskMonitorResult.TaskWithResult.ConfigureAwait(false), taskMonitorResult.IsSuccessfullyCompleted);
        }
        private TaskCachingResult GetFaultedTask()
        {
            if (CachingStore.TryDequeue(out Func<Task> dequeueTask))
            {
                return new TaskCachingResult
                {
                    CachedTask = dequeueTask,
                    IsSuccessfulReturn = true
                };
            }
            else
            {
                return new TaskCachingResult { IsSuccessfulReturn = false };
            }
        }
        public void RemoveFaultedTasks() => CachingStore.Clear();
        public bool IsAllSuccess { get => CachingStore.Count == 0; }
    }
}
