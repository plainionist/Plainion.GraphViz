﻿using System;
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
        private INode myParent;
        private bool myIsExpanded;
        private bool myIsSelected;

        public ClusterTreeNode()
        {
            Children = new ObservableCollection<ClusterTreeNode>();
        }

        public string Id { get; set; }

        public string Caption { get; set; }

        public bool IsExpanded
        {
            get { return myIsExpanded; }
            set { SetProperty( ref myIsExpanded, value ); }
        }

        public bool IsSelected
        {
            get { return myIsSelected; }
            set { SetProperty( ref myIsSelected, value ); }
        }

        IEnumerable<INode> INode.Children
        {
            get { return Children; }
        }

        public ObservableCollection<ClusterTreeNode> Children { get; private set; }

        public INode Parent
        {
            get { return myParent; }
            set { SetProperty( ref myParent, value ); }
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