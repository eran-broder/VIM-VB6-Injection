using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Brotils.Cancelable.CancelableEventProviderGeneric;

namespace ManagedLibraryForInjection.IPC
{
    public static class StreamWrapper
    {
        public static async Task ReadLoop(Stream stream, Action<string, CancellationToken> readCallback, CancellationToken token)
        {
            //TODO: really? think it over!
            var result = new byte[1024];

            while (!token.IsCancellationRequested)
            {
                //TODO: what if bytesread equals 0?
                var bytesRead = await stream.ReadAsync(result, token);
                var asString = System.Text.Encoding.Default.GetString(result);
                readCallback(asString, token);
            }
        }
    }
}