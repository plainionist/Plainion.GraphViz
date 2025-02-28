using System;
using System.ComponentModel;
using System.Windows.Input;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Interactivity.InteractionRequest;
using Prism.Commands;

namespace Plainion.GraphViz.Modules.Properties;

class PropertiesViewModel : ViewModelBase, IInteractionRequestAware
{
    private ToolProperties myGraphProperties;

    public PropertiesViewModel(IDomainModel model)
         : base(model)
    {
        OkCommand = new DelegateCommand(OnOk);

        GraphProperties = new ToolProperties();
    }

    public ICommand OkCommand { get; }

    private void OnOk()
    {
        FinishInteraction();
    }

    protected override void OnPresentationChanged()
    {
    }

    public Action FinishInteraction { get; set; }

    public INotification Notification { get; set; }

    public ToolProperties GraphProperties
    {
        get { return myGraphProperties; }
        set { SetProperty(ref myGraphProperties, value); }
    }
}

public class ToolProperties
{
    public string Name { get; set; } = "Default Tool";

    [ReadOnly(true)] // Make this read-only in UI
    public string ID { get; set; } = "Tool123";

    public int Speed { get; set; } = 100;

    public bool IsEnabled { get; set; } = true;
}

