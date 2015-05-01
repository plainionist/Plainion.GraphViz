using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Plainion.GraphViz.Modules.Reflection.Services.Framework;
using Plainion;

namespace Plainion.GraphViz.Modules.Reflection.Services
{
    [Export( typeof( AssemblyInspectionService ) )]
    public class AssemblyInspectionService
    {
        private Dictionary<string, DomainHandle> myActiveDomains;

        private class DomainHandle
        {
            private AssemblyInspectionService myService;

            public DomainHandle( AssemblyInspectionService service, string applicationDomain )
            {
                myService = service;

                Domain = new InspectionDomain( applicationDomain );
                RefCount = 0;
            }

            public InspectionDomain Domain { get; private set; }
            public int RefCount { get; private set; }

            public void Acquire()
            {
                RefCount++;
            }

            public void Release()
            {
                RefCount--;

                if( RefCount == 0 )
                {
                    Contract.Invariant( Domain != null, "Domain already destroyed" );

                    myService.myActiveDomains.Remove( Domain.ApplicationBase );
                    Domain.Dispose();
                    Domain = null;
                }
            }
        }

        public AssemblyInspectionService()
        {
            myActiveDomains = new Dictionary<string, DomainHandle>( StringComparer.OrdinalIgnoreCase );
        }

        public IInspectorHandle<T> CreateInspector<T>( string inspectionRootDirectory ) where T : InspectorBase
        {
            if( !myActiveDomains.ContainsKey( inspectionRootDirectory ) )
            {
                myActiveDomains[ inspectionRootDirectory ] = new DomainHandle( this, inspectionRootDirectory );
            }

            var domainHandle = myActiveDomains[ inspectionRootDirectory ];
            var inspector = domainHandle.Domain.CreateInspector<T>();

            return new InspectorHandle<T>( domainHandle, inspector );
        }

        private class InspectorHandle<T> : IInspectorHandle<T> where T : InspectorBase
        {
            private DomainHandle myDomain;

            public InspectorHandle( DomainHandle assemblyInspectionService, T inspector )
            {
                myDomain = assemblyInspectionService;
                Value = inspector;

                myDomain.Acquire();
            }

            public T Value
            {
                get;
                private set;
            }

            public void Dispose()
            {
                var disposableValue = Value as IDisposable;
                if( disposableValue != null )
                {
                    disposableValue.Dispose();
                }

                if( myDomain != null )
                {
                    myDomain.Release();
                }

                Value = null;
                myDomain = null;
            }
        }

        /// <summary/>
        /// <returns>Delegate to cancel the background processing</returns>
        internal Action RunAsync<TResult>( AsyncInspectorBase<TResult> inspector, Action<int> progressCallback, Action<TResult> completedCallback )
        {
            var worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            worker.DoWork += ( s, e ) =>
                {
                    var adapter = new BackgroundWorkerAdapter( ( BackgroundWorker )s );
                    inspector.ProgressCallback = adapter;
                    inspector.CancellationToken = adapter;

                    e.Result = inspector.Execute();
                };
            worker.ProgressChanged += ( s, e ) => progressCallback( e.ProgressPercentage );
            worker.RunWorkerCompleted += ( s, e ) =>
                {
                    completedCallback( ( TResult )e.Result );
                    // TODO: is this a good idea? aren't we still in the callstack of the worker?
                    worker.Dispose();
                };

            worker.RunWorkerAsync( inspector );

            return () =>
            {
                if( worker.IsBusy && !worker.CancellationPending )
                {
                    worker.CancelAsync();
                }
            };
        }
    }
}
