
namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance.Services.Framework
{
    public interface ICancellationToken
    {
        bool IsCancellationRequested { get; }
    }
}
