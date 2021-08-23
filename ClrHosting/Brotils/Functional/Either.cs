using System;

namespace Brotils.Functional
{
    public class Either<TL, TR>
    {
        private readonly TL left;
        private readonly TR right;
        private readonly bool isLeft;

        private Either(TL left)
        {
            this.left = left;
            this.isLeft = true;
        }

        private Either(TR right, bool isRightFlag /*value is ignored. only for overloading explicitly*/)
        {
            this.right = right;
            this.isLeft = false;
        }

        public T Match<T>(Func<TL, T> left, Func<TR, T> right)
            => this.isLeft ? left(this.left) : right(this.right);

        public TL MatchRight<T>(Func<TR, TL> rightMatch) =>
            this.Match(l => l, rightMatch);

        public static Either<TL, TR> Left(TL left) => new(left);
        public static Either<TL, TR> Right(TR right) => new(right, true);
    }
}
