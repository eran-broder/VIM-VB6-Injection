using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ManagedLibraryForInjection.VB
{
    internal record InternalAffairsMessage(string FunctionName, object[] Parameters);

    class ReflectionBasedHandler : MessageHandlerBase<InternalAffairsMessage>
    {
        public Type ReflectedType { get; }

        public ReflectionBasedHandler(Type reflectedType)
        {
            ReflectedType = reflectedType;
        }
        protected override Task<object> HandleMessage(InternalAffairsMessage message)
        {
            return Optional.OptionExtensions.SomeNotNull(ReflectedType.GetMethod(message.FunctionName))
                .Match(info => Task.FromResult(info.Invoke(null, message.Parameters)),
                    () => throw new Exception($"Type [{ReflectedType}] has no method [{message.FunctionName}]"));
            
        }
    }

}