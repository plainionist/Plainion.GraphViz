
namespace Plainion.GraphViz.Viewer.Abstractions.Services
{
    public class StatusMessage
    {
        public StatusMessage( string message )
        {
            Message = message;
        }

        public string Message { get; private set; }
    }
}
