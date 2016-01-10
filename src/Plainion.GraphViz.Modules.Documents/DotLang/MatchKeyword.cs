using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Plainion.GraphViz.Modules.Documents.DotLang
{
    class MatchKeyword : MatcherBase
    {
        public string Match { get; set; }

        private TokenType TokenType { get; set; }

        /// <summary>
        /// If true then matching on { in a string like "{test" will match the first cahracter
        /// because it is not space delimited. If false it must be space or special character delimited
        /// </summary>
        public bool AllowAsSubString { get; set; }

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
}
