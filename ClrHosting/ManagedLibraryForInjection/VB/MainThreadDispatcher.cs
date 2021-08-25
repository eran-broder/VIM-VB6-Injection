using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brotils;
using Brotils.Functional;
using Optional.Collections;
using Win32Utils;

namespace ManagedLibraryForInjection.VB
{
    public class MainThreadDispatcher
    {
        private int _specificMessageId = 3;
        private readonly Dictionary<int, (Func<object> func, TaskCompletionSource<object> taskCompletion)> _messageMap = new();
        public MainThreadDispatcher(IntPtr windowOfThread, int messageCode)
        {
            this.Dispatch = action =>
            {
                var currentMessageId = _specificMessageId++;
                var taskCompletionSource = new TaskCompletionSource<object>();
                _messageMap.Add(currentMessageId, (action, taskCompletionSource));
                PInvoke.PostMessage(windowOfThread, messageCode, currentMessageId, IntPtr.Zero);
                return taskCompletionSource.Task;
            };
        }

        public readonly Func<Func<object>, Task<object>> Dispatch;

        public void Invoke(int messageId)
        {
            //TODO: should I return either?
            _messageMap.GetValueOrNone(messageId).Match(
                pair =>
                {
                    try
                    {
                        var returnValue = pair.func();
                        pair.taskCompletion.SetResult(returnValue);
                    }
                    catch(Exception e)
                    {
                        pair.taskCompletion.SetException(e);
                    }
                    
                },
                () => throw new Exception($"cannot find message with ID {messageId}")
                );
        }
    }
}
