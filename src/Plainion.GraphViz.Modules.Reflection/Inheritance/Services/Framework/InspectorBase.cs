using System;
using Plainion;

namespace Plainion.GraphViz.Modules.Reflection.Inheritance.Services.Framework
{
    public abstract class InspectorBase : MarshalByRefObject
    {
        protected InspectorBase( string applicationBase )
        {
            Contract.RequiresNotNullNotEmpty( applicationBase, "applicationBase" );

            ApplicationBase = applicationBase;
        }

        public string ApplicationBase
        {
            get;
            private set;
        }
    }
}
