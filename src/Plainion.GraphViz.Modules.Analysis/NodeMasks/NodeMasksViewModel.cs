using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Prism.Commands;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Analysis.Filters
{
    class NodeMasksViewModel : ViewModelBase
    {
        public class Entry
        {
            private IGraphPresentation myPresentation;

            public Entry(INodeMask mask, IGraphPresentation presentation)
            {
                Contract.RequiresNotNull(mask, "mask");
                Contract.RequiresNotNull(presentation, "presentation");

                Mask = mask;
                myPresentation = presentation;

                SetToolTip(mask);
            }

            private void SetToolTip(INodeMask mask)
            {
                var nodeMask = mask as NodeMask;
                if (nodeMask == null)
                {
                    return;
                }

                var captionModule = myPresentation.GetPropertySetFor<Caption>();

                var nodeLabels = nodeMask.Values
                    .Select(nodeId => captionModule.Get(nodeId).DisplayText)
                    .OrderBy(l => l)
                    .ToList();

                ToolTip = string.Join(Environment.NewLine, nodeLabels);
            }

            public INodeMask Mask { get; private set; }

            public string ToolTip
            {
                get;
                private set;
            }
        }

        private IGraphPresentation myPresentation;
        private Entry mySelectedItem;

        public NodeMasksViewModel(IDomainModel model)
            : base(model)
        {
            Masks = new ObservableCollection<Entry>();

            DeleteMaskCommand = new DelegateCommand(OnDeleteMask);
            MoveMaskUpCommand = new DelegateCommand(OnMoveMaskUp);
            MoveMaskDownCommand = new DelegateCommand(OnMoveMaskDown);
        }

        protected override void OnPresentationChanged()
        {
            if (myPresentation == Model.Presentation)
            {
                return;
            }

            if (myPresentation != null)
            {
                myPresentation.GetModule<INodeMaskModule>().CollectionChanged -= OnMasksChanged;
            }

            myPresentation = Model.Presentation;

            if (myPresentation != null)
            {
                UpdateMasks();

                myPresentation.GetModule<INodeMaskModule>().CollectionChanged += OnMasksChanged;
            }
        }

        private void UpdateMasks()
        {
            Masks.Clear();

            foreach (var mask in myPresentation.GetModule<INodeMaskModule>().Items)
            {
                Masks.Add(new Entry(mask, myPresentation));
            }
        }

        private void OnMasksChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateMasks();
        }

        public ObservableCollection<Entry> Masks { get; private set; }

        public ICommand DeleteMaskCommand { get; private set; }

        public ICommand MoveMaskUpCommand { get; private set; }

        public ICommand MoveMaskDownCommand { get; private set; }

        private void OnDeleteMask()
        {
            if (SelectedItem == null)
            {
                return;
            }

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Remove(SelectedItem.Mask);

            SelectedItem = Masks.FirstOrDefault();
        }

        public Entry SelectedItem
        {
            get { return mySelectedItem; }
            set { SetProperty(ref mySelectedItem, value); }
        }

        private void OnMoveMaskUp()
        {
            var item = SelectedItem;
            if (item == null)
            {
                return;
            }

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.MoveUp(item.Mask);

            SelectedItem = Masks.Single(e => e.Mask == item.Mask);
        }

        private void OnMoveMaskDown()
        {
            var item = SelectedItem;
            if (item == null)
            {
                return;
            }

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.MoveDown(item.Mask);

            SelectedItem = Masks.Single(e => e.Mask == item.Mask);
        }
    }
}
