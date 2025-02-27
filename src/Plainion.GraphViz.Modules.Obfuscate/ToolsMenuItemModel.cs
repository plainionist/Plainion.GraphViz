using System.IO;
using System.Linq;
using Plainion.GraphViz.Viewer.Abstractions.Services;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.GraphViz.Model;
using Prism.Commands;
using Prism.Mvvm;
using System;
using Plainion.GraphViz.Presentation;
using System.Xml.Linq;

namespace Plainion.GraphViz.Modules.Obfuscate;

class ToolsMenuItemModel : BindableBase
{
    private readonly IDomainModel myModel;
    private readonly IPresentationCreationService myPresentationCreationService;

    public ToolsMenuItemModel(IDomainModel model, IPresentationCreationService presentationCreationService)
    {
        myModel = model;
        myPresentationCreationService = presentationCreationService;

        myModel.PresentationChanged += OnPresentationChanged;

        SimpleAlgoCommand = new DelegateCommand(() => Obfuscate(new SimpleObfuscator()), () => myModel.Presentation != null);
        PreserveStructureAlgoCommand = new DelegateCommand(() => Obfuscate(new StructureAwareObfuscator()), () => myModel.Presentation != null);
        RemoveAllCaptionsCommand = new DelegateCommand(OnRemoveAllCaptions, () => myModel.Presentation != null);
    }

    private void OnPresentationChanged(object sender, System.EventArgs e)
    {
        SimpleAlgoCommand.RaiseCanExecuteChanged();
        PreserveStructureAlgoCommand.RaiseCanExecuteChanged();
        RemoveAllCaptionsCommand.RaiseCanExecuteChanged();
    }

    public DelegateCommand SimpleAlgoCommand { get; }
    public DelegateCommand PreserveStructureAlgoCommand { get; }
    public DelegateCommand RemoveAllCaptionsCommand { get; }

    private void Obfuscate(IObfuscator obfuscator)
    {
        var builder = new RelaxedGraphBuilder();

        foreach (var node in myModel.Presentation.Graph.Nodes)
        {
            builder.TryAddNode(obfuscator.Obfuscate(node.Id));
        }

        foreach (var edge in myModel.Presentation.Graph.Edges)
        {
            builder.TryAddEdge(obfuscator.Obfuscate(edge.Source.Id), obfuscator.Obfuscate(edge.Target.Id));
        }

        foreach (var cluster in myModel.Presentation.Graph.Clusters)
        {
            builder.TryAddCluster(obfuscator.Obfuscate(cluster.Id), cluster.Nodes.Select(x => obfuscator.Obfuscate(x.Id)));
        }

        var presentation = myPresentationCreationService.CreatePresentation(Path.GetTempPath());
        presentation.Graph = builder.Graph;

        myModel.Presentation = presentation;
    }

    // If there is no caption then there will be the ID shown as fallback so
    // instead of removing we ensure every node has a cpation which is a single space
    private void OnRemoveAllCaptions()
    {
        var captions = myModel.Presentation.GetModule<ICaptionModule>();

        // we need a copy here because there are observers on the captions module
        // possibly modifying the caption
        var allIds = myModel.Presentation.Graph.Nodes.Select(x => x.Id)
            .Concat(myModel.Presentation.Graph.Edges.Select(x => x.Id))
            .Concat(myModel.Presentation.Graph.Clusters.Select(x => x.Id))
            .ToList();

        foreach (var id in allIds)
        {
            captions.Get(id).DisplayText = " ";
        }
    }
}
