using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Brotils;
using Brotils.Cancelable.CancelableEventProviderGeneric;
using Brotils.CancelableEventProvider;

namespace ManagedLibraryForInjection
{

    public class NamePipeTransportAsEventProvider
    {
        public static ICancelableEventProviderEmpty<string> Strat(string pipeName)
        {

            return CancelableEventProvider<string>.Create(invoke =>
            {
                var transport = new NamedPipeTransport();
                transport.NewMessage += chars => invoke(new string(chars));
                var cancel = transport.Start(pipeName, CancellationToken.None);
                return cancel;
            });
        }
    }
    //TODO: very poorly written
    class NamedPipeTransport
    {
        private CancellationToken _cancellationToken;

        public event Action<char[]> NewMessage;

        public Action Start(string pipeName, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            Task.Run(ListenerTask, cancellationToken);

            return Stop;
        }

        private void ListenerTask()
        {
            var pipeName = $"VimEmbedded";//_{Process.GetCurrentProcess().Id}";
            Console.WriteLine("listening on:");
            Console.WriteLine(pipeName);
            NamedPipeServerStream stream =
                new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message);

            stream.WaitForConnection();
            StreamReader reader = new StreamReader(stream);
            var array = new char[1024]; // TODO: how do you determine the buffer size? 
            var arraySpan = new Span<char>(array);

            while (!_cancellationToken.IsCancellationRequested)
            {
                var readChars = reader.Read(arraySpan);
                if (readChars > 0)
                {
                    arraySpan.Slice(1, 20);
                    var message = arraySpan[..readChars];
                    NewMessage?.Invoke(message.ToArray()); //TODO: should I really copy it?
                }
                else
                {
                    //TODO: switch logic to state machine
                    if (!stream.IsConnected)
                    {
                        Console.WriteLine("Client disconnected!");
                        break;
                    }
                }
                
            }
        }

        private void Stop()
        {

        }


    }
}
