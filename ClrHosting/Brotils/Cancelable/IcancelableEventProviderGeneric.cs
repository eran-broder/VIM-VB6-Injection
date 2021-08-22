using System;
using System.Collections.Generic;
using System.Linq;

namespace Brotils.Cancelable
{
    namespace CancelableEventProviderGeneric
    {

        public interface IAcceptingHandlers<T, TNextState> where TNextState : IAcceptingHandlers<T, TNextState>
        {
            public TNextState AddHandler(Action<T> handler);
        }

        public interface ICancelableEventProviderEmpty<T> : IAcceptingHandlers<T, ICancelableEventProviderReadyToStart<T>>
        {

        }


        public interface
            ICancelableEventProviderReadyToStart<T> : IAcceptingHandlers<T, ICancelableEventProviderReadyToStart<T>>
        {
            ICancelableEventProviderActive<T> Start();
        }

        public interface
            ICancelableEventProviderActive<T> : IAcceptingHandlers<T, ICancelableEventProviderActive<T>>, ICancelable
        {

        }

        public delegate Action InitializationHandler<T>(Action<T> eventInvoke);

        public class CancelableEventProvider<T>
        {
            public static ICancelableEventProviderEmpty<T> Create(InitializationHandler<T> initializer)
            {
                return new CancelableEventProviderEmpty<T>(initializer);
            }
        }

        public record CancelableEventProviderEmpty<T>
            (InitializationHandler<T> Initializer) : ICancelableEventProviderEmpty<T>
        {
            public ICancelableEventProviderReadyToStart<T> AddHandler(Action<T> handler)
            {
                return new CancelableEventProviderWithHandlers<T>(Initializer, new []{handler});
            }
        }

        public class CancelableEventProviderWithHandlersBase<T>
        {
            protected IEnumerable<Action<T>> _handlers;

            public CancelableEventProviderWithHandlersBase(IEnumerable<Action<T>> handlers)
            {
                _handlers = handlers;
            }
        }

        public class CancelableEventProviderWithHandlers<T>: CancelableEventProviderWithHandlersBase<T>, ICancelableEventProviderReadyToStart<T>
        {
            private readonly InitializationHandler<T> _initializer;

            public CancelableEventProviderWithHandlers(InitializationHandler<T> initializer, Action<T>[] handlers) : base(handlers)
            {
                _initializer = initializer;
            }

            public ICancelableEventProviderReadyToStart<T> AddHandler(Action<T> handler)
            {
                _handlers = _handlers.Append(handler);
                return this;
            }

            public ICancelableEventProviderActive<T> Start()
            {
                void InvokeAllHandlers(T arg) => _handlers.ToList().ForEach(h => h.Invoke(arg));
                var cancelAction = _initializer.Invoke(InvokeAllHandlers);
                return new CancelableEventProviderActive<T>(cancelAction, _handlers);
            }
        }

        public class CancelableEventProviderActive<T>: CancelableEventProviderWithHandlersBase<T>, ICancelableEventProviderActive<T>
        {
            private readonly Action _cancelAction;

            public CancelableEventProviderActive(Action cancelAction, IEnumerable<Action<T>> handlers) : base(handlers)
            {
                _cancelAction = cancelAction;
            }

            public ICancelableEventProviderActive<T> AddHandler(Action<T> handler)
            {
                _handlers = _handlers.Append(handler);
                return this;
            }

            public void Cancel()
            {
                _cancelAction();
            }
        }


    }

}

