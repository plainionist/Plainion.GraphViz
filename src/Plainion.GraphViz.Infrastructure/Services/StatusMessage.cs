
namespace Plainion.GraphViz.Infrastructure.Services
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
