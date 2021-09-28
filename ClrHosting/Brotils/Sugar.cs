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
            if (!value)
                throw new Exception(message);
        }

        //TODO: wrap the expression with a safe
        public static void Assert(Func<bool> func, string message) => Assert(func(), message);
        public static void NotNull(IntPtr handle, string message) => Assert(handle != IntPtr.Zero, message);
        public static void NotNull(object obj, string message) => Assert(obj != null, message);
    }
}
