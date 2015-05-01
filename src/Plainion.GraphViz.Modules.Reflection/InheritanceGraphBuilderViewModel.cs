using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Plainion.Collections;
using Plainion.GraphViz.Infrastructure.Services;
using Plainion.GraphViz.Infrastructure.ViewModel;
using Plainion.GraphViz.Modules.Reflection.Inspectors;
using Plainion.GraphViz.Modules.Reflection.Services;
using Plainion.GraphViz.Modules.Reflection.Services.Framework;
using Plainion.GraphViz.Presentation;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Plainion.Prism.Interactivity.InteractionRequest;
using Plainion.Prism.Mvvm;

namespace Plainion.GraphViz.Modules.Reflection
{
    [Export( typeof( InheritanceGraphBuilderViewModel ) )]
    public class InheritanceGraphBuilderViewModel : ViewModelBase
    {
        private string myAssemblyToAnalyseLocation;
        private TypeDescriptor myTypeToAnalyse;
        private int myProgress;
        private bool myIsReady;
        private bool myIgnoreDotNetTypes;
        private Action myCancelBackgroundProcessing;
        private IInspectorHandle<InheritanceGraphInspector> myInheritanceGraphInspector;
        private IInspectorHandle<AllTypesInspector> myAllTypesInspector;
        private bool myAddToGraph;

        public InheritanceGraphBuilderViewModel()
        {
            Types = new ObservableCollection<TypeDescriptor>();
            TypeFilter = OnFilterItem;

            CreateGraphCommand = new DelegateCommand( CreateGraph, () => TypeToAnalyse != null && IsReady );
            AddToGraphCommand = new DelegateCommand( AddToGraph, () => TypeToAnalyse != null && IsReady );
            CancelCommand = new DelegateCommand( () => myCancelBackgroundProcessing(), () => !IsReady );
            BrowseAssemblyCommand = new DelegateCommand( OnBrowseClicked, () => IsReady );
            ClosedCommand = new DelegateCommand( OnClosed );

            OpenFileRequest = new InteractionRequest<OpenFileDialogNotification>();

            IsReady = true;
            IgnoreDotNetTypes = true;
        }

        private void AddToGraph()
        {
            myAddToGraph = true;
            CreateGraph();
        }

        public bool IsReady
        {
            get { return myIsReady; }
            set
            {
                if ( SetProperty( ref myIsReady, value ) )
                {
                    CreateGraphCommand.RaiseCanExecuteChanged();
                    AddToGraphCommand.RaiseCanExecuteChanged();
                    CancelCommand.RaiseCanExecuteChanged();
                    BrowseAssemblyCommand.RaiseCanExecuteChanged();
                }
            }
        }

        [Import]
        public IPresentationCreationService PresentationCreationService { get; set; }

        [Import]
        public IStatusMessageService StatusMessageService { get; set; }

        [Import]
        public AssemblyInspectionService InspectionService { get; set; }

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

            OpenFileRequest.Raise( notification,
                n =>
                {
                    if ( n.Confirmed )
                    {
                        AssemblyToAnalyseLocation = n.FileName;
                    }
                } );
        }

        public AutoCompleteFilterPredicate<object> TypeFilter { get; private set; }

        private bool OnFilterItem( string search, object item )
        {
            var type = (TypeDescriptor)item;

            return type.FullName.ToLower().Contains( search.ToLower() );
        }

        private void CreateGraph()
        {
            IsReady = false;

            InspectionService.UpdateInspectorOnDemand( ref myInheritanceGraphInspector, Path.GetDirectoryName( AssemblyToAnalyseLocation ) );

            myInheritanceGraphInspector.Value.IgnoreDotNetTypes = IgnoreDotNetTypes;
            myInheritanceGraphInspector.Value.AssemblyLocation = AssemblyToAnalyseLocation;
            myInheritanceGraphInspector.Value.SelectedType = TypeToAnalyse;

            myCancelBackgroundProcessing = InspectionService.RunAsync( myInheritanceGraphInspector.Value, v => ProgressValue = v, OnInheritanceGraphCompleted );
        }

        internal void OnClosed()
        {
            AssemblyToAnalyseLocation = null;

            if ( myCancelBackgroundProcessing != null )
            {
                myCancelBackgroundProcessing();
            }

            InspectionService.DestroyInspectorOnDemand( ref myAllTypesInspector );
            InspectionService.DestroyInspectorOnDemand( ref myInheritanceGraphInspector );

            IsReady = true;
        }

        protected override void OnModelPropertyChanged( string propertyName )
        {
        }

        public string AssemblyToAnalyseLocation
        {
            get { return myAssemblyToAnalyseLocation; }
            set
            {
                if ( SetProperty( ref myAssemblyToAnalyseLocation, value ) )
                {
                    TypeToAnalyse = null;
                    Types.Clear();

                    if ( !string.IsNullOrWhiteSpace( myAssemblyToAnalyseLocation ) && File.Exists( myAssemblyToAnalyseLocation ) )
                    {
                        InspectionService.UpdateInspectorOnDemand( ref myAllTypesInspector, Path.GetDirectoryName( AssemblyToAnalyseLocation ) );

                        myAllTypesInspector.Value.AssemblyLocation = myAssemblyToAnalyseLocation;

                        Types.AddRange( myAllTypesInspector.Value.Execute() );
                    }
                }
            }
        }

        public ObservableCollection<TypeDescriptor> Types
        {
            get;
            private set;
        }

        public TypeDescriptor TypeToAnalyse
        {
            get { return myTypeToAnalyse; }
            set
            {
                // if s.th. is typed which is not available in the list of types we will get null here
                SetProperty( ref myTypeToAnalyse, value );
                CreateGraphCommand.RaiseCanExecuteChanged();
                AddToGraphCommand.RaiseCanExecuteChanged();

                if ( myTypeToAnalyse == null )
                {
                    SetError( ValidationFailure.Error );
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
            set { SetProperty( ref myIgnoreDotNetTypes, value ); }
        }

        public int ProgressValue
        {
            get { return myProgress; }
            set { SetProperty( ref myProgress, value ); }
        }

        private void OnInheritanceGraphCompleted( TypeRelationshipDocument document )
        {
            try
            {
                if ( document == null )
                {
                    return;
                }

                if ( !document.Graph.Nodes.Any() )
                {
                    MessageBox.Show( "No nodes found" );
                    return;
                }

                var presentation = PresentationCreationService.CreatePresentation( Path.GetDirectoryName( AssemblyToAnalyseLocation ) );

                var captionModule = presentation.GetPropertySetFor<Caption>();
                var tooltipModule = presentation.GetPropertySetFor<ToolTipContent>();
                var edgeStyleModule = presentation.GetPropertySetFor<EdgeStyle>();

                presentation.Graph = document.Graph;

                foreach ( var desc in document.Descriptors )
                {
                    captionModule.Add( new Caption( desc.Id, desc.Name ) );
                    tooltipModule.Add( new ToolTipContent( desc.Id, desc.FullName ) );
                }

                foreach ( var entry in document.EdgeTypes )
                {
                    edgeStyleModule.Add( new EdgeStyle( entry.Key )
                    {
                        Color = entry.Value == EdgeType.DerivesFrom ? Brushes.Black : Brushes.Blue
                    } );
                }

                if ( myAddToGraph && Model.Presentation != null && Model.Presentation.Graph != null )
                {
                    presentation = Model.Presentation.UnionWith( presentation,
                        () => PresentationCreationService.CreatePresentation( Path.GetDirectoryName( AssemblyToAnalyseLocation ) ) );

                    myAddToGraph = false;
                }

                if ( document.FailedItems.Any() )
                {
                    foreach ( var item in document.FailedItems )
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine( "Loading failed" );
                        sb.AppendFormat( "Item: {0}", item.Item );
                        sb.AppendLine();
                        sb.AppendFormat( "Reason: {0}", item.FailureReason );
                        StatusMessageService.Publish( new StatusMessage( sb.ToString() ) );
                    }
                }

                Model.Presentation = presentation;
            }
            finally
            {
                myCancelBackgroundProcessing = null;
                ProgressValue = 0;
                IsReady = true;
            }
        }
    }
}
