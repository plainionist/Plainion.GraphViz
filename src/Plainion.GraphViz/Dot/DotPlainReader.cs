using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Plainion.GraphViz.Dot
{
    // http://www.graphviz.org/doc/info/output.html#d:plain
    public class DotPlainReader : IDisposable
    {
        private TextReader myReader;
        private int myPosInLine;
        private bool myLineMatchedTag;
        private readonly StringBuilder myReadBuffer;

        public DotPlainReader(TextReader reader)
        {
            Contract.RequiresNotNull(reader, "reader");

            myReader = reader;
            myReadBuffer = new StringBuilder();
            myLineMatchedTag = true;
        }

        public string CurrentLine { get; private set; }

        public bool ReadLine(string tag)
        {
            if (myLineMatchedTag)
            {
                CurrentLine = myReader.ReadLine();

                while (CurrentLine.Length > 0 && CurrentLine[CurrentLine.Length - 1] == '\\')
                {
                    CurrentLine = CurrentLine.TrimEnd('\\') + myReader.ReadLine();
                }

                myLineMatchedTag = false;
            }

            if (CurrentLine != null && CurrentLine.StartsWith(tag))
            {
                myPosInLine = tag.Length;
                myLineMatchedTag = true;
            }

            return myLineMatchedTag;
        }

        public bool CanReadString()
        {
            while (CurrentLine[myPosInLine] == ' ') ++myPosInLine;
            return CurrentLine[myPosInLine] == '"';
        }

        public string ReadString()
        {
            while (CurrentLine[myPosInLine] == ' ') ++myPosInLine;

            myReadBuffer.Length = 0;

            if (CurrentLine[myPosInLine] == '"') // Quoted
            {
                ++myPosInLine;
                while (myPosInLine < CurrentLine.Length && CurrentLine[myPosInLine] != '"')
                {
                    myReadBuffer.Append(CurrentLine[myPosInLine]);
                    ++myPosInLine;
                }
                ++myPosInLine;
            }
            else
            {
                while (myPosInLine < CurrentLine.Length && CurrentLine[myPosInLine] != ' ')
                {
                    myReadBuffer.Append(CurrentLine[myPosInLine]);
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
                return double.Parse(str, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse line: " + CurrentLine, ex);
            }
        }

        public int ReadInt()
        {
            try
            {
                var str = ReadString();
                return int.Parse(str, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse line: " + CurrentLine, ex);
            }
        }

        public void Dispose()
        {
            if (myReader != null)
            {
                myReader.Close();
                myReader = null;
            }
        }
    }
}
