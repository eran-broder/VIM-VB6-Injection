using System;
using System.Threading.Tasks;
using Brotils;

namespace ManagedLibraryForInjection.VB
{
    record VbMessage(string FunctionName, object[] Parameters);

    class VbMessageHandler: MessageHandlerBase<VbMessage>
    {
        private readonly Func<Func<object>, Task<object>> _invoker;

        public VbMessageHandler(Func<Func<object>, Task<object>> invoker)
        {
            _invoker = invoker;
        }
        protected override Task<object> HandleMessage(VbMessage message)
        {
            var functionToRun = GetFunction(message);
            return _invoker(() => functionToRun());
        }

        //TODO: Do I handle each error? do I take care of bad function names? or just bubble the exception?
        private Func<object> GetFunction(VbMessage message)
        {
            var function = ReflectionUtils.GetStaticFunction<EcwEmbeddedAdapterProxy>(message.FunctionName);
            return () => function.Invoke(message.Parameters);
        }
    }
}
