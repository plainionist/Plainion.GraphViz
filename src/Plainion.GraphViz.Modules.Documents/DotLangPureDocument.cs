using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Plainion.GraphViz.Modules.Documents
{
    // http://www.graphviz.org/doc/info/lang.html
    // inspired by: https://github.com/devshorts/LanguageCreator
    public class DotLangPureDocument : AbstractGraphDocument
    {
        protected override void Load()
        {
            using( var reader = new StreamReader( Filename ) )
            {
                Read( reader );
            }
        }

        internal void Read( TextReader reader )
        {
            var lexer = new Lexer( reader.ReadToEnd() );

            var tokens = lexer.Lex().ToList();
            for( int i = 0; i < tokens.Count; ++i )
            {
                var token = tokens[ i ];

                if( token.TokenType == TokenType.WhiteSpace )
                {
                    continue;
                }

                if( token.TokenType == TokenType.Graph || token.TokenType == TokenType.Strict || token.TokenType == TokenType.DirectedGraph )
                {
                    continue;
                }

                if( token.TokenType == TokenType.Node || token.TokenType == TokenType.Edge )
                {
                    continue;
                }

                if( token.TokenType == TokenType.GraphBegin || token.TokenType == TokenType.GraphEnd )
                {
                    continue;
                }

                if( token.TokenType == TokenType.CommentBegin )
                {
                    while( token.TokenType != TokenType.CommentEnd )
                    {
                        i++;
                        token = tokens[ i ];
                    }
                    continue;
                }

                if( token.TokenType == TokenType.SingleLineComment )
                {
                    ReadToEndOfLine( tokens, ref i, token );
                    continue;
                }

                if( i + 1 < tokens.Count && tokens[ i + 1 ].TokenType == TokenType.Assignment )
                {
                    ReadToEndOfLine( tokens, ref i, token );
                    continue;
                }

                if( i + 1 < tokens.Count
                    && ( tokens[ i + 1 ].TokenType == TokenType.SemiColon || tokens[ i + 1 ].TokenType == TokenType.NewLine ) )
                {
                    TryAddNode( token.TokenValue );
                    continue;
                }

                if( i + 2 < tokens.Count && tokens[ i + 1 ].TokenType == TokenType.EdgeDef )
                {
                    TryAddEdge( token.TokenValue, tokens[ i + 2 ].TokenValue );
                    i += 2;
                    continue;
                }
            }
        }

        private static void ReadToEndOfLine( List<Token> tokens, ref int i, Token token )
        {
            while( token.TokenType != TokenType.NewLine )
            {
                i++;
                token = tokens[ i ];
            }
        }

        #region lexer

        public class Lexer
        {
            private Tokenizer myTokenizer;
            private List<IMatcher> myMatchers;

            public Lexer( String source )
            {
                myTokenizer = new Tokenizer( source );
            }

            public IEnumerable<Token> Lex()
            {
                myMatchers = InitializeMatchList();

                var current = Next();

                while( current != null && current.TokenType != TokenType.EndOfStream )
                {
                    if( current.TokenType != TokenType.WhiteSpace )
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

        public class Tokenizer : TokenizableStreamBase<String>
        {
            public Tokenizer( String source )
                : base( () => source.ToCharArray().Select( i => i.ToString( CultureInfo.InvariantCulture ) ).ToList() )
            {
            }
        }

        public class TokenizableStreamBase<T> where T : class
        {
            private List<T> myItems;
            private Stack<int> mySnapshotIndexes;

            public TokenizableStreamBase( Func<List<T>> extractor )
            {
                Index = 0;

                myItems = extractor();

                mySnapshotIndexes = new Stack<int>();
            }

            protected int Index { get; set; }

            public virtual T Current
            {
                get
                {
                    if( EndOfStream( 0 ) )
                    {
                        return null;
                    }

                    return myItems[ Index ];
                }
            }

            public void Consume()
            {
                Index++;
            }

            private bool EndOfStream( int lookahead )
            {
                if( Index + lookahead >= myItems.Count )
                {
                    return true;
                }

                return false;
            }

            public Boolean EndOfStream()
            {
                return EndOfStream( 0 );
            }

            public virtual T Peek( int lookahead )
            {
                if( EndOfStream( lookahead ) )
                {
                    return null;
                }

                return myItems[ Index + lookahead ];
            }

            public void TakeSnapshot()
            {
                mySnapshotIndexes.Push( Index );
            }

            public void RollbackSnapshot()
            {
                Index = mySnapshotIndexes.Pop();
            }

            public void CommitSnapshot()
            {
                mySnapshotIndexes.Pop();
            }
        }

        public class Token
        {
            public TokenType TokenType { get; private set; }

            public string TokenValue { get; private set; }

            public Token( TokenType tokenType, string token )
            {
                TokenType = tokenType;
                TokenValue = token;
            }

            public Token( TokenType tokenType )
            {
                TokenValue = null;
                TokenType = tokenType;
            }

            public override string ToString()
            {
                return TokenType + ": " + TokenValue;
            }
        }

        public enum TokenType
        {
            Edge,
            Graph,
            WhiteSpace,
            GraphBegin,
            GraphEnd,
            QuotedString,
            Word,
            Comma,
            Assignment,
            CommentBegin,
            CommentEnd,
            EdgeDef,
            Strict,
            SemiColon,
            Node,
            Subgraph,
            EndOfStream,
            DirectedGraph,
            AttributeBegin,
            AttributeEnd,
            Number,
            SingleLineComment,
            NewLine
        }

        public interface IMatcher
        {
            Token IsMatch( Tokenizer tokenizer );
        }

        public abstract class MatcherBase : IMatcher
        {
            public Token IsMatch( Tokenizer tokenizer )
            {
                if( tokenizer.EndOfStream() )
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

        public class MatchWord : MatcherBase
        {
            private List<MatchKeyword> SpecialCharacters { get; set; }
            public MatchWord( IEnumerable<IMatcher> keywordMatchers )
            {
                SpecialCharacters = keywordMatchers.Select( i => i as MatchKeyword ).Where( i => i != null ).ToList();
            }

            protected override Token IsMatchImpl( Tokenizer tokenizer )
            {
                String current = null;

                while( !tokenizer.EndOfStream() && !String.IsNullOrWhiteSpace( tokenizer.Current ) && SpecialCharacters.All( m => m.Match != tokenizer.Current ) )
                {
                    current += tokenizer.Current;
                    tokenizer.Consume();
                }

                if( current == null )
                {
                    return null;
                }

                // can't start a word with a special character
                if( SpecialCharacters.Any( c => current.StartsWith( c.Match ) ) )
                {
                    throw new Exception( String.Format( "Cannot start a word with a special character {0}", current ) );
                }

                return new Token( TokenType.Word, current );
            }
        }

        public class MatchString : MatcherBase
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

                    while( !tokenizer.EndOfStream() && tokenizer.Current != StringDelim )
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

        public class MatchKeyword : MatcherBase
        {
            public string Match { get; set; }

            private TokenType TokenType { get; set; }


            /// <summary>
            /// If true then matching on { in a string like "{test" will match the first cahracter
            /// because it is not space delimited. If false it must be space or special character delimited
            /// </summary>
            public Boolean AllowAsSubString { get; set; }

            public List<MatchKeyword> SpecialCharacters { get; set; }

            public MatchKeyword( TokenType type, String match )
            {
                Match = match;
                TokenType = type;
                AllowAsSubString = true;
            }

            protected override Token IsMatchImpl( Tokenizer tokenizer )
            {
                foreach( var character in Match )
                {
                    if( tokenizer.Current == character.ToString( CultureInfo.InvariantCulture ) )
                    {
                        tokenizer.Consume();
                    }
                    else
                    {
                        return null;
                    }
                }

                bool found;

                if( !AllowAsSubString )
                {
                    var next = tokenizer.Current;

                    found = String.IsNullOrWhiteSpace( next ) || SpecialCharacters.Any( character => character.Match == next );
                }
                else
                {
                    found = true;
                }

                if( found )
                {
                    return new Token( TokenType, Match );
                }

                return null;
            }
        }

        public class MatchNumber : MatcherBase
        {
            protected override Token IsMatchImpl( Tokenizer tokenizer )
            {

                var leftOperand = GetIntegers( tokenizer );

                if( leftOperand != null )
                {
                    if( tokenizer.Current == "." )
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
                var regex = new Regex( "[0-9]" );

                String num = null;

                while( tokenizer.Current != null && regex.IsMatch( tokenizer.Current ) )
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
        #endregion
    }
}
