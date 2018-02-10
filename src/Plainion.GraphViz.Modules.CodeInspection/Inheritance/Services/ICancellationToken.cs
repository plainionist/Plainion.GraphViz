
namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance.Services
{
    interface ICancellationToken
    {
        bool IsCancellationRequested { get; }
    }
}
