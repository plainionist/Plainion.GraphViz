using System;
using System.Text.RegularExpressions;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Viewer.Configuration
{
    class RegexConversion : ILabelConversionStep
    {
        public string Matching
        {
            get;
            set;
        }

        public string Replacement
        {
            get;
            set;
        }

        public bool IsInitialized
        {
            get
            {
                return !string.IsNullOrEmpty( Matching );
            }
        }

        public string Convert( string input )
        {
            if( !IsInitialized )
            {
                throw new InvalidOperationException( "Not properly configured" );
            }

            return Regex.Replace( input, Matching, Replacement );
        }

    }
}
