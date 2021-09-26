using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Brotils;
using Brotils.Functional;
using Optional.Collections;
using Win32Utils;

namespace ManagedLibraryForInjection.VB
{
    public class MainThreadDispatcher
    {
        private int _messageIdCounter = 3;

        private readonly Dictionary<int, (Func<object> func, TaskCompletionSource<object> taskCompletion)> _messageMap = new();

        //TODO: very bad. no timeout
        public MainThreadDispatcher(IntPtr windowOfThread, int messageCode)
        {
            this.Dispatch = action =>
            {
                var currentMessageId = _messageIdCounter++;
                var taskCompletionSource = new TaskCompletionSource<object>();
                _messageMap.Add(currentMessageId, (action, taskCompletionSource));
                PInvoke.PostMessage(windowOfThread, messageCode, currentMessageId, IntPtr.Zero);
                return taskCompletionSource.Task;
            };
        }

        public readonly Func<Func<object>, Task<object>> Dispatch;

        public void Invoke(int messageId)
        {
            Console.WriteLine("@@@@@@Invoking@@@@@@@");
            //TODO: should I return either?
            _messageMap.GetValueOrNone(messageId).Match(
                pair =>
                {
                    try
                    {
                        Console.WriteLine("111111111111111");
                        var returnValue = pair.func();
                        Console.WriteLine("222222222222222222");
                        Task.Run(() =>
                        {
                            Console.WriteLine("333333333333333333");
                            pair.taskCompletion.SetResult(returnValue);
                        });

                        //TODO: perhaps this is where shit goes down. I am playing with fire. an unmanaged thread resolving a managed task?
                        //pair.taskCompletion.SetResult(returnValue);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"4: {e.Message}");
                        pair.taskCompletion.SetException(e);
                    }
                    finally
                    {
                        _messageMap.Remove(messageId);
                    }
                    
                },
                () => throw new Exception($"cannot find message with ID {messageId}")
                );
        }
    }
}
