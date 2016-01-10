using System.Collections.Generic;
using System.Linq;

namespace Plainion.GraphViz.Modules.Documents.DotLang
{
    class Iterator
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
}
