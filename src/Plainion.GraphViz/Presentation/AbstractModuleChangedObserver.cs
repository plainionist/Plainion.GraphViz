using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Plainion.GraphViz.Presentation
{
    abstract class AbstractModuleChangedObserver<T> : IModuleChangedObserver
    {
        private int myMuteGuardCounter;

        public AbstractModuleChangedObserver(IModule<T> module)
        {
            System.Contract.RequiresNotNull(module);

            Module = module;

            myMuteGuardCounter = 0;

            Module.CollectionChanged += OnCollectionChanged;

            foreach (var entry in Module.Items.OfType<INotifyPropertyChanged>())
            {
                entry.PropertyChanged += OnPropertyChanged;
            }
        }

        protected IModule<T> Module { get; private set; }

        public event EventHandler ModuleChanged;

        protected abstract void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e);

        protected abstract void OnPropertyChanged(object sender, PropertyChangedEventArgs e);

        protected void RaiseModuleChanged()
        {
            if (ModuleChanged != null && myMuteGuardCounter == 0)
            {
                ModuleChanged.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            if (Module == null)
            {
                return;
            }

            foreach (var entry in Module.Items.OfType<INotifyPropertyChanged>())
            {
                entry.PropertyChanged -= OnPropertyChanged;
            }

            Module.CollectionChanged -= OnCollectionChanged;

            ModuleChanged = null;

            Module = null;
        }

        public IDisposable Mute() => new MuteGuard(this);

        private class MuteGuard : IDisposable
        {
            private AbstractModuleChangedObserver<T> myOwner;

            public MuteGuard(AbstractModuleChangedObserver<T> owner)
            {
                myOwner = owner;
                myOwner.myMuteGuardCounter++;
            }

            public void Dispose()
            {
                if (myOwner != null)
                {
                    myOwner.myMuteGuardCounter--;
                    myOwner = null;
                }
            }
        }
    }
}
