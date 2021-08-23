using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Brotils
{
    public static class TaskExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<TResult> AsTask<T, TResult>(this Task<T> task)
            where T : TResult
            where TResult : class
        {
            return task.Success(t => t.Result as TResult);
        }

        public static Task When<T>(this Task<T> task, TaskContinuationOptions continuationOptions, Action<Task<T>> continuationAction)
        {
            return task.ContinueWith(continuationAction, continuationOptions);
        }

        public static Task<TNewResult> When<T, TNewResult>(this Task<T> task, TaskContinuationOptions continuationOptions, Func<Task<T>, TNewResult> continuationAction)
        {
            return task.ContinueWith(continuationAction, continuationOptions);
        }

        public static Task Success(this Task task, Action<Task> action) =>
            Helper.SpecificContinue(task, action, TaskContinuationOptions.OnlyOnRanToCompletion);

        public static Task Error(this Task task, Action<Task> action) =>
            Helper.SpecificContinue(task, action, TaskContinuationOptions.OnlyOnFaulted);

        public static Task NoSuccess(this Task task, Action<Task> action) =>
            Helper.SpecificContinue(task, action, TaskContinuationOptions.NotOnRanToCompletion);

        public static Task Canceled(this Task task, Action<Task> action) =>
            Helper.SpecificContinue(task, action, TaskContinuationOptions.OnlyOnCanceled);

        public static Task Always(this Task task, Action<Task> action) =>
            Helper.SpecificContinue(task, action, TaskContinuationOptions.None);

        public static Task<T> Success<T>(this Task<T> task, Action<Task<T>> action) =>
            Helper.SpecificContinue(task, action, TaskContinuationOptions.OnlyOnRanToCompletion);

        public static Task<T> Error<T>(this Task<T> task, Action<Task<T>> action) =>
            Helper.SpecificContinue(task, action, TaskContinuationOptions.OnlyOnFaulted);

        public static Task<T> NoSuccess<T>(this Task<T> task, Action<Task<T>> action) =>
            Helper.SpecificContinue(task, action, TaskContinuationOptions.NotOnRanToCompletion);

        public static Task<T> Canceled<T>(this Task<T> task, Action<Task<T>> action) =>
            Helper.SpecificContinue(task, action, TaskContinuationOptions.OnlyOnCanceled);

        public static Task<T> Always<T>(this Task<T> task, Action<Task<T>> action) =>
            Helper.SpecificContinue(task, action, TaskContinuationOptions.None);

        public static Task<TNewResult> Success<T, TNewResult>(this Task<T> task, Func<Task<T>, TNewResult> action) =>
            Helper.SpecificContinue(task, action, TaskContinuationOptions.OnlyOnRanToCompletion);

        public static Task<TNewResult> Error<T, TNewResult>(this Task<T> task, Func<Task<T>, TNewResult> action) =>
            Helper.SpecificContinue(task, action, TaskContinuationOptions.OnlyOnFaulted);

        public static Task<TNewResult> NoSuccess<T, TNewResult>(this Task<T> task, Func<Task<T>, TNewResult> action) =>
            Helper.SpecificContinue(task, action, TaskContinuationOptions.NotOnRanToCompletion);

        public static Task<TNewResult> Canceled<T, TNewResult>(this Task<T> task, Func<Task<T>, TNewResult> action) =>
            Helper.SpecificContinue(task, action, TaskContinuationOptions.OnlyOnCanceled);

        public static Task<TNewResult> Always<T, TNewResult>(this Task<T> task, Func<Task<T>, TNewResult> action) =>
            Helper.SpecificContinue(task, action, TaskContinuationOptions.None);

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


    }

    class Helper
    {
        public static Task<T> SpecificContinue<T>(Task<T> task, Action<Task<T>> action, TaskContinuationOptions option)
        {
            task.ContinueWith(action, option);
            return task;
        }

        public static Task SpecificContinue(Task task, Action<Task> action, TaskContinuationOptions option)
        {
            task.ContinueWith(action, option);
            return task;
        }

        public static Task<TNewResult> SpecificContinue<T, TNewResult>(Task<T> task, Func<Task<T>, TNewResult> action, TaskContinuationOptions option) => task.ContinueWith(action, option);
    }
}
