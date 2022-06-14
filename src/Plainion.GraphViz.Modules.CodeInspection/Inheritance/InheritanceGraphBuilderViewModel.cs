using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Plainion.Collections;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Modules.CodeInspection.Core;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance.Actors;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance.Analyzers;
using Plainion.GraphViz.Presentation;
using Plainion.Prism.Interactivity.InteractionRequest;
using Plainion.Prism.Mvvm;
using Prism.Commands;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance
{
    class InheritanceGraphBuilderViewModel : ViewModelBase
    {
        private string myAssemblyToAnalyseLocation;
        private TypeDescriptor myTypeToAnalyse;
        private bool myIsReady;
        private bool myIgnoreDotNetTypes;
        private CancellationTokenSource myCTS;
        private bool myAddToGraph;
        private IPresentationCreationService myPresentationCreationService;
        private IStatusMessageService myStatusMessageService;
        private InheritanceClient myInheritanceClient;

        public InheritanceGraphBuilderViewModel(IPresentationCreationService presentationCreationService, IStatusMessageService statusMessageService, InheritanceClient inheritanceClient, IDomainModel model)
            : base(model)
        {
            myPresentationCreationService = presentationCreationService;
            myStatusMessageService = statusMessageService;
            myInheritanceClient = inheritanceClient;

            Types = new ObservableCollection<TypeDescriptor>();
            TypeFilter = OnFilterItem;

            CreateGraphCommand = new DelegateCommand(CreateGraph, () => TypeToAnalyse != null && IsReady);
            AddToGraphCommand = new DelegateCommand(AddToGraph, () => TypeToAnalyse != null && IsReady);
            CancelCommand = new DelegateCommand(OnCancel, () => !IsReady);
            BrowseAssemblyCommand = new DelegateCommand(OnBrowseClicked, () => IsReady);
            ClosedCommand = new DelegateCommand(OnClosed);

            OpenFileRequest = new InteractionRequest<OpenFileDialogNotification>();

            IsReady = true;
            IgnoreDotNetTypes = true;
        }

        private void AddToGraph()
        {
            myAddToGraph = true;
            CreateGraph();
        }

        private void OnCancel()
        {
            if (myCTS != null)
            {
                myCTS.Cancel();
            }

            IsReady = true;
        }

        public bool IsReady
        {
            get { return myIsReady; }
            set
            {
                if (SetProperty(ref myIsReady, value))
                {
                    CreateGraphCommand.RaiseCanExecuteChanged();
                    AddToGraphCommand.RaiseCanExecuteChanged();
                    CancelCommand.RaiseCanExecuteChanged();
                    BrowseAssemblyCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public DelegateCommand CreateGraphCommand { get; private set; }

        public DelegateCommand AddToGraphCommand { get; private set; }

        public DelegateCommand CancelCommand { get; private set; }

        public DelegateCommand BrowseAssemblyCommand { get; private set; }

        public DelegateCommand ClosedCommand { get; private set; }

        public InteractionRequest<OpenFileDialogNotification> OpenFileRequest { get; private set; }

        private void OnBrowseClicked()
        {
            var notification = new OpenFileDialogNotification();
            notification.RestoreDirectory = true;
            notification.Filter = "Assemblies (*.dll,*.exe)|*.dll;*.exe";
            notification.FilterIndex = 0;

            OpenFileRequest.Raise(notification,
                n =>
                {
                    if (n.Confirmed)
                    {
                        AssemblyToAnalyseLocation = n.FileName;
                    }
                });
        }

        public AutoCompleteFilterPredicate<object> TypeFilter { get; private set; }

        private bool OnFilterItem(string search, object item)
        {
            var type = (TypeDescriptor)item;

            return type.FullName.ToLower().Contains(search.ToLower());
        }

        private async void CreateGraph()
        {
            IsReady = false;

            try
            {
                myCTS = new CancellationTokenSource();

                var doc = await myInheritanceClient.AnalyzeInheritanceAsync(AssemblyToAnalyseLocation, IgnoreDotNetTypes, TypeToAnalyse, myCTS.Token);

                myCTS.Dispose();
                myCTS = null;

                if (doc != null)
                {
                    OnInheritanceGraphCompleted(doc);
                }
            }
            finally
            {
                IsReady = true;
            }
        }

        private void OnClosed()
        {
            AssemblyToAnalyseLocation = null;

            if (myCTS != null)
            {
                myCTS.Cancel();
            }

            IsReady = true;
        }

        public string AssemblyToAnalyseLocation
        {
            get { return myAssemblyToAnalyseLocation; }
            set
            {
                if (SetProperty(ref myAssemblyToAnalyseLocation, value))
                {
                    TypeToAnalyse = null;
                    Types.Clear();

                    CollectTypes();
                }
            }
        }

        private async void CollectTypes()
        {
            if (!string.IsNullOrWhiteSpace(myAssemblyToAnalyseLocation) && File.Exists(myAssemblyToAnalyseLocation))
            {
                var types = await myInheritanceClient.GetAllTypesAsync(myAssemblyToAnalyseLocation, new CancellationTokenSource().Token);

                Types.AddRange(types);
            }
        }

        public ObservableCollection<TypeDescriptor> Types { get; private set; }

        public TypeDescriptor TypeToAnalyse
        {
            get { return myTypeToAnalyse; }
            set
            {
                // if s.th. is typed which is not available in the list of types we will get null here
                SetProperty(ref myTypeToAnalyse, value);
                CreateGraphCommand.RaiseCanExecuteChanged();
                AddToGraphCommand.RaiseCanExecuteChanged();

                if (myTypeToAnalyse == null)
                {
                    SetError(ValidationFailure.Error);
                }
                else
                {
                    ClearErrors();
                }
            }
        }

        public bool IgnoreDotNetTypes
        {
            get { return myIgnoreDotNetTypes; }
            set { SetProperty(ref myIgnoreDotNetTypes, value); }
        }

        private void OnInheritanceGraphCompleted(TypeRelationshipDocument document)
        {
            if (!document.Edges.Any())
            {
                MessageBox.Show("No nodes found");
                return;
            }

            var presentation = myPresentationCreationService.CreatePresentation(Path.GetDirectoryName(AssemblyToAnalyseLocation));

            var captionModule = presentation.GetPropertySetFor<Caption>();
            var tooltipModule = presentation.GetPropertySetFor<ToolTipContent>();
            var edgeStyleModule = presentation.GetPropertySetFor<EdgeStyle>();

            var builder = new RelaxedGraphBuilder();
            foreach (var edge in document.Edges)
            {
                var e = builder.TryAddEdge(edge.Item1, edge.Item2);
                edgeStyleModule.Add(new EdgeStyle(e.Id)
                {
                    Color = edge.Item3 == ReferenceType.DerivesFrom ? Brushes.Black : Brushes.Blue
                });
            }

            presentation.Graph = builder.Graph;

            foreach (var desc in document.Descriptors)
            {
                captionModule.Add(new Caption(desc.Id, desc.Name));
                tooltipModule.Add(new ToolTipContent(desc.Id, desc.FullName));
            }

            if (myAddToGraph && Model.Presentation != null && Model.Presentation.Graph != null)
            {
                presentation = Model.Presentation.UnionWith(presentation,
                    () => myPresentationCreationService.CreatePresentation(Path.GetDirectoryName(AssemblyToAnalyseLocation)));

                myAddToGraph = false;
            }

            if (document.FailedItems.Any())
            {
                foreach (var item in document.FailedItems)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Loading failed");
                    sb.AppendFormat("Item: {0}", item.Item);
                    sb.AppendLine();
                    sb.AppendFormat("Reason: {0}", item.FailureReason);
                    myStatusMessageService.Publish(new StatusMessage(sb.ToString()));
                }
            }

            Model.Presentation = presentation;
        }
    }
}
