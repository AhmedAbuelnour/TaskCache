using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TaskCache
{
    internal class TaskCachingResult
    {
        public Func<Task> CachedTask { get; set; }
        public bool IsSuccessfulReturn { get; set; }
    }
    internal class TaskCachingResult<Tout>
    {
        public Func<Task, Tout> CachedTask { get; set; }
        public bool IsSuccessfulReturn { get; set; }
    }
}
