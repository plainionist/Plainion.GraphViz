using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Plainion.GraphViz.Presentation
{
    class GenericModuleChangedObserver<T> : AbstractModuleChangedObserver<T>
    {
        public GenericModuleChangedObserver(IModule<T> module)
            : base(module)
        {
        }

        protected override void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems.OfType<INotifyPropertyChanged>())
                {
                    item.PropertyChanged += OnPropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems.OfType<INotifyPropertyChanged>())
                {
                    item.PropertyChanged -= OnPropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var entry in Module.Items.OfType<INotifyPropertyChanged>())
                {
                    entry.PropertyChanged -= OnPropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                // safely ignore
            }
            else
            {
                throw new NotSupportedException(e.Action.ToString());
            }

            RaiseModuleChanged();
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e) => RaiseModuleChanged();
    }
}
