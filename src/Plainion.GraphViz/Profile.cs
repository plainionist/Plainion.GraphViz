using System;
using System.Diagnostics;

namespace Plainion.GraphViz
{
    public class Profile : IDisposable
    {
        private readonly string myMessage;
        private readonly DateTime myStart;

        public Profile(string msg)
        {
            myMessage = msg;
            myStart = DateTime.UtcNow;
        }

        public void Dispose()
        {
            Debug.WriteLine(myMessage + ": " + (DateTime.UtcNow - myStart));
        }
    }
}
