using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Services;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Tests.Packaging.Services
{
    [TestFixture]
    class SpecUtilsTests
    {
        [Test]
        public void Serialize_WithClusters_IsDeserializable()
        {
            var spec = new SystemPackaging();

            var package = new Package { Name = "P1" };
            var cluster1 = new Cluster { Name = "C1", Id = "42" };
            cluster1.Patterns.Add( new Include { Pattern = "*tests*" } );
            package.Clusters.Add( cluster1 );

            spec.Packages.Add( package );

            var newSpec = SpecUtils.Deserialize( SpecUtils.Serialize( spec ) );

            Assert.That( newSpec.Packages.Count, Is.EqualTo( 1 ) );
            Assert.That( newSpec.Packages.Single().Clusters.Count, Is.EqualTo( 1 ) );
            Assert.That( newSpec.Packages.Single().Clusters.Single().Includes.Count(), Is.EqualTo( 1 ) );
            Assert.That( newSpec.Packages.Single().Clusters.Single().Includes.Single().Pattern, Is.EqualTo( "*tests*" ) );
        }
    }
}
