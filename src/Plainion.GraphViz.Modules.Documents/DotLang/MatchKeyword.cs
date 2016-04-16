using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Plainion.GraphViz.Modules.Documents.DotLang
{
    class MatchKeyword : MatcherBase
    {
        private TokenType myTokenType;

        public string Match { get; private set; }

        /// <summary>
        /// If true then matching on { in a string like "{test" will match the first cahracter
        /// because it is not space delimited. If false it must be space or special character delimited
        /// </summary>
        public bool AllowAsSubString { get; set; }

        public List<MatchKeyword> SpecialCharacters { get; set; }

        public MatchKeyword( TokenType type, string match )
        {
            Match = match;
            myTokenType = type;
            AllowAsSubString = true;
        }

        protected override Token IsMatchImpl( Tokenizer tokenizer )
        {
            foreach( var character in Match )
            {
                if( tokenizer.Current == character )
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

                found = char.IsWhiteSpace( next ) || SpecialCharacters.Any( character => character.Match.Length == 1 && character.Match[ 0 ] == next );
            }
            else
            {
                found = true;
            }

            if( found )
            {
                return new Token( myTokenType, Match );
            }

            return null;
        }
    }
}
