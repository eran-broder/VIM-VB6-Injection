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

        public static void NotNull(IntPtr handle, string message) => Assert(handle != IntPtr.Zero, message);
        public static void NotNull(object obj, string message) => Assert(obj != null, message);

        public static T NoException<T>(Func<T> func, string errorMessage)
        {
            try
            {
                return func();
            }
            catch(Exception e)
            {
                throw new Exception(errorMessage, e);
            }
        }
    }

}
