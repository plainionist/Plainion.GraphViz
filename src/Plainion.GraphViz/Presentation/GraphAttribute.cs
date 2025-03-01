using Plainion.GraphViz.Dot;

namespace Plainion.GraphViz.Presentation;

public class GraphAttribute
{
    /// <summary>
    /// Value can be null or empty string
    /// </summary>
    public GraphAttribute(LayoutAlgorithm algorithm, string name, string value)
    {
        System.Contract.RequiresNotNullNotEmpty(name);

        Algorithm = algorithm;
        Name = name;
        Value = value;
    }

    public LayoutAlgorithm Algorithm { get; }
    public string Name { get; }
    public string Value { get; set; }
}


