using System;
using System.Text;

namespace Plainion.GraphViz.Modules.Documents.DotLang
{
    class MatchString : MatcherBase
    {
        public const char QUOTE = '"';

        private char myStringDelim;

        public MatchString( char delim )
        {
            myStringDelim = delim;
        }

        protected override Token IsMatchImpl( Tokenizer tokenizer )
        {
            var str = new StringBuilder();

            if( tokenizer.Current == myStringDelim )
            {
                tokenizer.Consume();

                while( !tokenizer.EndOfStream && tokenizer.Current != myStringDelim )
                {
                    str.Append( tokenizer.Current );
                    tokenizer.Consume();
                }

                if( tokenizer.Current == myStringDelim )
                {
                    tokenizer.Consume();
                }
            }

            if( str.Length > 0 )
            {
                return new Token( TokenType.QuotedString, str.ToString() );
            }

            return null;
        }
    }
}
