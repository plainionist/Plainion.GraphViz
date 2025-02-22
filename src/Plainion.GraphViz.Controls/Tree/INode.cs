
using System.Collections.Generic;

namespace Plainion.GraphViz.Controls.Tree;

/// <summary>
/// Required interface of nodes in <see cref="TreeEditor"/>.
/// </summary>
public interface INode
{
    INode Parent { get; }

    IEnumerable<INode> Children { get; }

    bool Matches(string pattern);
}
