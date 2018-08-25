namespace Plainion.GraphViz.Modules.CodeInspection.Actors
{
    class CancelMessage
    {
    }

    class CanceledMessage
    {
    }

    class FailedMessage
    {
        // https://github.com/akkadotnet/akka.net/issues/1409
        // -> exceptions are currently not serializable in raw version
        public string Error { get; set; }
    }

    class FinishedMessage
    {
    }
}
