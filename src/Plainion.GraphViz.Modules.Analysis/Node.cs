using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.Mvvm;
using Plainion.Windows.Controls.Tree;

namespace Plainion.GraphViz.Modules.Analysis
{
    class Node : BindableBase, INode
    {
        private string myId;
        private string myName;

        public Node()
        {
            Children = new ObservableCollection<Node>();
        }

        public string Id
        {
            get { return myId; }
            set { SetProperty( ref myId, value ); }
        }

        public string Name
        {
            get { return myName; }
            set { SetProperty( ref myName, value ); }
        }

        IEnumerable<INode> INode.Children
        {
            get { return Children; }
        }

        public ObservableCollection<Node> Children { get; private set; }

        public INode Parent { get; set; }

        bool INode.Matches( string pattern )
        {
            if( pattern == "*" )
            {
                return Name != null;
            }

            return ( Name != null && Name.Contains( pattern, StringComparison.OrdinalIgnoreCase ) )
                || ( Id != null && Id.Contains( pattern, StringComparison.OrdinalIgnoreCase ) );
        }
    }
}
