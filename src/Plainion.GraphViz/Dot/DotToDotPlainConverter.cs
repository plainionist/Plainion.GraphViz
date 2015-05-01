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

        public void Convert( FileInfo dotFile, FileInfo dotPlainFile )
        {
            var unflattenExe = Path.Combine(  myDotToolsHome, "unflatten.exe" );
            var dotExe = Path.Combine( myDotToolsHome, "dot.exe" );

            var args = string.Format( "/C \"{0} -l5 -c8 {1} | {2} -Tplain -q -o{3}\"", 
                unflattenExe, dotFile.FullName, 
                dotExe, dotPlainFile.FullName );

            var startInfo = new ProcessStartInfo( "cmd", args );
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = Path.GetTempPath();

            var stdErr = new StringWriter();
            var ret = Processes.Execute( startInfo, null, stdErr );

            if( ret != 0 || !dotPlainFile.Exists || dotFile.LastWriteTime > dotPlainFile.LastWriteTime )
            {
                throw new InvalidOperationException( "Dot plain file generation failed: " + stdErr.ToString() );
            }
        }
    }
}
