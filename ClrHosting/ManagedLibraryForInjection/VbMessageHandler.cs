using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedLibraryForInjection
{
    record VbMessage(string FunctionName, object[] Parameters);

    class VbMessageHandler: MessageHandlerBase<VbMessage>
    {
        protected override Task<object> HandleMessage(VbMessage message)
        {
            EcwEmbeddedAdapterProxy.SetReferral();
            return Task.FromResult((object)true);
        }
    }
}
