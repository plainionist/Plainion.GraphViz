using System;
using System.Diagnostics;

namespace Plainion.GraphViz.Modules.Analysis
{
    class Profile : IDisposable
    {
        private string myMessage;
        private DateTime myStart;

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
