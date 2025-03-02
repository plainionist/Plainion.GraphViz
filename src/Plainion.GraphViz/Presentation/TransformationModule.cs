﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Plainion.Graphs;

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
            get { return myGraph ?? myPresentation.Graph; }
        }

        public override IEnumerable<IGraphTransformation> Items
        {
            get { return myTransformations; }
        }

        public void Add(IGraphTransformation transformation)
        {
            myTransformations.Add(transformation);

            if (transformation is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged += OnTransformationChanged;
            }

            ApplyTransformations();

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, transformation));
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

        public void Clear()
        {
            var transformations = myTransformations.ToList();

            foreach (var transformation in transformations)
            {
                if (transformation is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                myTransformations.Remove(transformation);

                if (transformation is INotifyPropertyChanged notifyPropertyChanged)
                {
                    notifyPropertyChanged.PropertyChanged -= OnTransformationChanged;
                }
            }

            ApplyTransformations();

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, transformations));
        }
    }
}
