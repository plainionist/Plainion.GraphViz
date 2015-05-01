using System;
using System.ComponentModel.Composition;
using Plainion.GraphViz.Viewer.Configuration;

namespace Plainion.GraphViz.Viewer.Services
{
    [Export( typeof( ConfigurationService ) )]
    public class ConfigurationService
    {
        public Config Config { get; private set; }

        public ConfigurationService()
        {
            Config = Config.LoadDefaults();
        }

        internal void Update( string dataRoot )
        {
            Config = Config.Load( dataRoot );

            if( ConfigChanged != null )
            {
                ConfigChanged( this, EventArgs.Empty );
            }
        }

        public event EventHandler ConfigChanged;
    }
}
