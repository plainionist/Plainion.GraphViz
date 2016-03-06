using System;
using Plainion.GraphViz.Modules.Reflection.Inheritance.Services.Framework;

namespace Plainion.GraphViz.Modules.Reflection.Inheritance.Services
{
    /// <summary>
    /// Small utils to simplify manage of inspectors
    /// </summary>
    public static class InspectionExtensions
    {
        public static void UpdateInspectorOnDemand<T>( this AssemblyInspectionService service, ref IInspectorHandle<T> inspector,  string applicationBase ) where T : InspectorBase
        {
            if( inspector != null && !applicationBase.StartsWith( inspector.Value.ApplicationBase, StringComparison.OrdinalIgnoreCase ) )
            {
                inspector.Dispose();
                inspector = null;
            }

            if( inspector == null )
            {
                inspector = service.CreateInspector<T>( applicationBase );
            }
        }

        public static void DestroyInspectorOnDemand<T>( this AssemblyInspectionService service, ref IInspectorHandle<T> inspector ) where T : InspectorBase
        {
            if( inspector != null )
            {
                inspector.Dispose();
                inspector = null;
            }
        }
    }
}
