using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brotils
{
    public static class ReflectionUtils
    {

        public static Func<object[], object> GetStaticFunction<T>(string functionName)
        {
            var @type = typeof(T);
            var method = @type.GetMethod(functionName);
            Assersions.Assert(method != null, 
                $"Function [{functionName}] does not exist on type {@type.Name}");
            return parameters => method.Invoke(null, parameters);
        }

    }
}
