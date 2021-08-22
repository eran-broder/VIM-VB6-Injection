using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedLibraryForInjection
{

    record Message(int Id);
    record Response(int Id);

    interface ITransport
    {
        event Action<Message> GotMessage;
        void SendResponse(Response message);
    }
    class MessageHandler
    {

    }
}
