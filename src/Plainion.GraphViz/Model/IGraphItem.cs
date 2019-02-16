
using System;

namespace Plainion.GraphViz.Model
{
    public interface IGraphItem : IEquatable<IGraphItem>
    {
        string Id { get; }
    }
}
