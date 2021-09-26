using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Brotils
{
    public static class TaskExtensions
    {
        public static Task When<T>(this Task<T> task, TaskContinuationOptions continuationOptions,
            Action<Task<T>> continuationAction)
        {
            return task.ContinueWith(continuationAction, continuationOptions);
        }

        public static Task<TNewResult> When<T, TNewResult>(this Task<T> task,
            TaskContinuationOptions continuationOptions, Func<Task<T>, TNewResult> continuationAction)
        {
            return task.ContinueWith(continuationAction, continuationOptions);
        }

        public static bool IsFinished(this Task task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                case TaskStatus.Canceled:
                case TaskStatus.Faulted:
                    return true;
                default:
                    return false;
            }
        }

        public static Task Map(
            this Task task,
            Action<Task> success,
            Action<Task> noSuccess)
        {
            return task.ContinueWith(task1 =>
            {
                if (task1.Status == TaskStatus.RanToCompletion)
                    success(task1);
                else
                    noSuccess(task1);
            });
        }

        public static Task<TNewResult> Map<TNewResult>(
            this Task task,
            Func<Task, TNewResult> success,
            Func<Task, TNewResult> noSuccess)
        {
            return task.ContinueWith(task1 =>
            {
                if (task1.Status == TaskStatus.RanToCompletion)
                    return success(task1);
                else
                    return noSuccess(task1);
            });
        }

        public static Task Map<T>(
            this Task<T> task,
            Action<Task<T>> success,
            Action<Task<T>> noSuccess)
        {
            return task.ContinueWith(task1 =>
            {
                if (task1.Status == TaskStatus.RanToCompletion)
                    success(task1);
                else
                    noSuccess(task1);
            });
        }

        public static Task<TNewResult> Map<T, TNewResult>(
            this Task<T> task,
            Func<Task<T>, TNewResult> success,
            Func<Task<T>, TNewResult> noSucces)
        {
            return task.ContinueWith(task1 =>
            {
                if (task1.Status == TaskStatus.RanToCompletion)
                    return success(task1);
                else
                    return noSucces(task1);
            });
        }
    }
}
