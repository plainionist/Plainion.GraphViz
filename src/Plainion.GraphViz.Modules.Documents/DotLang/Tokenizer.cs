using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Plainion.GraphViz.Modules.Documents.DotLang
{
    public class TokenizableStreamBase<T>
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

    public class Tokenizer : TokenizableStreamBase<String>
    {
        public Tokenizer( String source )
            : base( () => source.ToCharArray().Select( i => i.ToString( CultureInfo.InvariantCulture ) ).ToArray() )
        {
        }
    }
}
