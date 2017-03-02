using System;
using System.Collections.Generic;
using Plainion.GraphViz.Dot;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Documents.DotLang
{
    class Parser
    {
        private class Subgraph
        {
            private HashSet<string> myNodes = new HashSet<string>();

            public string Name { get; set; }

            public HashSet<string> Nodes { get { return myNodes; } }
        }

        private Iterator myIterator;
        private DotLangDocument myDocument;
        private Subgraph myCurrentSubGraph;

        public Parser(Lexer lexer, DotLangDocument document)
        {
            myIterator = new Iterator(lexer);
            myDocument = document;
        }

        public void Parse()
        {
            while (myIterator.MoveNext())
            {
                if (myIterator.Current.Type == TokenType.Graph || myIterator.Current.Type == TokenType.Strict || myIterator.Current.Type == TokenType.DirectedGraph)
                {
                    continue;
                }

                if (myIterator.Current.Type == TokenType.Node || myIterator.Current.Type == TokenType.Edge)
                {
                    continue;
                }

                if (myIterator.Current.Type == TokenType.GraphBegin)
                {
                    continue;
                }

                if (myIterator.Current.Type == TokenType.GraphEnd)
                {
                    if (myCurrentSubGraph != null)
                    {
                        myDocument.TryAddCluster(myCurrentSubGraph.Name, myCurrentSubGraph.Nodes);
                        myCurrentSubGraph = null;
                    }

                    continue;
                }

                if (myIterator.Current.Type == TokenType.CommentBegin)
                {
                    while (myIterator.Current.Type != TokenType.CommentEnd && myIterator.MoveNext()) ;
                    continue;
                }

                if (myIterator.Current.Type == TokenType.SingleLineComment)
                {
                    while (myIterator.Current.Type != TokenType.NewLine && myIterator.MoveNext()) ;
                    continue;
                }

                if (myIterator.Current.Type == TokenType.Subgraph)
                {
                    myCurrentSubGraph = new Subgraph();
                    myIterator.MoveNext();
                    myCurrentSubGraph.Name = myIterator.Current.Value;
                    continue;
                }

                if (myIterator.IsNext(TokenType.Assignment))
                {
                    if (myCurrentSubGraph != null && myIterator.Current.Value.Equals("label", StringComparison.OrdinalIgnoreCase))
                    {
                        // assignment
                        myIterator.MoveNext();

                        myIterator.MoveNext();
                        var value = myIterator.Current.Value;

                        myDocument.Add(new Caption(myCurrentSubGraph.Name, value));
                    }

                    // end of statement
                    while (!(myIterator.Current.Type == TokenType.NewLine || myIterator.Current.Type == TokenType.SemiColon)
                        && myIterator.MoveNext()) ;
                    continue;
                }

                if (IsNodeDefinition())
                {
                    var node = myDocument.TryAddNode(myIterator.Current.Value);

                    // we ignore duplicates
                    if (node != null)
                    {
                        if (myCurrentSubGraph != null)
                        {
                            myCurrentSubGraph.Nodes.Add(node.Id);
                        }

                        TryReadAttributes(node);
                    }
                    else
                    {
                        while ((myIterator.Current.Type != TokenType.SemiColon && myIterator.Current.Type != TokenType.NewLine) && myIterator.MoveNext()) ;
                    }

                    continue;
                }

                if (myIterator.IsNext(TokenType.EdgeDef))
                {
                    var source = myIterator.Current;
                    myIterator.MoveNext();
                    myIterator.MoveNext();
                    var target = myIterator.Current;

                    var edge = myDocument.TryAddEdge(source.Value, target.Value);

                    if (myCurrentSubGraph != null)
                    {
                        myCurrentSubGraph.Nodes.Add(source.Value);
                        myCurrentSubGraph.Nodes.Add(target.Value);
                    }

                    TryReadAttributes(edge);

                    continue;
                }

                if (myIterator.Current.Type == TokenType.SemiColon || myIterator.Current.Type == TokenType.NewLine)
                {
                    continue;
                }

                throw new NotImplementedException("Unsupported node type: " + myIterator.Current.Type);
            }
        }

        private void TryReadAttributes<T>(T owner) where T : IGraphItem
        {
            if (!myIterator.IsNext(TokenType.AttributeBegin))
            {
                return;
            }

            myIterator.MoveNext();

            while (myIterator.Current.Type != TokenType.AttributeEnd)
            {
                myIterator.MoveNext();
                var key = myIterator.Current.Value;

                // assignment
                myIterator.MoveNext();

                myIterator.MoveNext();
                var value = myIterator.Current.Value;

                if (key.Equals("label", StringComparison.OrdinalIgnoreCase))
                {
                    myDocument.Add(new Caption(owner.Id, value));
                }

                if (key.Equals("color", StringComparison.OrdinalIgnoreCase))
                {
                    if (owner is Node)
                    {
                        myDocument.Add(new NodeStyle(owner.Id) { FillColor = StyleConverter.GetBrush(value) });
                    }
                    else if (owner is Edge)
                    {
                        myDocument.Add(new EdgeStyle(owner.Id) { Color = StyleConverter.GetBrush(value) });
                    }
                }

                // either colon or end
                myIterator.MoveNext();
            }
        }

        private bool IsNodeDefinition()
        {
            return (myIterator.Current.Type == TokenType.Word || myIterator.Current.Type == TokenType.QuotedString)
                && (!myIterator.IsNext(TokenType.EdgeDef));
        }
    }
}
