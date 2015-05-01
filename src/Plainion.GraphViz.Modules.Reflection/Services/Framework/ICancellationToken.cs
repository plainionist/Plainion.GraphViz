
namespace Plainion.GraphViz.Modules.Reflection.Services.Framework
{
    public interface ICancellationToken
    {
        bool IsCancellationRequested { get; }
    }
}
