using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Modules.CodeInspection.Tests.TestData
{
    class LinqAnonymousType
    {
        public IEnumerable<IGraphItem> Find()
        {
            var items = GetActiveViews<IGraphItem>();

            return items
                .Select(item => new
                {
                    Value = item,
                    Node = item as Node
                })
                .Where(x => x.Node != null)
                .Select(x => x.Value)
                .ToList();
        }

        private IEnumerable<T> GetActiveViews<T>() where T : class, IGraphItem
        {
            return new T[] { 
                new Node("1") as T, 
                new Edge(new Node("2"), new Node("3")) as T};
        }
    }
}
