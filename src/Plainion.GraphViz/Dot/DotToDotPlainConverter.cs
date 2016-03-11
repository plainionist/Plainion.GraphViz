using System;
using System.Diagnostics;
using System.IO;
using Plainion.IO;

namespace Plainion.GraphViz.Dot
{
    public class DotToDotPlainConverter
    {
        private string myDotToolsHome;

        public DotToDotPlainConverter( string dotToolsHome )
        {
            myDotToolsHome = dotToolsHome;

            if( !Directory.Exists( myDotToolsHome ) )
            {
                throw new DirectoryNotFoundException( myDotToolsHome );
            }

            if( !File.Exists( Path.Combine( myDotToolsHome, "dot.exe" ) ) )
            {
                throw new IOException( "DotToolsHome invalid. Dot.exe not found" );
            }
        }

        public void Convert( FileInfo dotFile, FileInfo plainFile )
        {
            string arguments;

            RunWithDot(  out arguments, dotFile, plainFile );
            //RunWithSfdp( out arguments, dotFile, plainFile );

            var startInfo = new ProcessStartInfo( "cmd", arguments );
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = Path.GetTempPath();

            var stdErr = new StringWriter();
            var ret = Processes.Execute( startInfo, null, stdErr );

            if( ret != 0 || !plainFile.Exists || dotFile.LastWriteTime > plainFile.LastWriteTime )
            {
                throw new InvalidOperationException( "Dot plain file generation failed: " + stdErr.ToString() );
            }
        }

        private void RunWithDot( out string arguments, FileInfo dotFile, FileInfo plainFile )
        {
            var unflattenExe = Path.Combine( myDotToolsHome, "unflatten.exe" );
            var dotExe = Path.Combine( myDotToolsHome, "dot.exe" );

            arguments = string.Format( "/C \"{0} -l5 -c8 {1} | {2} -Tplain -q -o{3}\"",
                unflattenExe, dotFile.FullName,
                dotExe, plainFile.FullName );
        }

        private void RunWithSfdp( out string arguments, FileInfo dotFile, FileInfo plainFile )
        {
            var exe = Path.Combine( myDotToolsHome, "sfdp.exe" );

            arguments = string.Format( "/C \"{0} -x -Goverlap=scale -Tplain -q -o{1} {2}\"",
                exe, plainFile.FullName, dotFile.FullName );
        }
    }
}
