using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.Mvvm;
using Plainion.GraphViz.Algorithms;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;
using Plainion.Windows.Controls.Tree;

namespace Plainion.GraphViz.Modules.Analysis
{
    class ClusterTreeNode : BindableBase, INode
    {
        private IGraphPresentation myPresentation;
        private Node myModel;
        private INode myParent;
        private bool myIsExpanded;

        public ClusterTreeNode( IGraphPresentation presentation, Node model )
        {
            myPresentation = presentation;
            myModel = model;

            Children = new ObservableCollection<ClusterTreeNode>();
        }

        public string Id { get; set; }

        public string Caption { get; set; }

        public bool IsExpanded
        {
            get { return myIsExpanded; }
            set { SetProperty( ref myIsExpanded, value ); }
        }

        IEnumerable<INode> INode.Children
        {
            get { return Children; }
        }

        public ObservableCollection<ClusterTreeNode> Children { get; private set; }

        public INode Parent
        {
            get { return myParent; }
            set
            {
                if( SetProperty( ref myParent, value ) && myModel != null )
                {
                    // i am a node (not a cluster)
                    new ChangeClusterAssignment( myPresentation )
                        .Execute( t => t.AddToCluster( myModel.Id, ( ( ClusterTreeNode )myParent ).Id ) );
                }
            }
        }

        bool INode.Matches( string pattern )
        {
            if( pattern == "*" )
            {
                return Caption != null;
            }

            return ( Caption != null && Caption.Contains( pattern, StringComparison.OrdinalIgnoreCase ) )
                || ( Id != null && Id.Contains( pattern, StringComparison.OrdinalIgnoreCase ) );
        }
    }
}
