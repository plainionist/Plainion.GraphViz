﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Plainion.GraphViz.Actors.Client;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance.Analyzers;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance.Actors
{
    class InheritanceClient : ActorClientBase
    {
        public async Task<IEnumerable<TypeDescriptor>> GetAllTypesAsync(string assemblyLocation, CancellationToken cancellationToken = default)
        {
            var msg = new GetAllTypesMessage
            {
                AssemblyLocation = assemblyLocation
            };

            var response = await this.ProcessAsync(typeof(AllTypesActor), msg, cancellationToken);

            if (response is AllTypesMessage m)
            {
                return m.Types;
            }
            else
            {
                return null;
            }
        }

        public async Task<TypeRelationshipDocument> AnalyzeInheritanceAsync(string assemblyLocation, bool ignoreDotNetTypes, TypeDescriptor typeToAnalyse, CancellationToken cancellationToken = default)
        {
            var msg = new GetInheritanceGraphMessage
            {
                IgnoreDotNetTypes = ignoreDotNetTypes,
                AssemblyLocation = assemblyLocation,
                TypeToAnalyze = typeToAnalyse
            };

            var response = await this.ProcessAsync(typeof(InheritanceActor), msg, cancellationToken);

            if (response is InheritanceGraphMessage m)
            {
                var serializer = new DocumentSerializer();
                return serializer.Deserialize<TypeRelationshipDocument>(m.Document);
            }
            else
            {
                return null;
            }
        }
    }
}
