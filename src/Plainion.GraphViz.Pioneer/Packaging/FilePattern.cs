using System.Diagnostics;
using Plainion.Text;
namespace Plainion.GraphViz.Pioneer.Packaging
{
    public class FilePattern
    {
        public string Pattern { get; set; }
    
        internal bool Matches(string file)
        {
            return Wildcard.IsMatch(file, Pattern);
        }
    }
}
