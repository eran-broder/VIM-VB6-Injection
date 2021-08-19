using System;
using Microsoft.VisualBasic;

namespace Brotils
{
    public static class Sugar
    {
        
    }

    public static class Assersions
    {
        public static void Assert(bool value, string message)
        {

        }

        //TODO: wrap the expression with a safe
        public static void Assert(Func<bool> func, string message) => Assert(func(), message);
    }
}
