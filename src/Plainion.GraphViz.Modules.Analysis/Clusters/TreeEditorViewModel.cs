using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Windows.Mvvm;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

class TreeEditorViewModel : ViewModelBase
{
    private readonly NodeViewModel myRoot;

    public TreeEditorViewModel(IDomainModel model, NodeViewModel root)
        : base(model)
    {
        System.Contract.RequiresNotNull(root);

        myRoot = root;

        ExpandAllCommand = new DelegateCommand(() => myRoot.ExpandAll());
        CollapseAllCommand = new DelegateCommand(() => myRoot.CollapseAll());
    }

    public DelegateCommand ExpandAllCommand { get; }
    public DelegateCommand CollapseAllCommand { get; }
}