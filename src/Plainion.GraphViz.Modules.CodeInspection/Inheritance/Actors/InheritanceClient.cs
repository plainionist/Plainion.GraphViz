using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance.Analyzers;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance.Actors
{
    [Export(typeof(InheritanceClient))]
    class InheritanceClient
    {
        public IEnumerable<TypeDescriptor> GetAllTypes(string assemblyLocation)
        {
            using (var inspector = new InspectorHandle<AllTypesActor>(Path.GetDirectoryName(assemblyLocation)))
            {
                inspector.Value.AssemblyLocation = assemblyLocation;

                return inspector.Value.Execute();
            }
        }

        public Task<TypeRelationshipDocument> AnalyzeInheritanceAsync(string assemblyLocation, bool ignoreDotNetTypes, TypeDescriptor typeToAnalyse, Action<int> progressCallback, CancellationToken cancellationToken)
        {
            using (var inspector = new InspectorHandle<InheritanceActor>(Path.GetDirectoryName(assemblyLocation)))
            {
                inspector.Value.IgnoreDotNetTypes = ignoreDotNetTypes;
                inspector.Value.AssemblyLocation = assemblyLocation;
                inspector.Value.SelectedType = typeToAnalyse;

                var tcs = new TaskCompletionSource<TypeRelationshipDocument>();

                RunAsync(progressCallback, inspector.Value, tcs, cancellationToken);

                return tcs.Task;
            }
        }

        private static void RunAsync(Action<int> progressCallback, InheritanceActor inspector, TaskCompletionSource<TypeRelationshipDocument> tcs, CancellationToken cancellationToken)
        {
            var worker = new System.ComponentModel.BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            worker.DoWork += (s, e) =>
            {
                var adapter = new BackgroundWorkerAdapter((System.ComponentModel.BackgroundWorker)s);
                inspector.ProgressCallback = adapter;
                inspector.CancellationToken = adapter;

                e.Result = inspector.Execute();
            };
            worker.ProgressChanged += (s, e) => progressCallback(e.ProgressPercentage);
            worker.RunWorkerCompleted += (s, e) =>
            {
                tcs.SetResult((TypeRelationshipDocument)e.Result);
                // TODO: is this a good idea? aren't we still in the callstack of the worker?
                worker.Dispose();
            };

            cancellationToken.Register(() =>
            {
                if (worker.IsBusy && !worker.CancellationPending)
                {
                    worker.CancelAsync();
                    tcs.SetCanceled();
                }
            });

            worker.RunWorkerAsync(inspector);
        }
    }

    class BackgroundWorkerAdapter : MarshalByRefObject, IProgress<int>, ICancellationToken
    {
        private System.ComponentModel.BackgroundWorker myWorker;

        public BackgroundWorkerAdapter(System.ComponentModel.BackgroundWorker worker)
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

    class InspectorHandle<T> : IDisposable where T : class
    {
        private InspectionDomain myDomain;

        public InspectorHandle(string appBase)
        {
            myDomain = new InspectionDomain(appBase);
            Value = myDomain.CreateInspector<T>();
        }

        public T Value { get; private set; }

        public void Dispose()
        {
            var disposable = Value as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
            Value = null;

            if (myDomain != null)
            {
                myDomain.Dispose();
                myDomain = null;
            }
        }
    }

    class InspectionDomain : IDisposable
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
                BindingFlags.Default, null, null, null, null).Unwrap();
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

    class AssemblyResolveHandler : MarshalByRefObject
    {
        public void Attach()
        {
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
