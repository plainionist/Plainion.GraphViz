using System;
using System.Runtime.CompilerServices;

namespace Plainion.GraphViz.Modules.MdFiles
{
    internal static class Contract
    {
        public static void RequiresNotNull<T>(T argument, [CallerArgumentExpression(nameof(argument))] string argumentName = "") where T : class
        {
            if (argument is null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        public static void RequiresNotNullNotEmpty(string argument, [CallerArgumentExpression(nameof(argument))] string argumentName = "")
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentNullException(argumentName);
            }
        }
    }
}