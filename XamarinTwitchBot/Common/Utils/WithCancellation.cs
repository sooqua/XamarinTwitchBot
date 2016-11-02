// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   CancellationToken extension.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Common
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// CancellationToken extension.
    /// </summary>
    internal static partial class Utils
    {
        public static Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            return task.IsCompleted // fast-path optimization
                       ? task
                       : task.ContinueWith(completedTask => completedTask.GetAwaiter().GetResult(), cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
    }
}