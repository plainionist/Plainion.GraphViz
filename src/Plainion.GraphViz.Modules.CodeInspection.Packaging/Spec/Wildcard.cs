﻿using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec
{
    public abstract class Wildcard
    {
        private string myPatternValue;
        private Lazy<FastWildcard> myPattern;

        private class FastWildcard
        {
            private readonly GraphViz.CodeInspection.Wildcard myWildcard;
            private readonly string mySubstring;

            public FastWildcard(string pattern)
            {
                if (pattern.Contains('*'))
                {
                    myWildcard = new GraphViz.CodeInspection.Wildcard(pattern, RegexOptions.IgnoreCase);
                }
                else
                {
                    mySubstring = pattern;
                }
            }

            public bool IsMatch(string str)
            {
                return myWildcard != null ? myWildcard.IsMatch(str) : str.Contains(mySubstring, StringComparison.OrdinalIgnoreCase);
            }
        }

        public string Pattern
        {
            get { return myPatternValue; }
            set
            {
                if (myPatternValue == value)
                {
                    return;
                }

                myPatternValue = value;

                myPattern = new Lazy<FastWildcard>(() => new FastWildcard(myPatternValue));
            }
        }

        internal bool Matches(string file)
        {
            return myPattern.Value.IsMatch(file);
        }

        [DefaultValue(null)]
        public string Comment { get; set; }
    }
}
