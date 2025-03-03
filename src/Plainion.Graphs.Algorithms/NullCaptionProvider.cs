namespace Plainion.Graphs.Algorithms;

public class NullCaptionProvider : ICaptionProvider
{
    public string GetCaption(string id) => id;
}