using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Analysis.Services
{
    [Export( typeof( INodeMasksPersistanceService ) )]
    class NodeMasksPersistanceService : INodeMasksPersistanceService
    {
        public IEnumerable<INodeMask> Load( string filename )
        {
            using( var stream = new FileStream( filename, FileMode.Open, FileAccess.Read ) )
            {
                var formatter = new BinaryFormatter();
                return ( List<INodeMask> )formatter.Deserialize( stream );
            }
        }

        public void Save( string filename, IEnumerable<INodeMask> masks )
        {
            using( var stream = new FileStream( filename, FileMode.OpenOrCreate, FileAccess.Write ) )
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize( stream, masks.ToList() );
            }
        }
    }
}
