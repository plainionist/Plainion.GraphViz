using System.Collections.Generic;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Analysis.Services
{
    public interface INodeMasksPersistanceService
    {
        IEnumerable<INodeMask> Load( string filename );
        void Save( string filename, IEnumerable<INodeMask> masks );
    }
}
