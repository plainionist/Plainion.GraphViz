/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2016. All rights reserved
   ------------------------------------------------------------------------------------------------- */
   
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Modules.Reflection.Services.Framework;

namespace Plainion.GraphViz.Modules.Reflection.Analysis.Packaging
{
    class PackagingGraphBuilder
    {
        private RelaxedGraphBuilder myBuilder;
        private IDictionary<string, TypeDescriptor> myIdToTypeMap;
        private IDictionary<string, EdgeType> myEdgeTypes;

        public PackagingGraphBuilder()
        {
            myBuilder = new RelaxedGraphBuilder();
            myIdToTypeMap = new Dictionary<string, TypeDescriptor>();
            myEdgeTypes = new Dictionary<string, EdgeType>();
        }

        public bool IgnoreDotNetTypes { get; set; }

        internal void Process( Assembly assembly )
        {
            foreach( var type in assembly.GetTypes() )
            {
                ProcessType( type );
            }
        }

        private void ProcessType( Type type )
        {
            if( type.Namespace == null )
            {
                // some .net internal implementation detail ...
                return;
            }

            if( IgnoreType( type ) )
            {
                return;
            }

            var name = type.Name;
            var typeDesc = new TypeDescriptor( type );
            myBuilder.TryAddNode( typeDesc.Id );
            myIdToTypeMap[ typeDesc.Id ] = typeDesc;

            if( type.BaseType != null && !IsPrimitive( type.BaseType ) && !IgnoreType( type.BaseType ) )
            {
                var baseDesc = new TypeDescriptor( type.BaseType );

                var edge = myBuilder.TryAddEdge( typeDesc.Id, baseDesc.Id );
                myIdToTypeMap[ baseDesc.Id ] = baseDesc;
                myEdgeTypes.Add( edge.Id, EdgeType.DerivesFrom );
            }

            var interfaces = type.GetInterfaces();
            if( type.BaseType != null )
            {
                interfaces = interfaces.Except( type.BaseType.GetInterfaces() ).ToArray();
            }
            foreach( var iface in interfaces )
            {
                if( IgnoreType( iface ) )
                {
                    continue;
                }

                var ifaceDesc = new TypeDescriptor( iface );

                var edge = myBuilder.TryAddEdge( typeDesc.Id, ifaceDesc.Id );
                myIdToTypeMap[ ifaceDesc.Id ] = ifaceDesc;
                myEdgeTypes.Add( edge.Id, EdgeType.Implements );
            }
        }

        private bool IgnoreType( Type type )
        {
            if( IgnoreDotNetTypes )
            {
                return type.Namespace == "System" || type.Namespace.StartsWith( "System." );
            }

            return false;
        }

        private bool IsPrimitive( Type type )
        {
            return type == typeof( object ) || type == typeof( ValueType ) || type == typeof( Enum );
        }

        internal void WriteTo( string forTypeId, TypeRelationshipDocument document )
        {
            var visitedTypes = new HashSet<TypeDescriptor>();
            TakeSiblingsOf( document, visitedTypes, myIdToTypeMap[ forTypeId ] );

            foreach( var edge in document.Graph.Edges )
            {
                document.EdgeTypes.Add( edge.Id, myEdgeTypes[ edge.Id ] );
            }
        }

        private void TakeSiblingsOf( TypeRelationshipDocument document, HashSet<TypeDescriptor> visitedTypes, params TypeDescriptor[] roots )
        {
            if( !roots.Any() )
            {
                return;
            }

            var typesToFollow = new HashSet<TypeDescriptor>();

            foreach( var root in roots )
            {
                visitedTypes.Add( root );

                var node = myBuilder.Graph.FindNode( root.Id );

                document.AddNode( root );

                foreach( var edge in node.In )
                {
                    var source = myIdToTypeMap[ edge.Source.Id ];
                    document.AddEdge( source, root );
                    typesToFollow.Add( source );
                }

                // TODO: down only - otherwise we need a "MaxDepth"
                //foreach( var edge in node.Out )
                //{
                //    var target = myIdToTypeMap[ edge.Target.Id ];
                //    document.AddEdge( root, target );
                //    typesToFollow.Add( target );
                //}
            }

            typesToFollow.ExceptWith( visitedTypes );

            TakeSiblingsOf( document, visitedTypes, typesToFollow.ToArray() );
        }
    }
}
