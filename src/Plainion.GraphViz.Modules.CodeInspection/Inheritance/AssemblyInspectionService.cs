using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance.Services.Framework;
using Plainion;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Security.Policy;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance.Services
{
    [Export(typeof(AssemblyInspectionService))]
    public class AssemblyInspectionService
    {
        private Dictionary<string, DomainHandle> myActiveDomains;

        private class DomainHandle
        {
            private AssemblyInspectionService myService;

            public DomainHandle(AssemblyInspectionService service, string appBase)
            {
                myService = service;

                Domain = new InspectionDomain(appBase);
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

                if (RefCount == 0)
                {
                    Contract.Invariant(Domain != null, "Domain already destroyed");

                    myService.myActiveDomains.Remove(Domain.ApplicationBase);
                    Domain.Dispose();
                    Domain = null;
                }
            }
        }

        public AssemblyInspectionService()
        {
            myActiveDomains = new Dictionary<string, DomainHandle>(StringComparer.OrdinalIgnoreCase);
        }

        public IInspectorHandle<T> CreateInspector<T>(string appBase) where T : class
        {
            if (!myActiveDomains.ContainsKey(appBase))
            {
                myActiveDomains[appBase] = new DomainHandle(this, appBase);
            }

            var domainHandle = myActiveDomains[appBase];
            var inspector = domainHandle.Domain.CreateInspector<T>();

            return new InspectorHandle<T>(domainHandle, inspector);
        }

        private class InspectorHandle<T> : IInspectorHandle<T> where T : class
        {
            private DomainHandle myDomain;

            public InspectorHandle(DomainHandle assemblyInspectionService, T inspector)
            {
                myDomain = assemblyInspectionService;
                Value = inspector;

                myDomain.Acquire();
            }

            public T Value { get; private set; }

            public void Dispose()
            {
                var disposableValue = Value as IDisposable;
                if (disposableValue != null)
                {
                    disposableValue.Dispose();
                }

                if (myDomain != null)
                {
                    myDomain.Release();
                }

                Value = null;
                myDomain = null;
            }
        }

        /// <summary/>
        /// <returns>Delegate to cancel the background processing</returns>
        internal Action RunAsync(InheritanceGraphInspector inspector, Action<int> progressCallback, Action<TypeRelationshipDocument> completedCallback)
        {
            var worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            worker.DoWork += (s, e) =>
                {
                    var adapter = new BackgroundWorkerAdapter((BackgroundWorker)s);
                    inspector.ProgressCallback = adapter;
                    inspector.CancellationToken = adapter;

                    e.Result = inspector.Execute();
                };
            worker.ProgressChanged += (s, e) => progressCallback(e.ProgressPercentage);
            worker.RunWorkerCompleted += (s, e) =>
                {
                    completedCallback((TypeRelationshipDocument)e.Result);
                    // TODO: is this a good idea? aren't we still in the callstack of the worker?
                    worker.Dispose();
                };

            worker.RunWorkerAsync(inspector);

            return () =>
            {
                if (worker.IsBusy && !worker.CancellationPending)
                {
                    worker.CancelAsync();
                }
            };
        }
    }

    internal class BackgroundWorkerAdapter : MarshalByRefObject, IProgress<int>, ICancellationToken
    {
        private BackgroundWorker myWorker;

        public BackgroundWorkerAdapter(BackgroundWorker worker)
        {
            myWorker = worker;
        }

        public void Report(int progress)
        {
            myWorker.ReportProgress(progress);
        }

        public bool IsCancellationRequested
        {
            get { return myWorker.CancellationPending; }
        }
    }

    internal class InspectionDomain : IDisposable
    {
        private AppDomain myDomain;
        private AssemblyResolveHandler myAssemblyResolveHandler;

        public InspectionDomain(string appBase)
        {
            Contract.RequiresNotNullNotEmpty(appBase, "appBase");

            ApplicationBase = appBase;

            var evidence = new Evidence(AppDomain.CurrentDomain.Evidence);
            var setup = AppDomain.CurrentDomain.SetupInformation;
            setup.ApplicationBase = ApplicationBase;
            myDomain = AppDomain.CreateDomain("GraphViz.Modules.Reflection.Sandbox-" + appBase.GetHashCode(), evidence, setup);

            myAssemblyResolveHandler = CreateInstance<AssemblyResolveHandler>();
            myAssemblyResolveHandler.Attach();
        }

        private T CreateInstance<T>()
        {
            return (T)myDomain.CreateInstanceFrom(typeof(T).Assembly.Location, typeof(T).FullName).Unwrap();
        }

        public string ApplicationBase { get; private set; }

        public T CreateInspector<T>()
        {
            return (T)myDomain.CreateInstanceFrom(typeof(T).Assembly.Location, typeof(T).FullName, false,
                BindingFlags.Default, null, new[] { ApplicationBase }, null, null).Unwrap();
        }

        public void Dispose()
        {
            if (myDomain != null)
            {
                Debug.WriteLine("Unloading AppDomain");

                myAssemblyResolveHandler.Detach();
                AppDomain.Unload(myDomain);
                myDomain = null;

                GC.Collect();
            }
        }
    }

    internal class AssemblyResolveHandler : MarshalByRefObject
    {
        //private volatile object myAssemblyRefereces;

        public void Attach()
        {
            //myAssemblyRefereces = GetType().Assembly.GetTypes();

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += OnReflectionOnlyAssemblyResolve;
        }

        // http://blogs.microsoft.co.il/sasha/2008/07/19/appdomains-and-remoting-life-time-service/
        public override object InitializeLifetimeService()
        {
            return null;
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(asm => string.Equals(asm.FullName, args.Name, StringComparison.OrdinalIgnoreCase));
        }

        private Assembly OnReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var loadedAssembly = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies()
                    .FirstOrDefault(asm => string.Equals(asm.FullName, args.Name, StringComparison.OrdinalIgnoreCase));

            if (loadedAssembly != null)
            {
                return loadedAssembly;
            }

            var assemblyName = new AssemblyName(args.Name);
            var dependentAssemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName.Name + ".dll");

            if (!File.Exists(dependentAssemblyPath))
            {
                dependentAssemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName.Name + ".exe");

                if (!File.Exists(dependentAssemblyPath))
                {
                    try
                    {
                        // e.g. .NET assemblies, assemblies from GAC
                        return Assembly.ReflectionOnlyLoad(args.Name);
                    }
                    catch
                    {
                        // ignore exception here - e.g. System.Windows.Interactivity - app will work without
                        Debug.WriteLine("Failed to load: " + assemblyName);
                        return null;
                    }
                }
            }

            var assembly = Assembly.ReflectionOnlyLoadFrom(dependentAssemblyPath);
            return assembly;
        }

        public void Detach()
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= OnReflectionOnlyAssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
        }
    }
}
