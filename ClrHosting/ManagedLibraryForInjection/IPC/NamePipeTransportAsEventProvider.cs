using System.Threading;
using Brotils.Cancelable.CancelableEventProviderGeneric;

namespace ManagedLibraryForInjection.IPC
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
}