using System;
using System.Globalization;
using System.IO;
using System.Text;
using Plainion;

namespace Plainion.GraphViz.Dot
{
    // http://www.graphviz.org/doc/info/output.html#d:plain
    public class DotPlainReader : IDisposable
    {
        private TextReader myReader;
        private string myCurrentLine;
        private int myPosInLine;
        private bool myLineMatchedTag;
        private StringBuilder myReadBuffer = new StringBuilder();

        public DotPlainReader( TextReader reader )
        {
            Contract.RequiresNotNull( reader, "reader" );

            myReader = reader;
            myLineMatchedTag = true;
        }

        public bool ReadLine( string tag )
        {
            if( myLineMatchedTag )
            {
                myCurrentLine = myReader.ReadLine();

                if (myCurrentLine.Length > 0 && myCurrentLine[myCurrentLine.Length - 1] == '\\')
                {
                    myCurrentLine = myCurrentLine.TrimEnd('\\') + myReader.ReadLine();
                }

                myLineMatchedTag = false;
            }

            if( myCurrentLine != null && myCurrentLine.StartsWith( tag ) )
            {
                myPosInLine = tag.Length;
                myLineMatchedTag = true;
            }

            return myLineMatchedTag;
        }

        public string ReadString()
        {
            while( myCurrentLine[ myPosInLine ] == ' ' ) ++myPosInLine;
            myReadBuffer.Length = 0;
            if( myCurrentLine[ myPosInLine ] == '"' ) // Quoted
            {
                ++myPosInLine;
                while( myPosInLine < myCurrentLine.Length && myCurrentLine[ myPosInLine ] != '"' )
                {
                    myReadBuffer.Append( myCurrentLine[ myPosInLine ] );
                    ++myPosInLine;
                }
                ++myPosInLine;
            }
            else
            {
                while( myPosInLine < myCurrentLine.Length && myCurrentLine[ myPosInLine ] != ' ' )
                {
                    myReadBuffer.Append( myCurrentLine[ myPosInLine ] );
                    ++myPosInLine;
                }
            }
            return myReadBuffer.ToString();
        }

        public double ReadDouble()
        {
            try
            {
                var str = ReadString();
                return double.Parse( str, CultureInfo.InvariantCulture );
            }
            catch( Exception ex )
            {
                throw new Exception( "Failed to parse line: " + myCurrentLine, ex );
            }
        }

        public int ReadInt()
        {
            try
            {
                var str = ReadString();
                return int.Parse( str, CultureInfo.InvariantCulture );
            }
            catch( Exception ex )
            {
                throw new Exception( "Failed to parse line: " + myCurrentLine, ex );
            }
        }

        internal void Close()
        {
            myReader.Close();
        }

        public void Dispose()
        {
            if( myReader != null )
            {
                myReader.Close();
                myReader = null;
            }
        }
    }
}
