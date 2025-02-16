using System;
using System.Diagnostics;
using System.IO;
using Plainion.Diagnostics;

namespace Plainion.GraphViz.Dot
{
    public class DotToDotPlainConverter
    {
        private string myDotToolsHome;

        public DotToDotPlainConverter(string dotToolsHome)
        {
            myDotToolsHome = dotToolsHome;

            if (!Directory.Exists(myDotToolsHome))
            {
                throw new DirectoryNotFoundException(myDotToolsHome);
            }

            if (!File.Exists(Path.Combine(myDotToolsHome, "dot.exe")))
            {
                throw new IOException("DotToolsHome invalid. Dot.exe not found");
            }

            Algorithm = LayoutAlgorithm.Auto;
        }

        public LayoutAlgorithm Algorithm { get; set; }

        public void Convert(FileInfo dotFile, FileInfo plainFile)
        {
            try
            {
                string arguments;

                if (Algorithm == LayoutAlgorithm.Hierarchy || Algorithm == LayoutAlgorithm.Flow || Algorithm == LayoutAlgorithm.Auto)
                {
                    arguments = CreateArgumentsForDot(dotFile, plainFile);
                }
                else if (Algorithm == LayoutAlgorithm.ForceDirectedPlacement)
                {
                    arguments = CreateArgumentsForFdp(dotFile, plainFile);
                }
                else if (Algorithm == LayoutAlgorithm.NeatSpring)
                {
                    arguments = CreateArgumentsForNeato(dotFile, plainFile);
                }
                else
                {
                    arguments = CreateArgumentsForSfdp(dotFile, plainFile);
                }

                var startInfo = new ProcessStartInfo("cmd", arguments);
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.WorkingDirectory = Path.GetTempPath();

                var stdErr = new StringWriter();
                var ret = Processes.Execute(startInfo, null, stdErr);

                if (Algorithm == LayoutAlgorithm.ScalableForcceDirectedPlancement)
                {
                    // ignore the error code for this engine - it mostly still works :)
                    ret = 0;
                }

                if (ret != 0 || !plainFile.Exists || dotFile.LastWriteTime > plainFile.LastWriteTime)
                {
                    // limit the size of the error message otherwise we will blow the messagebox window
                    // showing this unhandled exception later on
                    var msg = stdErr.ToString();
                    if (msg.Length > 512)
                    {
                        msg = msg.Substring(0, 512);
                    }
                    throw new InvalidOperationException("Dot plain file generation failed: " + msg);
                }
            }
            catch
            {
                if (Algorithm == LayoutAlgorithm.Hierarchy || Algorithm == LayoutAlgorithm.Flow || Algorithm == LayoutAlgorithm.Auto)
                {
                    // unfort dot.exe dies quite often with "trouble in init_rank" if graph is too complex
                    // -> try fallback with sfdp.exe
                    Algorithm = LayoutAlgorithm.ScalableForcceDirectedPlancement;
                    Convert(dotFile, plainFile);
                }
                else
                {
                    throw;
                }
            }
        }

        private string CreateArgumentsForDot(FileInfo dotFile, FileInfo plainFile)
        {
            var unflattenExe = Path.Combine(myDotToolsHome, "unflatten.exe");
            var dotExe = Path.Combine(myDotToolsHome, "dot.exe");
            return $"/C \"\"{unflattenExe}\" -l5 -c8 {dotFile.FullName} | \"{dotExe}\" -Tplain -q -o{plainFile.FullName}\"";
        }

        private string CreateArgumentsForSfdp(FileInfo dotFile, FileInfo plainFile)
        {
            var exe = Path.Combine(myDotToolsHome, "sfdp.exe");
            return $"/C \"\"{exe}\" -x -Goverlap=scale -Tplain -q -o{plainFile.FullName} {dotFile.FullName}\"";
        }

        private string CreateArgumentsForFdp(FileInfo dotFile, FileInfo plainFile)
        {
            var exe = Path.Combine(myDotToolsHome, "fdp.exe");
            return $"/C \"\"{exe}\" -x -Goverlap=prism -Gstart=rand -Gsplines=true -Gpackmode=graph -Tplain -q -o{plainFile.FullName} {dotFile.FullName}\"";
        }

        private string CreateArgumentsForNeato(FileInfo dotFile, FileInfo plainFile)
        {
            var exe = Path.Combine(myDotToolsHome, "neato.exe");
            return $"/C \"\"{exe}\" -x -Gmode=ipsep -Goverlap=false -Gstart=rand -Gsplines=true -q -o{plainFile.FullName} {dotFile.FullName}\"";
        }
    }
}
