using System;
using System.Threading;

namespace Plainion.GraphViz.Modules.Reflection.Services.Framework
{
    public abstract class AsyncInspectorBase<TResult> : InspectorBase
    {
        protected AsyncInspectorBase( string applicationBase )
            : base( applicationBase )
        {
        }

        /// <summary>
        /// Set if you want to have progress be reported
        /// </summary>
        public IProgress<int> ProgressCallback { get; set; }

        /// <summary>
        /// Set if you want to be able to cancel.
        /// </summary>
        public ICancellationToken CancellationToken { get; set; }

        protected void ReportProgress( int value )
        {
            if( ProgressCallback != null )
            {
                ProgressCallback.Report( value );
            }
        }

        protected bool IsCancellationRequested
        {
            get { return CancellationToken != null && CancellationToken.IsCancellationRequested; }
        }
    
        public abstract TResult Execute();
    }
}
