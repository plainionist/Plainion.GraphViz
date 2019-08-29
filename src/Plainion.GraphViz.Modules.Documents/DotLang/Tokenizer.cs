using System;
using System.Collections.Generic;
using System.Globalization;

namespace Plainion.GraphViz.Modules.Documents.DotLang
{
    class TokenizableStreamBase<T>
    {
        private T[] myItems;
        private Stack<int> mySnapshotIndexes;

        public TokenizableStreamBase( Func<T[]> extractor )
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
                if( Index >= myItems.Length )
                {
                    return default( T );
                }

                return myItems[ Index ];
            }
        }

        public void Consume()
        {
            Index++;
        }

        public bool EndOfStream
        {
            get { return Index >= myItems.Length; }
        }

        public virtual T Peek( int lookahead )
        {
            if( Index + lookahead >= myItems.Length )
            {
                return default( T );
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

    class Tokenizer : TokenizableStreamBase<char>
    {
        public Tokenizer( string source )
            : base( () => source.ToCharArray() )
        {
        }
    }
}
