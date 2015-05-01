
namespace Plainion.GraphViz.Presentation
{
    /// <summary>
    /// Provides posibility for the application to do convertions on the label of a graph item.
    /// </summary>
    public interface ILabelConverter
    {
        string Convert( string originalLabel );
    }
}
