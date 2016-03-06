using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Plainion.GraphViz.Modules.Reflection.Packaging
{
    public class PrivateSettersContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties( Type type, MemberSerialization memberSerialization )
        {
            var props = type.GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
                .Select( p => base.CreateProperty( p, memberSerialization ) )
                .ToList();

            props.ForEach( p => { p.Writable = true; p.Readable = true; } );

            return props;
        }
    }
}
