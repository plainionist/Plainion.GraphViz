
namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance.Services
{
    public interface ICancellationToken
    {
        bool IsCancellationRequested { get; }
    }
}
