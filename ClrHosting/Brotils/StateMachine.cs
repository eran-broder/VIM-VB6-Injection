using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brotils
{
    public class StateMachine
    {

    }

    public record StateRec(string Name, Action EnterAction);

    public class State
    {
        protected StateMachine Context { get; }

        public State(StateMachine context)
        {
            Context = context;
        }
    }
}
