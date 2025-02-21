using System;
using System.IO;
using System.Linq;
using Plainion.GraphViz.Dot;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.CodeInspection;

public static class GraphUtils
{
    public static void Serialize(string file, IGraphPresentation presentation)
    {
        var graph = presentation.GetModule<ITransformationModule>().Graph;

        if (graph.Nodes.Any(presentation.Picking.Pick))
        {
            Console.WriteLine("Dumping graph ...");

            var writer = new DotWriter(file)
            {
                PrettyPrint = true
            };

            writer.Write(graph, presentation.Picking, presentation);
        }
        else
        {
            Console.WriteLine("Graph is empty");

            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }
}
