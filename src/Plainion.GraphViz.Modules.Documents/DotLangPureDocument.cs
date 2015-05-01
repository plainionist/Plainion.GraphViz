using System.Collections.Generic;
using System.IO;
using Plainion.GraphViz.Presentation;
using System;

namespace Plainion.GraphViz.Modules.Documents
{
    // http://www.graphviz.org/doc/info/lang.html
    public class DotLangPureDocument : AbstractGraphDocument
    {
        protected override void Load()
        {
            using( var reader = new StreamReader( Filename ) )
            {
                while( !reader.EndOfStream )
                {
                    var line = reader.ReadLine();

                    if( string.IsNullOrWhiteSpace( line ) )
                    {
                        continue;
                    }

                    if( line.StartsWith( "digraph" ) )
                    {
                        continue;
                    }

                    if( line.Trim() == "{" || line.Trim() == "}" )
                    {
                        continue;
                    }

                    if( line.Split( '=' ).Length > 1 )
                    {
                        // skip attributes
                        continue;
                    }

                    if( !line.Contains( "->" ) )
                    {
                        TryAddNode( line.Trim( ' ', '"' ) );
                    }
                    else
                    {
                        var tokens = line.Split( new[] { "->" }, StringSplitOptions.RemoveEmptyEntries );
                        var source = tokens[ 0 ].Trim( ' ', '"' );
                        var target = tokens[ 1 ].Trim( ' ', '"' );
                        TryAddEdge( source, target );
                    }
                }
            }
        }
    }
}
