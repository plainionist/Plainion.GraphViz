using System;
using System.Text;

namespace Plainion.GraphViz.Modules.Documents.DotLang
{
    class MatchWhiteSpace : MatcherBase
    {
        protected override Token IsMatchImpl( Tokenizer tokenizer )
        {
            var str = new StringBuilder();

            while( !tokenizer.EndOfStream() && String.IsNullOrWhiteSpace( tokenizer.Current ) )
            {
                str.Append( tokenizer.Current );

                tokenizer.Consume();
            }

            if( str.Length > 0 )
            {
                return new Token( TokenType.WhiteSpace, str.ToString() );
            }

            return null;
        }
    }
}
