using System;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using Akka.Actor;
using Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Spec;

namespace Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Actors
{
    class PackageAnalysingActor : ReceiveActor
    {
        public PackageAnalysingActor()
        {
            Receive<GraphBuildRequest>(r => OnReceive(r));
        }

        private bool OnReceive(GraphBuildRequest request)
        {
            try
            {
                var activity = new AnalyzePackageDependencies();
                activity.OutputFile = request.OutputFile;

                using (var reader = new StringReader(request.Spec))
                {
                    var spec = (SystemPackaging)XamlReader.Load(XmlReader.Create(reader));
                    activity.Execute(spec);
                }

                Sender.Tell(activity.OutputFile, Self);
            }
            catch (Exception e)
            {
                Sender.Tell(new Failure { Exception = e }, Self);
            }

            return true;
        }
    }
}
