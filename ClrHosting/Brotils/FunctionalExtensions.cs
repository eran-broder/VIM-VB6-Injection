using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brotils.Functional;
using Optional;

namespace Brotils
{
    public static class FunctionalExtensions
    {
        public static Either<T, Exception> ValueOrException<T>(Func<T> func)
        {
            try
            {
                var result = func();
                return Either<T, Exception>.Left(result);
            }
            catch (Exception e)
            {
                return Either<T, Exception>.Right(e);
            }
        }
    }
}
