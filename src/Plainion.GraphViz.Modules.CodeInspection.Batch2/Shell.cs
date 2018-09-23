using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Plainion.GraphViz.Modules.CodeInspection.Batch
{
    public class Shell
    {
        private static object myLock = new object();

        public static void Warn(string msg)
        {
            Write(ConsoleColor.Yellow, "WARNING: " + msg);
        }

        public static void Error(string msg)
        {
            Write(ConsoleColor.Red, "ERROR: " + msg);
            Environment.Exit(1);
        }

        private static void Write(ConsoleColor color, string msg)
        {
            lock (myLock)
            {
                var old = Console.ForegroundColor;

                try
                {
                    Console.ForegroundColor = color;

                    Console.WriteLine(msg);
                }
                finally
                {
                    Console.ForegroundColor = old;
                }
            }
        }

        public static T Profile<T>(string caption, Func<T> f)
        {
            Console.WriteLine(caption);

            var watch = new Stopwatch();
            watch.Start();

            var ret = f();

            watch.Stop();
            Console.WriteLine($"{caption} elapsed {watch.Elapsed}");

            return ret;
        }

        public static IEnumerable<string> ResolveAssemblies(string binFolder, IEnumerable<string> patterns)
        {
            return patterns
                .SelectMany(pattern => 
                    { 
                        var files = Directory.GetFiles(binFolder, pattern) ;
                        if (files.Length == 0)
                        {
                            Shell.Warn($"No assemblies found for pattern: {pattern}");
                            return Enumerable.Empty<string>();
                        }
                        else
                        {
                            return files;
                        }
                    })
                .Select(f => Path.GetFullPath(f))
                .ToList();
    }
}
}
