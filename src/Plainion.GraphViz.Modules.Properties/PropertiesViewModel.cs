using System;
using System.ComponentModel;
using System.Linq;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Interactivity.InteractionRequest;

namespace Plainion.GraphViz.Modules.Properties;

class PropertiesViewModel : ViewModelBase, IInteractionRequestAware
{
    private GraphAttributeCollection myGraphAttributes;

    public PropertiesViewModel(IDomainModel model)
         : base(model)
    {
        if (Model.Presentation != null)
        {
            GraphAttributes = new GraphAttributeCollection(Model.Presentation.GetModule<IGraphAttributesModule>());
        }
    }

    protected override void OnPresentationChanged()
    {
        if (Model.Presentation != null)
        {
            GraphAttributes = new GraphAttributeCollection(Model.Presentation.GetModule<IGraphAttributesModule>());
        }
    }

    public Action FinishInteraction { get; set; }

    public INotification Notification { get; set; }

    public GraphAttributeCollection GraphAttributes
    {
        get { return myGraphAttributes; }
        set { SetProperty(ref myGraphAttributes, value); }
    }
}

[DisplayName("Graph Attributes")]
public class GraphAttributeCollection(IGraphAttributesModule module) : ICustomTypeDescriptor
{
    private readonly PropertyDescriptorCollection myProperties = new(module.Items.Select(x => new CustomItemPropertyDescriptor(x)).ToArray());

    public PropertyDescriptorCollection GetProperties() => myProperties;
    public PropertyDescriptorCollection GetProperties(Attribute[] attributes) => GetProperties();
    public string GetClassName() => nameof(GraphAttributeCollection);
    public string GetComponentName() => nameof(GraphAttributeCollection);
    public System.ComponentModel.TypeConverter GetConverter() => null;
    public EventDescriptor GetDefaultEvent() => null;
    public PropertyDescriptor GetDefaultProperty() => null;
    public object GetEditor(Type editorBaseType) => null;
    public EventDescriptorCollection GetEvents() => EventDescriptorCollection.Empty;
    public EventDescriptorCollection GetEvents(Attribute[] attributes) => EventDescriptorCollection.Empty;
    public object GetPropertyOwner(PropertyDescriptor pd) => this;
    public AttributeCollection GetAttributes() => AttributeCollection.Empty;
}

public class CustomItemPropertyDescriptor : PropertyDescriptor
{
    private readonly GraphAttribute myItem;
    private readonly AttributeCollection myAttributes;

    // prefix name with layout algorithm so that we can have same property multiple times
    public CustomItemPropertyDescriptor(GraphAttribute item)
        : base($"{item.Algorithm}_{item.Name}", null)
    {
        myItem = item;
        myAttributes = new AttributeCollection(new CategoryAttribute(myItem.Algorithm.ToString()));
    }

    public override string DisplayName => myItem.Name;
    public override bool IsReadOnly => false;
    public override Type PropertyType => typeof(string);
    public override object GetValue(object component) => myItem.Value;
    public override void SetValue(object component, object value) { myItem.Value = Convert.ToString(value); }
    public override AttributeCollection Attributes => myAttributes;

    public override Type ComponentType => typeof(GraphAttributeCollection);
    public override bool CanResetValue(object component) => false;
    public override void ResetValue(object component) { }
    public override bool ShouldSerializeValue(object component) => false;
}