using Plainion.GraphViz.Dot;

namespace Plainion.GraphViz.Presentation;

public class GraphAttribute
{
    private string myValue;

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
    public string Value
    {
        get { return myValue; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                myValue = null;
            }
            else
            {
                myValue = value.Trim();
            }
        }
    }
}

