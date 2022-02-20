
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RLStatsClasses.Extensions
{
    public static class TaskDisposer
    {
        public static void DisposeTasks(IEnumerable<Task> tasks)
        {
            foreach (var task in tasks)
                task.Dispose();
        }
    }
}
