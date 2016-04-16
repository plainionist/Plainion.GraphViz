using System;
using System.Text;

namespace Plainion.GraphViz.Modules.Documents.DotLang
{
    class MatchString : MatcherBase
    {
        public const string QUOTE = "\"";

        private String StringDelim { get; set; }

        public MatchString( String delim )
        {
            StringDelim = delim;
        }

        protected override Token IsMatchImpl( Tokenizer tokenizer )
        {
            var str = new StringBuilder();

            if( tokenizer.Current == StringDelim )
            {
                tokenizer.Consume();

                while( !tokenizer.EndOfStream && tokenizer.Current != StringDelim )
                {
                    str.Append( tokenizer.Current );
                    tokenizer.Consume();
                }

                if( tokenizer.Current == StringDelim )
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
