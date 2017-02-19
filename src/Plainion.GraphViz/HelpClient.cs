using System.Diagnostics;

namespace Plainion.GraphViz
{
    /// <summary>
    /// API to the help system
    /// </summary>
    public class HelpClient
    {
        public static uint Port = 0;

        public static void OpenPage( string relativePath )
        {
            Process.Start( string.Format( "http://localhost:{0}{1}", Port, relativePath ) );
        }
    }
}
