using System;
using System.Threading.Tasks;

namespace TaskCache
{
    public class TaskWrapper
    {
        public Func<Task> WrappedTask { get; set; }
        public int NumberOfTries { get; internal set; }
    }
}
