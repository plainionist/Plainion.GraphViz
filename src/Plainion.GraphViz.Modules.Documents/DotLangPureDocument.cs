using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Documents
{
    // http://www.graphviz.org/doc/info/lang.html
    // inspired by: https://github.com/devshorts/LanguageCreator
    public class DotLangPureDocument : AbstractGraphDocument, ICaptionDocument
    {
        private List<Caption> myCaptions;

        public IEnumerable<Caption> Captions
        {
            get { return myCaptions; }
        }

        protected override void Load()
        {
            using( var reader = new StreamReader( Filename ) )
            {
                Read( reader );
            }
        }

        protected internal void Add( Caption caption )
        {
            myCaptions.Add( caption );
        }

        internal void Read( TextReader reader )
        {
            myCaptions = new List<Caption>();
            
            var lexer = new Lexer( reader.ReadToEnd() );
            var parser = new Parser( lexer, this );
            parser.Parse();
        }

        private class Parser
        {
            private Iterator myIterator;
            private DotLangPureDocument myDocument;

            public Parser( Lexer lexer, DotLangPureDocument document )
            {
                myIterator = new Iterator( lexer );
                myDocument = document;
            }

            private class Iterator
            {
                private Lexer myLexer;
                private IList<Token> myTokens;
                private int myCurrent;

                public Iterator( Lexer lexer )
                {
                    myLexer = lexer;
                    myTokens = myLexer.Lex().ToList();
                    myCurrent = -1;
                }

                public Token Current
                {
                    get { return myTokens[ myCurrent ]; }
                }

                public Token Next
                {
                    get { return myCurrent + 1 < myTokens.Count ? myTokens[ myCurrent + 1 ] : null; }
                }

                public bool IsNext( TokenType tokenType )
                {
                    return Next != null && Next.Type == tokenType;
                }

                public bool MoveNext()
                {
                    myCurrent++;
                    return myCurrent < myTokens.Count;
                }
            }

            public void Parse()
            {
                while( myIterator.MoveNext() )
                {
                    if( myIterator.Current.Type == TokenType.Graph || myIterator.Current.Type == TokenType.Strict || myIterator.Current.Type == TokenType.DirectedGraph )
                    {
                        continue;
                    }

                    if( myIterator.Current.Type == TokenType.Node || myIterator.Current.Type == TokenType.Edge )
                    {
                        continue;
                    }

                    if( myIterator.Current.Type == TokenType.GraphBegin || myIterator.Current.Type == TokenType.GraphEnd )
                    {
                        continue;
                    }

                    if( myIterator.Current.Type == TokenType.CommentBegin )
                    {
                        while( myIterator.Current.Type != TokenType.CommentEnd && myIterator.MoveNext() ) ;
                        continue;
                    }

                    if( myIterator.Current.Type == TokenType.SingleLineComment )
                    {
                        while( myIterator.Current.Type != TokenType.NewLine && myIterator.MoveNext() ) ;
                        continue;
                    }

                    if( myIterator.IsNext( TokenType.Assignment ) )
                    {
                        // end of statement
                        while( !( myIterator.Current.Type == TokenType.NewLine || myIterator.Current.Type == TokenType.SemiColon )
                            && myIterator.MoveNext() ) ;
                        continue;
                    }

                    if( IsNodeDefinition() )
                    {
                        var node = myDocument.TryAddNode( myIterator.Current.Value );

                        TryReadAttributes( node.Id );
                        continue;
                    }

                    if( myIterator.IsNext( TokenType.EdgeDef ) )
                    {
                        var source = myIterator.Current;
                        myIterator.MoveNext();
                        myIterator.MoveNext();
                        var target = myIterator.Current;
                        var edge = myDocument.TryAddEdge( source.Value, target.Value );

                        TryReadAttributes( edge.Id );
                        
                        continue;
                    }
                }
            }

            private void TryReadAttributes( string ownerId )
            {
                if( !myIterator.IsNext( TokenType.AttributeBegin ) )
                {
                    return;
                }

                myIterator.MoveNext();
                
                while( myIterator.Current.Type != TokenType.AttributeEnd )
                {
                    myIterator.MoveNext();
                    var key = myIterator.Current.Value;

                    // assignment
                    myIterator.MoveNext();
                    
                    myIterator.MoveNext();
                    var value = myIterator.Current.Value;

                    if( key.Equals( "label", StringComparison.OrdinalIgnoreCase ) )
                    {
                        myDocument.Add( new Caption( ownerId, value ) );
                    }

                    // either colon or end
                    myIterator.MoveNext();
                }
            }

            private bool IsNodeDefinition()
            {
                return ( myIterator.Current.Type == TokenType.Word || myIterator.Current.Type == TokenType.QuotedString )
                    && ( !myIterator.IsNext( TokenType.EdgeDef ) );
            }
        }

        #region lexer

        public class Lexer
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
            public TokenType Type { get; private set; }

            public string Value { get; private set; }

            public Token( TokenType tokenType, string token )
            {
                Type = tokenType;
                Value = token;
            }

            public Token( TokenType tokenType )
            {
                Value = null;
                Type = tokenType;
            }

            public override string ToString()
            {
                return Type + ": " + Value;
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
