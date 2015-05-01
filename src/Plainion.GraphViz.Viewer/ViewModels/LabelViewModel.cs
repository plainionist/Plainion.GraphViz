using Microsoft.Practices.Prism.Mvvm;

namespace Plainion.GraphViz.Viewer.ViewModels
{
    public class LabelViewModel : BindableBase
    {
        private string myCommited;
        private string myTemporal;

        public LabelViewModel( string original )
        {
            Original = original;

            Commited = Original;
            Temporal = Original;
        }

        public string Original { get; private set; }

        public string Commited
        {
            get
            {
                return myCommited;
            }
            set
            {
                if( myCommited != value )
                {
                    myCommited = value;

                    OnPropertyChanged( "Commited" );
                }

                Temporal = value;
            }
        }

        public string Temporal
        {
            get
            {
                return myTemporal;
            }
            set
            {
                if( myTemporal == value )
                {
                    return;
                }

                myTemporal = value;

                OnPropertyChanged( "Temporal" );
            }
        }
    }
}
