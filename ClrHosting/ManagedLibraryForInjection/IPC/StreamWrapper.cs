using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Brotils;
using Brotils.Cancelable.CancelableEventProviderGeneric;

namespace ManagedLibraryForInjection.IPC
{
    public static class StreamWrapper
    {

        private static uint _bufferSize = 1024;
        //When task is completed, it means the client has disconnected.
        public static async Task ReadLoop(Stream stream, Action<string, CancellationToken> readCallback, CancellationToken token)
        {
            var result = new byte[_bufferSize];

            while (true)
            {
                token.ThrowIfCancellationRequested();
                Console.WriteLine("About to read");
                var bytesRead = await stream.ReadAsync(result, token);
                Console.WriteLine($"read [{bytesRead}] bytes");
                Assersions.Assert(bytesRead <= _bufferSize, $"Stream message exceeds predefined buffer size of {_bufferSize}");
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