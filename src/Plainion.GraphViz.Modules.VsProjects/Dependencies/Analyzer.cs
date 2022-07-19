using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Plainion.Collections;

namespace Plainion.GraphViz.Modules.VsProjects.Dependencies
{
    class Analyzer
    {
        public Task<AnalysisDocument> AnalyzeAsync(string folderToAnalyze, CancellationToken token) =>
            Task.Run(() => AnalyzeCore(folderToAnalyze), token);

        private AnalysisDocument AnalyzeCore(string folderToAnalyze)
        {
            var doc = new AnalysisDocument();
            if (!Directory.Exists(folderToAnalyze))
            {
                doc.FailedItems.Add(new FailedProject
                {
                    FullPath = folderToAnalyze,
                    Exception = new DirectoryNotFoundException()
                });

                return doc;
            }

            var results = Directory.GetFiles(folderToAnalyze, "*.*proj", SearchOption.AllDirectories)
                .Select(TryLoadProject)
                .ToList();

            doc.Projects.AddRange(results.OfType<VsProject>());
            doc.FailedItems.AddRange(results.OfType<FailedProject>());

            return doc;
        }

        private object TryLoadProject(string path)
        {
            try
            {
                var doc = XElement.Load(path);

                var projectName = Path.GetFileNameWithoutExtension(path);
                var projectHome = Path.GetDirectoryName(path);
                var assemblyNameProperty = doc.Elements()
                    .Where(x => x.Name.LocalName == "PropertyGroup")
                    .SelectMany(x => x.Elements())
                    .FirstOrDefault(x => x.Name.LocalName == "AssemblyName");

                // <PackageReference Include="DotNetProjects.WpfToolkit.Input" Version="6.1.94" />
                // <ProjectReference Include="..\Plainion.GraphViz.Infrastructure\Plainion.GraphViz.Infrastructure.csproj" />
                // <Reference Include="System.ServiceModel" />
                return new VsProject
                {
                    Name = projectName,
                    FullPath = path,
                    Assembly = assemblyNameProperty != null ? assemblyNameProperty.Value : projectName,
                    References = doc.Elements()
                        .Where(x => x.Name.LocalName == "ItemGroup")
                        .SelectMany(x => x.Elements())
                        .Where(x => x.Name.LocalName == "Reference")
                        .Select(x => x.Attribute("Include").Value)
                        .Select(x => x.Split(',').First())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList(),
                    ProjectReferences = doc.Elements()
                        .Where(x => x.Name.LocalName == "ItemGroup")
                        .SelectMany(x => x.Elements())
                        .Where(x => x.Name.LocalName == "ProjectReference")
                        .Select(x => x.Attribute("Include").Value)
                        .Select(x => Path.GetFullPath(Path.Combine(projectHome, x)))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList(),
                    PackageReferences = doc.Elements()
                        .Where(x => x.Name.LocalName == "ItemGroup")
                        .SelectMany(x => x.Elements())
                        .Where(x => x.Name.LocalName == "PackageReference")
                        .Select(x => x.Attribute("Include").Value)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList()
                };
            }
            catch (Exception ex)
            {
                return new FailedProject
                {
                    FullPath = path,
                    Exception = ex
                };
            }
        }
    }
}