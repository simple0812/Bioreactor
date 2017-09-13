using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shunxi.Infrastructure.Common.Extension
{
    public static class TaskExtension
    {
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var cancellationCompletionSource = new TaskCompletionSource<bool>();

            using (cancellationToken.Register(() => cancellationCompletionSource.TrySetResult(true)))
            {
                if (task != await Task.WhenAny(task, cancellationCompletionSource.Task))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }

            return await task;
        }

        public static async Task WithCancellation(this Task task, CancellationToken cancellationToken)
        {
            var cancellationCompletionSource = new TaskCompletionSource<bool>();

            using (cancellationToken.Register(() => cancellationCompletionSource.TrySetResult(true)))
            {
                if (task != await Task.WhenAny(task, cancellationCompletionSource.Task))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }

            await task;
        }

        public static void IgnorCompletion(this Task task)
        {
        }
    }
}
