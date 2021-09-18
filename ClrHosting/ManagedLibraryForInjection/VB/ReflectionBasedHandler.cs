﻿using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ManagedLibraryForInjection.VB
{

    internal record InternalAffairsMessage(string MethodName);

    //TODO: support parameters
    class ReflectionBasedHandler : MessageHandlerBase<InternalAffairsMessage>
    {
        public Type ReflectedType { get; }

        public ReflectionBasedHandler(Type reflectedType)
        {
            ReflectedType = reflectedType;
        }
        protected override Task<object> HandleMessage(InternalAffairsMessage message)
        {
            return Optional.OptionExtensions.SomeNotNull(ReflectedType.GetMethod(message.MethodName))
                .Match(info => Task.FromResult(info.Invoke(null, new object[]{})),
                    () => throw new Exception($"Type [{ReflectedType}] has no method [{message.MethodName}]"));
            
        }
    }

}