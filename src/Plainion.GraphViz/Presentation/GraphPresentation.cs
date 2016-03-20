using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Plainion.GraphViz.Model;
using Plainion;

namespace Plainion.GraphViz.Presentation
{
    // TODO: we should add later support for dynamic add of modules in order to support "plugins"
    // e.g. loaded via MEF from directly
    public class GraphPresentation : IGraphPresentation
    {
        private List<object> myModules;
        private IGraph myGraph;
        private IModuleChangedObserver myNodeMaskModuleObserver;
        private IModuleChangedObserver myEdgeMaskModuleObserver;

        public GraphPresentation()
        {
            myModules = new List<object>();

            myModules.Add( new PropertySetModule<ToolTipContent>( id => new ToolTipContent( id, null ) ) );
            myModules.Add( new PropertySetModule<Selection>( id => new Selection( id ) { IsSelected = false } ) );
            // TODO: move translation from string to WPF entities to parser
            myModules.Add( new PropertySetModule<NodeStyle>( id => new NodeStyle( id ) { BorderColor = Brushes.Black, FillColor = Brushes.LightGray, Shape = "ellipse", Style = "solid" } ) );
            myModules.Add( new PropertySetModule<EdgeStyle>( id => new EdgeStyle( id ) { Color = Brushes.Black, Style = "solid" } ) );
            myModules.Add( new CaptionModule( id => new Caption( id, null ) ) );
            myModules.Add( new GraphLayoutModule() );
            myModules.Add( new NodeMaskModule() );
            myModules.Add( new EdgeMaskModule() );
            myModules.Add( new TransformationModule( this ) );

            Picking = new PickingCache( this, new GraphPicking( this ) );

            myNodeMaskModuleObserver = GetModule<INodeMaskModule>().CreateObserver();
            myNodeMaskModuleObserver.ModuleChanged += OnModuleChanged;

            myEdgeMaskModuleObserver = GetModule<IEdgeMaskModule>().CreateObserver();
            myEdgeMaskModuleObserver.ModuleChanged += OnModuleChanged;
        }

        private void OnModuleChanged( object sender, EventArgs e )
        {
            if( GraphVisibilityChanged != null )
            {
                GraphVisibilityChanged( this, EventArgs.Empty );
            }
        }

        public IPropertySetModule<T> GetPropertySetFor<T>() where T : AbstractPropertySet
        {
            return myModules.OfType<PropertySetModule<T>>().SingleOrDefault();
        }

        public T GetModule<T>()
        {
            return myModules.OfType<T>().SingleOrDefault();
        }

        public IGraph Graph
        {
            get
            {
                return myGraph;
            }
            set
            {
                if( myGraph != null )
                {
                    throw new InvalidOperationException( "Graph already set to presentation. Changing Graph is not allowed!" );
                }

                // graph is mutable in order to support easy graph building -> protect against graph changes
                myGraph = Objects.Clone( value );
            }
        }

        public IGraphPicking Picking { get; private set; }

        public void InvalidateLayout()
        {
            GetModule<IGraphLayoutModule>().Clear();
        }

        public event EventHandler GraphVisibilityChanged;

        public void Dispose()
        {
            if( myModules != null )
            {
                myNodeMaskModuleObserver.ModuleChanged -= OnModuleChanged;
                myNodeMaskModuleObserver.Dispose();

                myEdgeMaskModuleObserver.ModuleChanged -= OnModuleChanged;
                myEdgeMaskModuleObserver.Dispose();

                GraphVisibilityChanged = null;

                myModules.Clear();
                myModules = null;
            }
        }

        public IGraphPresentation UnionWith( IGraphPresentation other, Func<IGraphPresentation> presentationCreator )
        {
            var result = presentationCreator();

            var builder = new RelaxedGraphBuilder();

            foreach( var node in GetNodes( this, other ) )
            {
                builder.TryAddNode( node.Id );
            }

            foreach( var edge in GetEdges( this, other ) )
            {
                builder.TryAddEdge( edge.Source.Id, edge.Target.Id );
            }

            foreach( var cluster in GetClusters( this, other ) )
            {
                builder.TryAddCluster( cluster.Id, cluster.Nodes.Select( n => n.Id ) );
            }

            result.Graph = builder.Graph;

            UnionWith<ToolTipContent>( this, other, result );
            UnionWith<Selection>( this, other, result );
            UnionWith<NodeStyle>( this, other, result );
            UnionWith<EdgeStyle>( this, other, result );
            UnionWith<Caption>( this, other, result );

            {
                var resultModule = result.GetModule<INodeMaskModule>();

                foreach( var item in GetModule<INodeMaskModule>().Items.Concat( other.GetModule<INodeMaskModule>().Items ) )
                {
                    resultModule.Push( item );
                }
            }

            {
                var resultModule = result.GetModule<IEdgeMaskModule>();

                foreach( var item in GetModule<IEdgeMaskModule>().Items )
                {
                    resultModule.Add( item );
                }

                foreach( var item in other.GetModule<IEdgeMaskModule>().Items )
                {
                    if( !resultModule.Items.Any( i => i.Id == item.Id ) )
                    {
                        resultModule.Add( item );
                    }
                }
            }

            return result;
        }

        private IEnumerable<Node> GetNodes( GraphPresentation lhs, IGraphPresentation rhs )
        {
            if( lhs.Graph == null ) return rhs.Graph.Nodes;
            if( rhs.Graph == null ) return lhs.Graph.Nodes;

            return lhs.Graph.Nodes.Concat( rhs.Graph.Nodes );
        }

        private IEnumerable<Edge> GetEdges( GraphPresentation lhs, IGraphPresentation rhs )
        {
            if( lhs.Graph == null ) return rhs.Graph.Edges;
            if( rhs.Graph == null ) return lhs.Graph.Edges;

            return lhs.Graph.Edges.Concat( rhs.Graph.Edges );
        }

        private IEnumerable<Cluster> GetClusters( GraphPresentation lhs, IGraphPresentation rhs )
        {
            if( lhs.Graph == null ) return rhs.Graph.Clusters;
            if( rhs.Graph == null ) return lhs.Graph.Clusters;

            return lhs.Graph.Clusters.Concat( rhs.Graph.Clusters );
        }

        private static void UnionWith<T1>( IGraphPresentation lhs, IGraphPresentation rhs, IGraphPresentation result ) where T1 : AbstractPropertySet
        {
            var resultModule = result.GetPropertySetFor<T1>();

            foreach( var item in lhs.GetPropertySetFor<T1>().Items )
            {
                resultModule.Add( item );
            }

            foreach( var item in rhs.GetPropertySetFor<T1>().Items )
            {
                if( !resultModule.Items.Any( i => i.OwnerId == item.OwnerId ) )
                {
                    resultModule.Add( item );
                }
            }
        }
    }
}
