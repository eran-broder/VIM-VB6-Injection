using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedLibraryForInjection.IPC
{
    //TODO: very poorly written
    class NamedPipeTransport
    {
        //private CancellationToken _cancellationToken;
        private bool _stopped = false; //TODO: really? the best you can do?

        public event Action<char[]> NewMessage;

        public Action Start(string pipeName, CancellationToken cancellationToken)
        {
            //_cancellationToken = cancellationToken;
            Task.Run(ListenerTask);

            return ()=> _stopped = true; //TODO: wrap things up
        }

        private void ListenerTask()
        {
            var pipeName = $"VimEmbedded";//_{Process.GetCurrentProcess().Id}";
            Console.WriteLine($"listening on: {pipeName} on thread [{Thread.CurrentThread.ManagedThreadId}]");
            NamedPipeServerStream stream =
                new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message);

            stream.WaitForConnection();
            Console.WriteLine("Client connected!");
            StreamReader reader = new StreamReader(stream);
            var array = new char[1024]; // TODO: how do you determine the buffer size? 
            var arraySpan = new Span<char>(array);

            while (!_stopped/*_cancellationToken.IsCancellationRequested*/ )
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
    }
}
