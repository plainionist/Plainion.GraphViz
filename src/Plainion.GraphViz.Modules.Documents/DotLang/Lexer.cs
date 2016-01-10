using System.Collections.Generic;
using System.Linq;

namespace Plainion.GraphViz.Modules.Documents.DotLang
{
    class Lexer
    {
        private Tokenizer myTokenizer;
        private List<IMatcher> myMatchers;

        public Lexer( string source )
        {
            myTokenizer = new Tokenizer( source );
            myMatchers = InitializeMatchList();
        }

        public IEnumerable<Token> Lex()
        {
            var current = Next();

            while( current != null && current.Type != TokenType.EndOfStream )
            {
                if( current.Type != TokenType.WhiteSpace )
                {
                    yield return current;
                }

                current = Next();
            }
        }

        // the order here matters because it defines token precedence
        private List<IMatcher> InitializeMatchList()
        {
            var keywordmatchers = new List<IMatcher> 
                                  {
                                      new MatchKeyword(TokenType.Graph, "graph"),
                                      new MatchKeyword(TokenType.DirectedGraph, "digraph"),
                                      new MatchKeyword(TokenType.Strict, "strict"),
                                      new MatchKeyword(TokenType.Node, "node"),
                                      new MatchKeyword(TokenType.Edge, "edge"),
                                      new MatchKeyword(TokenType.Subgraph, "subgraph"),
                                  };


            var specialCharacters = new List<IMatcher>
                                    {
                                        new MatchKeyword(TokenType.EdgeDef, "->"),
                                        new MatchKeyword(TokenType.GraphBegin, "{"),
                                        new MatchKeyword(TokenType.GraphEnd, "}"),
                                        new MatchKeyword(TokenType.AttributeBegin, "["),
                                        new MatchKeyword(TokenType.AttributeEnd, "]"),
                                        new MatchKeyword(TokenType.Assignment, "="),
                                        new MatchKeyword(TokenType.Comma, ","),
                                        new MatchKeyword(TokenType.SemiColon, ";"),
                                        new MatchKeyword(TokenType.CommentBegin, "/*"),
                                        new MatchKeyword(TokenType.CommentEnd, "*/"),
                                        new MatchKeyword(TokenType.SingleLineComment, "//"),
                                    };

            // give each keyword the list of possible delimiters and not allow them to be 
            // substrings of other words, i.e. token fun should not be found in string "function"
            keywordmatchers.ForEach( keyword =>
            {
                var current = ( keyword as MatchKeyword );
                current.AllowAsSubString = false;
                current.SpecialCharacters = specialCharacters.Select( i => i as MatchKeyword ).ToList();
            } );

            var matchers = new List<IMatcher>();
            matchers.Add( new MatchString( MatchString.QUOTE ) );
            matchers.AddRange( specialCharacters );
            matchers.AddRange( keywordmatchers );
            matchers.Add( new MatchNewLine() );
            matchers.Add( new MatchWhiteSpace() );
            matchers.Add( new MatchNumber() );
            matchers.Add( new MatchWord( specialCharacters ) );

            return matchers;
        }

        private Token Next()
        {
            if( myTokenizer.EndOfStream() )
            {
                return new Token( TokenType.EndOfStream );
            }

            return myMatchers
                .Select( match => match.IsMatch( myTokenizer ) )
                .FirstOrDefault( token => token != null );
        }
    }
}
