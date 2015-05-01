using System;
using System.ComponentModel;
using Plainion.GraphViz.Modules.Reflection.Services.Framework;

namespace Plainion.GraphViz.Modules.Reflection.Services
{
    internal class BackgroundWorkerAdapter : MarshalByRefObject, IProgress<int>, ICancellationToken
    {
        private BackgroundWorker myWorker;

        public BackgroundWorkerAdapter( BackgroundWorker worker )
        {
            myWorker = worker;
        }

        public void Report( int progress )
        {
            myWorker.ReportProgress( progress );
        }

        public bool IsCancellationRequested
        {
            get { return myWorker.CancellationPending; }
        }
    }
}
