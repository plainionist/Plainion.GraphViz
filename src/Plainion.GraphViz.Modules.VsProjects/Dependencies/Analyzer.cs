using System;
using System.Collections;
using System.Collections.Generic;
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

            static IEnumerable<string> GetDirectories(string folder)
            {
                bool VisitFolder(string folder) =>
                    !Path.GetFileName(folder).Equals("node_modules", StringComparison.OrdinalIgnoreCase);

                yield return folder;
                foreach (var subFolder in Directory.GetDirectories(folder).Where(VisitFolder).SelectMany(GetDirectories))
                {
                    yield return subFolder;
                }
            }

            var results = GetDirectories(folderToAnalyze)
                .SelectMany(x => Directory.GetFiles(x, "*.*proj"))
                .Select(x => TryLoadProject(folderToAnalyze, x))
                .ToList();

            doc.Projects.AddRange(results.OfType<VsProject>());
            doc.FailedItems.AddRange(results.OfType<FailedProject>());

            return doc;
        }

        private object TryLoadProject(string home, string path)
        {
            home = home.TrimEnd('/', '\\');

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
                    RelativePath = path.Substring(home.Length).TrimStart('/', '\\'),
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
                        .Select(x => Path.GetFullPath(Path.Combine(projectHome, x)).Substring(home.Length).TrimStart('/', '\\'))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList(),
                    PackageReferences = doc.Elements()
                        .Where(x => x.Name.LocalName == "ItemGroup")
                        .SelectMany(x => x.Elements())
                        .Where(x => x.Name.LocalName == "PackageReference")
                        .Select(x => x.Attribute("Include"))
                        .Where(x => x != null)
                        .Select(x => x.Value)
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