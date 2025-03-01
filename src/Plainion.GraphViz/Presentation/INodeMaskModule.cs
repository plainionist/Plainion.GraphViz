
namespace Plainion.GraphViz.Presentation;

public interface INodeMaskModule : IModule<INodeMask>
{
    void Push(INodeMask mask);

    void Insert(int pos, INodeMask mask);

    void MoveDown(INodeMask item);

    void MoveUp(INodeMask item);

    void Remove(INodeMask item);
}
