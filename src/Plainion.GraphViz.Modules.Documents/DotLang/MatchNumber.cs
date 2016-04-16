using System;
using System.Text.RegularExpressions;

namespace Plainion.GraphViz.Modules.Documents.DotLang
{
    class MatchNumber : MatcherBase
    {
        protected override Token IsMatchImpl( Tokenizer tokenizer )
        {
            var leftOperand = GetIntegers( tokenizer );

            if( leftOperand != null )
            {
                if( tokenizer.Current == '.' )
                {
                    tokenizer.Consume();

                    var rightOperand = GetIntegers( tokenizer );

                    // found a float
                    if( rightOperand != null )
                    {
                        return new Token( TokenType.Number, leftOperand + "." + rightOperand );
                    }
                }

                return new Token( TokenType.Number, leftOperand );
            }

            return null;
        }

        private String GetIntegers( Tokenizer tokenizer )
        {
            string num = null;

            while( char.IsDigit( tokenizer.Current ) )
            {
                num += tokenizer.Current;
                tokenizer.Consume();
            }

            if( num != null )
            {
                return num;
            }

            return null;
        }
    }
}
