using System;
using System.Text;

namespace Plainion.GraphViz.Modules.Documents.DotLang
{
    class MatchNewLine : MatcherBase
    {
        protected override Token IsMatchImpl( Tokenizer tokenizer )
        {
            var str = new StringBuilder();

            while( !tokenizer.EndOfStream() && ( tokenizer.Current == "\r" || tokenizer.Current == "\n" ) )
            {
                str.Append( tokenizer.Current );

                tokenizer.Consume();
            }

            if( str.ToString() == Environment.NewLine )
            {
                return new Token( TokenType.NewLine );
            }

            return null;
        }
    }
}
