using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Dot
{
    public class DotToolLayoutEngine : ILayoutEngine
    {
        private readonly FileInfo myDotFile;
        private readonly FileInfo myPlainFile;
        private readonly DotToDotPlainConverter myConverter;

        public static readonly int FastRenderingNodeCountLimit = 300;

        public DotToolLayoutEngine(DotToDotPlainConverter converter)
        {
            myConverter = converter;

            var sessionId = Guid.NewGuid().ToString();
            myDotFile = new FileInfo(Path.Combine(Path.GetTempPath(), sessionId + ".dot"));
            myPlainFile = new FileInfo(Path.Combine(Path.GetTempPath(), sessionId + ".plain"));
        }

        public void Relayout(IGraphPresentation presentation)
        {
            var writer = new DotWriter(myDotFile.FullName);
            writer.FastRenderingNodeCountLimit = FastRenderingNodeCountLimit;
            writer.IgnoreStyle = true;

            var writtenNodesCount = writer.Write(presentation);

            myConverter.Algorithm = presentation.GetModule<IGraphLayoutModule>().Algorithm == LayoutAlgorithm.Auto && writtenNodesCount > FastRenderingNodeCountLimit
                ? LayoutAlgorithm.Sfdp
                : presentation.GetModule<IGraphLayoutModule>().Algorithm;

            myConverter.Convert(myDotFile, myPlainFile);

            // if converter changed algo (e.g. because of issues) we want to re-apply it to the presentation
            presentation.GetModule<IGraphLayoutModule>().Algorithm = myConverter.Algorithm;

            var nodeLayouts = new List<NodeLayout>();
            var edgeLayouts = new List<EdgeLayout>();

            ParsePlainFile(nodeLayouts, edgeLayouts, presentation.GetPropertySetFor<Caption>());

            var module = presentation.GetModule<IGraphLayoutModule>();
            module.Set(nodeLayouts, edgeLayouts);
        }

        private void ParsePlainFile(List<NodeLayout> nodeLayouts, List<EdgeLayout> edgeLayouts, IPropertySetModule<Caption> captionModule)
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

                while (parser.MoveNextEntry("edge"))
                {
                    var sourceNodeId = parser.ReadId();
                    var targetNodeId = parser.ReadId();

                    var edgeId = Edge.CreateId(sourceNodeId, targetNodeId);

                    var layout = parser.ReadEdgeLayout(edgeId);

                    var label = parser.ReadLabel(edgeId);

                    if (label.Label != "invis")
                    {
                        // no label position
                        layout.LabelPosition = parser.ReadPoint();
                    }

                    edgeLayouts.Add(layout);
                }
            }
        }
    }
}
