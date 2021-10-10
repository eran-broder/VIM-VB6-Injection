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

        public record SplitResult<T, U>(IEnumerable<T> First, IEnumerable<U> Second);

        public static SplitResult<T, U> Split<T, U>(this IEnumerable<Tuple<T, U>> joined)
        {
            List<T> first = new List<T>();
            List<U> second = new List<U>();
            foreach (var tuple in joined)
            {
                first.Add(tuple.Item1);
                second.Add(tuple.Item2);
            }

            return new SplitResult<T, U>(first, second);
        }
    }
}
