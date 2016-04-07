using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    class TransformationModule : AbstractModule<IGraphTransformation>, ITransformationModule
    {
        private GraphPresentation myPresentation;
        private IGraph myGraph;
        private List<IGraphTransformation> myTransformations;

        public TransformationModule(GraphPresentation presentation)
        {
            myPresentation = presentation;
            myTransformations = new List<IGraphTransformation>();

            // Hint: we already add DynamicClusterTransformation here so that all fold operations are behind that and
            // that newly added nodes to a cluster are automatically folded (if folding is applied)
            Add(new DynamicClusterTransformation());
        }

        public IGraph Graph
        {
            get { return myTransformations.Count == 0 ? myPresentation.Graph : myGraph; }
        }

        public override IEnumerable<IGraphTransformation> Items
        {
            get { return myTransformations; }
        }

        public void Add(IGraphTransformation transformation)
        {
            myTransformations.Add(transformation);

            var notifyPropertyChanged = transformation as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged += OnTransformationChanged;
            }

            ApplyTransformations();

            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, transformation));
        }

        private void OnTransformationChanged(object sender, PropertyChangedEventArgs e)
        {
            ApplyTransformations();
        }

        private void ApplyTransformations()
        {
            if (myPresentation.Graph == null)
            {
                // e.g. because we added initial DynamicClusterTransformation here
                return;
            }

            using (new Profile("ApplyTransformations"))
            {
                myGraph = myPresentation.Graph;

                foreach (var transformation in myTransformations)
                {
                    myGraph = transformation.Transform(myGraph);
                }
            }
        }

        public void Remove(IGraphTransformation transformation)
        {
            myTransformations.Remove(transformation);

            var notifyPropertyChanged = transformation as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged -= OnTransformationChanged;
            }

            ApplyTransformations();

            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, transformation));
        }
    }
}
