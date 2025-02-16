using System;
using System.Collections.Generic;
using System.IO;
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
            var layoutAlgorithm = presentation.GetModule<IGraphLayoutModule>().Algorithm;

            var writer = new DotWriter(myDotFile.FullName);
            writer.FastRenderingNodeCountLimit = FastRenderingNodeCountLimit;
            writer.IgnoreStyle = true;

            if (layoutAlgorithm == LayoutAlgorithm.Flow)
            {
                writer.Settings = DotPresets.Flow;
            }

            var writtenNodesCount = writer.Write(presentation.GetModule<ITransformationModule>().Graph, presentation.Picking, presentation);

            myConverter.Algorithm = layoutAlgorithm == LayoutAlgorithm.Auto && writtenNodesCount > FastRenderingNodeCountLimit
                ? LayoutAlgorithm.ScalableForcceDirectedPlancement
                : presentation.GetModule<IGraphLayoutModule>().Algorithm;

            myConverter.Convert(myDotFile, myPlainFile);

            // if converter changed algo (e.g. because of issues) we want to re-apply it to the presentation
            presentation.GetModule<IGraphLayoutModule>().Algorithm = myConverter.Algorithm;

            var nodeLayouts = new List<NodeLayout>();
            var edgeLayouts = new List<EdgeLayout>();

            ParsePlainFile(nodeLayouts, edgeLayouts);

            var module = presentation.GetModule<IGraphLayoutModule>();
            module.Set(nodeLayouts, edgeLayouts);
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
    }
}
