using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Plainion.GraphViz.Modules.Documents.DotLang
{
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
                if( IsEndOfStream( 0 ) )
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

        private bool IsEndOfStream( int lookahead )
        {
            if( Index + lookahead >= myItems.Count )
            {
                return true;
            }

            return false;
        }

        public bool EndOfStream
        {
            get { return IsEndOfStream( 0 ); }
        }

        public virtual T Peek( int lookahead )
        {
            if( IsEndOfStream( lookahead ) )
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

    public class Tokenizer : TokenizableStreamBase<String>
    {
        public Tokenizer( String source )
            : base( () => source.ToCharArray().Select( i => i.ToString( CultureInfo.InvariantCulture ) ).ToList() )
        {
        }
    }
}
