using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Dot
{
    public class DotToolLayoutEngine : ILayoutEngine
    {
        private readonly FileInfo myDotFile;
        private readonly FileInfo myPlainFile;
        private readonly DotToDotPlainConverter myConverter;

        public DotToolLayoutEngine(DotToDotPlainConverter converter)
        {
            myConverter = converter;

            var sessionId = Guid.NewGuid().ToString();
            myDotFile = new FileInfo(Path.Combine(Path.GetTempPath(), sessionId + ".dot"));
            myPlainFile = new FileInfo(Path.Combine(Path.GetTempPath(), sessionId + ".plain"));
        }

        public void Relayout(IGraphPresentation presentation)
        {
            var graph = presentation.GetModule<ITransformationModule>().Graph;

            // "Auto" is a hierarchical layout which does not make much sense when exceeding certain node limit
            // For historical reasons: 300
            var layoutAlgorithm = presentation.GetModule<IGraphLayoutModule>().Algorithm;
            layoutAlgorithm = layoutAlgorithm == LayoutAlgorithm.Auto && graph.Nodes.Count(presentation.Picking.Pick) > 300
                ? LayoutAlgorithm.ScalableForceDirectedPlancement
                : layoutAlgorithm;

            var writer = new DotWriter(myDotFile.FullName)
            {
                IgnoreStyle = true,
            };
            writer.Write(graph, presentation.Picking, presentation);
            layoutAlgorithm = ConvertWithFallback(layoutAlgorithm);

            // if converter changed algo (e.g. because of issues) we want to re-apply it to the presentation
            presentation.GetModule<IGraphLayoutModule>().Algorithm = layoutAlgorithm;

            var nodeLayouts = new List<NodeLayout>();
            var edgeLayouts = new List<EdgeLayout>();

            ParsePlainFile(nodeLayouts, edgeLayouts);

            SetLayouts(graph, presentation, nodeLayouts, edgeLayouts);
        }

        private LayoutAlgorithm ConvertWithFallback(LayoutAlgorithm algorithm)
        {
            try
            {
                myConverter.Convert(algorithm, myDotFile, myPlainFile);
                return algorithm;
            }
            catch
            {
                if (algorithm == LayoutAlgorithm.Hierarchy || algorithm == LayoutAlgorithm.Flow || algorithm == LayoutAlgorithm.Auto)
                {
                    // unfort dot.exe dies quite often with "trouble in init_rank" if graph is too complex
                    // -> try fallback with sfdp.exe
                    return ConvertWithFallback(LayoutAlgorithm.ScalableForceDirectedPlancement);
                }
                else
                {
                    throw;
                }
            }
        }

        private void ParsePlainFile(List<NodeLayout> nodeLayouts, List<EdgeLayout> edgeLayouts)
        {
            using (var reader = new DotPlainReader(new StreamReader(myPlainFile.FullName)))
            {
                var parser = new DotPlainParser(reader);

                parser.Open();

                while (parser.MoveNextEntry("node"))
                {
                    var nodeId = parser.ReadId();

                    var layout = parser.ReadNodeLayout(nodeId);
                    nodeLayouts.Add(layout);
                }

                // edge "MyProject.Facade" "MyProject.EventBroker" 4 3.2872 0.48858 3.999 1.3306 6.5111 4.3023 7.3657 5.3132 "." 5.5417 2.9583 solid black
                // edge "MyProject.Facade" "MyProject.EventBroker" 4 3.0211 0.48395 3.7425 1.2714 6.1987 3.9522 7.0675 4.9004 solid black
                while (parser.MoveNextEntry("edge"))
                {
                    var sourceNodeId = parser.ReadId();
                    var targetNodeId = parser.ReadId();

                    var edgeId = Edge.CreateId(sourceNodeId, targetNodeId);

                    var layout = parser.ReadEdgeLayout(edgeId);

                    var label = parser.ReadLabel(edgeId);

                    if (label != null && label.Label != "invis")
                    {
                        // no label position
                        layout.LabelPosition = parser.ReadPoint();
                    }

                    edgeLayouts.Add(layout);
                }
            }
        }

        private void SetLayouts(IGraph graph, IGraphPresentation presentation, List<NodeLayout> nodeLayouts, List<EdgeLayout> edgeLayouts)
        {
            var module = presentation.GetModule<IGraphLayoutModule>();
            module.Set(nodeLayouts, edgeLayouts);

            var maxXNode = nodeLayouts.OrderByDescending(x => x.Center.X).First();
            var graphWidth = maxXNode.Center.X + maxXNode.Width;

            // Observation: "neato" skips nodes without edges.
            // to prevent rendering from crashing lets add dummy layouts
            var y = 1.0;
            const double nodeWidth = 5.0;
            const double nodeHeight = 0.5;
            foreach (var node in graph.Nodes.Where(presentation.Picking.Pick))
            {
                if (module.GetLayout(node) == null)
                {
                    // defaults derived once from some ".plain" output
                    // node "A" 27.092 14.774 4.1987 0.5 "A" solid ellipse black lightgrey
                    module.Add(new NodeLayout(node.Id)
                    {
                        Center = new Point(graphWidth + nodeWidth, y), // includes margin to left
                        Width = nodeWidth,
                        Height = nodeHeight
                    });
                    y += nodeHeight + (2 * nodeHeight); // include padding to top
                }
            }

            // Observation: "neato" skips edges for unclear reason.
            // to prevent rendering from crashing lets add dummy layouts
            foreach (var edge in graph.Edges.Where(presentation.Picking.Pick))
            {
                if (module.GetLayout(edge) == null)
                {
                    // defaults derived once from some ".plain" output
                    // edge "MyProject.Facade" "MyProject.EventBroker" 4 3.2872 0.48858 3.999 1.3306 6.5111 4.3023 7.3657 5.3132 "." 5.5417 2.9583 solid black
                    module.Add(new EdgeLayout(edge.Id)
                    {
                        // we need at least 2 points to not crash the renderer
                        Points = [
                            new Point(graphWidth, 1),
                            new Point(graphWidth, 1)
                        ]
                    });
                }
            }

        }
    }
}
