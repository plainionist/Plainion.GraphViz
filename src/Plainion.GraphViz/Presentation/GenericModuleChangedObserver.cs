using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Plainion.GraphViz.Presentation
{
    internal class GenericModuleChangedObserver<T> : IModuleChangedObserver
    {
        private IModule<T> myModule;

        public GenericModuleChangedObserver( IModule<T> module )
        {
            myModule = module;

            myModule.CollectionChanged += OnCollectionChanged;

            foreach( var entry in myModule.Items.OfType<INotifyPropertyChanged>() )
            {
                entry.PropertyChanged += OnPropertyChanged;
            }
        }

        public event EventHandler ModuleChanged;

        private void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if( e.Action == NotifyCollectionChangedAction.Add )
            {
                foreach( var item in e.NewItems.OfType<INotifyPropertyChanged>() )
                {
                    item.PropertyChanged += OnPropertyChanged;
                }
            }
            else if( e.Action == NotifyCollectionChangedAction.Remove )
            {
                foreach( var item in e.OldItems.OfType<INotifyPropertyChanged>() )
                {
                    item.PropertyChanged -= OnPropertyChanged;
                }
            }
            else if( e.Action == NotifyCollectionChangedAction.Move )
            {
                // safely ignore
            }
            else
            {
                throw new NotSupportedException( e.Action.ToString() );
            }

            RaiseModuleChanged();
        }

        private void OnPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            RaiseModuleChanged();
        }

        private void RaiseModuleChanged()
        {
            if( ModuleChanged != null )
            {
                ModuleChanged( this, EventArgs.Empty );
            }
        }

        public void Dispose()
        {
            if( myModule != null )
            {
                foreach( var entry in myModule.Items.OfType<INotifyPropertyChanged>() )
                {
                    entry.PropertyChanged -= OnPropertyChanged;
                }

                myModule.CollectionChanged -= OnCollectionChanged;

                ModuleChanged = null;

                myModule = null;
            }
        }
    }
}
