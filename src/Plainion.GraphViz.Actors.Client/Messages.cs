namespace Plainion.GraphViz.Actors.Client;

public class CancelMessage { }

public class CanceledMessage { }

public class FailedMessage
{
    // https://github.com/akkadotnet/akka.net/issues/1409
    // -> exceptions are currently not serializable in raw version
    public string Error { get; set; }
}

public class FinishedMessage { }
