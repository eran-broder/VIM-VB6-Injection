using System;
using System.Collections.Generic;
using System.Linq;
using Brotils.Cancelable;

namespace Brotils
{
    namespace CancelableEventProvider
    {
        public interface IAcceptingHandlers<out TNextState> where TNextState : IAcceptingHandlers<TNextState>
        {
            public TNextState AddHandler(Action handler);
        }

        public interface ICancelableEventProviderEmpty : IAcceptingHandlers<ICancelableEventProviderReadyToStart>
        {

        }

        public interface ICancelableEventProviderReadyToStart : IAcceptingHandlers<ICancelableEventProviderReadyToStart>
        {
            ICancelableEventProviderActive Start();
        }

        public interface ICancelableEventProviderActive : IAcceptingHandlers<ICancelableEventProviderActive>,
            ICancelable
        {

        }

        public delegate Action InitializationHandler(Action eventInvoke);

        public class CancelableEventProvider
        {
            public static ICancelableEventProviderEmpty Create(InitializationHandler initializer)
            {
                return new CancelableEventProviderEmpty(initializer);
            }
        }

        public record CancelableEventProviderEmpty(InitializationHandler Initializer) : ICancelableEventProviderEmpty
        {
            public ICancelableEventProviderReadyToStart AddHandler(Action handler)
            {
                return new CancelableEventProviderWithHandlers(Initializer, new[] {handler});
            }
        }


        public class CancelableEventProviderBase<TNextState>
        {
            protected InitializationHandler Initializer;
            protected List<Action> Handlers;

            public CancelableEventProviderBase(InitializationHandler initializer, IEnumerable<Action> handlers)
            {
                Initializer = initializer;
                Handlers = handlers.ToList();
            }

            public TNextState AddHandler(Action handler, TNextState returnValue)
            {
                Handlers.Add(handler);
                return returnValue;
            }
        }

        public class CancelableEventProviderWithHandlers :
            CancelableEventProviderBase<ICancelableEventProviderReadyToStart>, ICancelableEventProviderReadyToStart
        {
            public CancelableEventProviderWithHandlers(InitializationHandler initializer, IEnumerable<Action> handlers)
                : base(initializer, handlers)
            {
            }



            public ICancelableEventProviderActive Start()
            {
                void InvokeAllHandlers() => Handlers.ForEach(h => h.Invoke());
                var cancelAction = Initializer.Invoke(InvokeAllHandlers);
                return new CancelableEventProviderActive(cancelAction, Handlers);
            }

            public ICancelableEventProviderReadyToStart AddHandler(Action handler)
            {
                return this.AddHandler(handler, this);
            }
        }

        public record CancelableEventProviderActive
            (Action CancelAction, List<Action> Handlers) : ICancelableEventProviderActive
        {
            public ICancelableEventProviderActive AddHandler(Action handler)
            {
                return this with {Handlers = Handlers.Concat(new[] {handler}).ToList()};
            }

            public void Cancel()
            {
                throw new NotImplementedException();
            }
        }
    }

}
