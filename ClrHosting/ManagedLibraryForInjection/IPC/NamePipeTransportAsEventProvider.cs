using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Brotils.Cancelable.CancelableEventProviderGeneric;

namespace ManagedLibraryForInjection.IPC
{
    public static class StreamWrapper
    {
        public static ICancelableEventProviderEmpty<string> Create(Stream stream)
        {
            return CancelableEventProvider<string>.Create(invoke =>
            {
                var stopped = false;
                

                Task.Run(() =>
                {
                    StreamReader reader = new StreamReader(stream);
                    var array = new char[1024]; // TODO: how do you determine the buffer size? 
                    var arraySpan = new Span<char>(array);

                    while (!stopped /*_cancellationToken.IsCancellationRequested*/)
                    {
                        var readChars = reader.Read(arraySpan);
                        if (readChars > 0)
                        {
                            arraySpan.Slice(1, 20);
                            var message = arraySpan[..readChars];
                            invoke(new string(message.ToArray())); 
                        }
                    }
                });
                return () => stopped = true;
            });
        }
    }
}