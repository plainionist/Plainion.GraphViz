using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Documents.DotLang
{
    class Parser
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
                // TODO: this is not very optimal - couldnt we do it with enumerator as well?
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
}
