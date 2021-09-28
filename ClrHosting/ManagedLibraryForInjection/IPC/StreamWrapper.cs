using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Brotils.Cancelable.CancelableEventProviderGeneric;

namespace ManagedLibraryForInjection.IPC
{
    public static class StreamWrapper
    {
        //When task is completed, it means the client has disconnected.
        //TODO: should I make it more explicit?
        public static async Task ReadLoop(Stream stream, Action<string, CancellationToken> readCallback, CancellationToken token)
        {
            //TODO: really? think it over!
            var result = new byte[1024];

            while (true)
            {
                token.ThrowIfCancellationRequested();

                var bytesRead = await stream.ReadAsync(result, token);
                if (bytesRead == 0)
                {
                    Console.WriteLine("well...stopping the loop");
                    return;
                }
                var asString = System.Text.Encoding.Default.GetString(result, 0, bytesRead);
                readCallback(asString, token);
            }
        }
    }
}