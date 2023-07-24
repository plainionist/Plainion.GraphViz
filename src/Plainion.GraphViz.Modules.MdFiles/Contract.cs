using System;
using System.Collections.Generic;
using System.Linq;
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

        public static void RequiresNoDuplicates<T>(IEnumerable<T> argument, [CallerArgumentExpression(nameof(argument))] string argumentName = "")
        {
            if (argument.Distinct().Count() != argument.Count())
            {
                throw new ArgumentException("Must not contain duplicate values", argumentName);
            }
        }
    }
}