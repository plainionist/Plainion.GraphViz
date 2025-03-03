using System;
using System.Runtime.Serialization;

namespace Plainion.Graphs.Projections;

[Serializable]
public abstract class AbstractNodeMask : NotifyPropertyChangedBase, INodeMask
{
    private bool myIsApplied;
    private bool myIsShowMask;
    private string myLabel;

    protected AbstractNodeMask()    {    }

    protected AbstractNodeMask(SerializationInfo info, StreamingContext context)
    {
        myLabel = (string)info.GetValue("Label", typeof(string));
        myIsApplied = (bool)info.GetValue("IsApplied", typeof(bool));
        myIsShowMask = (bool)info.GetValue("IsShowMask", typeof(bool));
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Label", myLabel);
        info.AddValue("IsApplied", myIsApplied);
        info.AddValue("IsShowMask", myIsShowMask);
    }

    protected override void OnPropertyChanged(string propertyName)
    {
        IsDirty = true;

        base.OnPropertyChanged(propertyName);
    }

    public string Label
    {
        get { return myLabel; }
        set { SetProperty(ref myLabel, value); }
    }

    public bool IsShowMask
    {
        get { return myIsShowMask; }
        set { SetProperty(ref myIsShowMask, value); }
    }

    public bool IsApplied
    {
        get { return myIsApplied; }
        set { SetProperty(ref myIsApplied, value); }
    }

    public abstract bool? IsSet(Node node);

    public bool IsDirty
    {
        get;
        protected set;
    }

    public void MarkAsClean()
    {
        IsDirty = false;
    }

    public abstract void Invert(IGraph graph, IGraphPicking picking);
}
