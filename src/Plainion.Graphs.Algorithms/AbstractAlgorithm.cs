using System;
using Plainion.Graphs.Projections;

namespace Plainion.Graphs.Algorithms;

public class AbstractAlgorithm
{
    public AbstractAlgorithm(IGraphProjections projections)
    {
        Contract.RequiresNotNull(projections);

        Projections = projections;
    }

    protected IGraphProjections Projections { get; }
}
