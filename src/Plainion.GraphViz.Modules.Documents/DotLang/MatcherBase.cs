
namespace Plainion.GraphViz.Modules.Documents.DotLang
{
    abstract class MatcherBase : IMatcher
    {
        public Token IsMatch( Tokenizer tokenizer )
        {
            if( tokenizer.EndOfStream )
            {
                return new Token( TokenType.EndOfStream );
            }

            tokenizer.TakeSnapshot();

            var match = IsMatchImpl( tokenizer );

            if( match == null )
            {
                tokenizer.RollbackSnapshot();
            }
            else
            {
                tokenizer.CommitSnapshot();
            }

            return match;
        }

        protected abstract Token IsMatchImpl( Tokenizer tokenizer );
    }
}
