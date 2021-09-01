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
                
                Task.Run(async () =>
                {
                    //TODO: really? think it over!
                    var result = new byte[1024];

                    while (!stopped /*_cancellationToken.IsCancellationRequested*/)
                    {
                        //TODO: what if bytesread equals 0?
                        var bytesRead = await stream.ReadAsync(result);
                        var asString = System.Text.Encoding.Default.GetString(result);
                        invoke(asString);
                    }
                });
                return () => stopped = true;
            });
        }
    }
}