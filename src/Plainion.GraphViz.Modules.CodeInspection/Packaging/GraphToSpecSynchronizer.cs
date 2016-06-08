using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;
using Plainion.GraphViz.Presentation;

using Cluster = Plainion.GraphViz.Model.Cluster;


namespace Plainion.GraphViz.Modules.CodeInspection.Packaging
{
    class GraphToSpecSynchronizer
    {
        private readonly Func<SystemPackaging> myGetSpec;
        private readonly Action<SystemPackaging> mySetSpec;
        private IGraphPresentation myPresentation;
        private IModuleChangedObserver myTransformationsObserver;
        private IModuleChangedJournal<Caption> myCaptionsJournal;
        // used to remember original cluster id mapping to renamed cluster name when playing back the rename into the spec file
        private readonly Dictionary<string, string> myClusterIdToExternalNameMapping;

        public GraphToSpecSynchronizer(Func<SystemPackaging> GetSpec, Action<SystemPackaging> SetSpec)
        {
            myGetSpec = GetSpec;
            mySetSpec = SetSpec;

            myClusterIdToExternalNameMapping = new Dictionary<string, string>();
        }

        public IGraphPresentation Presentation
        {
            get { return myPresentation; }
            set
            {
                myClusterIdToExternalNameMapping.Clear();

                if (myTransformationsObserver != null)
                {
                    myTransformationsObserver.ModuleChanged -= OnTransformationsChanged;
                    myTransformationsObserver.Dispose();
                    myTransformationsObserver = null;
                }

                if (myCaptionsJournal != null)
                {
                    myCaptionsJournal.ModuleChanged -= OnCaptionsChanged;
                    myCaptionsJournal.Dispose();
                    myCaptionsJournal = null;
                }

                myPresentation = value;

                if (myPresentation != null)
                {
                    var transformationModule = myPresentation.GetModule<ITransformationModule>();
                    myTransformationsObserver = transformationModule.CreateObserver();
                    myTransformationsObserver.ModuleChanged += OnTransformationsChanged;

                    var captionsModule = myPresentation.GetModule<ICaptionModule>();
                    myCaptionsJournal = captionsModule.CreateJournal();
                    myCaptionsJournal.ModuleChanged += OnCaptionsChanged;
                }
            }
        }

        private void OnTransformationsChanged(object sender, EventArgs eventArgs)
        {
            using (new Profile("GraphToSpecSynchronizer:OnTransformationsChanged"))
            {
                var spec = myGetSpec();

                UpdateSpec(spec);

                mySetSpec(spec);
            }
        }

        private void UpdateSpec(SystemPackaging spec)
        {
            HandleNodeToClusterAssignment(spec);

            HandleRemovedClusters(spec);

            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            HandleClusterRenames(spec, transformationModule.Graph.Clusters);
        }

        // this will also handle new clusters (but only if they have notes)
        private void HandleNodeToClusterAssignment(SystemPackaging spec)
        {
            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            var transformation = transformationModule.Items.OfType<DynamicClusterTransformation>().Single();
            var captions = myPresentation.GetModule<ICaptionModule>();

            var clusters = spec.Packages
                .SelectMany(p => p.Clusters)
                .ToList();

            foreach (var entry in transformation.NodeToClusterMapping)
            {
                var externalClusterId = GetExternalClusterId(entry.Value);

                var clustersMatchingNode = clusters
                    .Where(c => c.Matches(entry.Key))
                    .ToList();

                // remove from all (potentially old) clusters
                var clustersToRemoveFrom = clustersMatchingNode
                    .Where(c => entry.Value == null || c.Name != externalClusterId);
                foreach (var cluster in clustersToRemoveFrom)
                {
                    var exactMatch = cluster.Includes.FirstOrDefault(p => p.Pattern == entry.Key);
                    if (exactMatch != null)
                    {
                        cluster.Patterns.Remove(exactMatch);
                    }
                    else
                    {
                        cluster.Patterns.Add(new Exclude { Pattern = entry.Key });
                    }
                }

                if (entry.Value == null)
                {
                    continue;
                }

                // add to the cluster it should now belong to
                var clusterToAddTo = clusters
                    .FirstOrDefault(c => c.Name == externalClusterId);

                if (clusterToAddTo == null)
                {
                    // --> new cluster added in UI
                    clusterToAddTo = new Spec.Cluster { Name = captions.Get(entry.Value).DisplayText };
                    clusters.Add(clusterToAddTo);
                    spec.Packages.First().Clusters.Add(clusterToAddTo);

                    // also update the mapping
                    myClusterIdToExternalNameMapping[entry.Value] = clusterToAddTo.Name;
                }

                if (clusterToAddTo.Matches(entry.Key))
                {
                    // node already or again matched
                    // -> ignore
                    continue;
                }
                else
                {
                    clusterToAddTo.Patterns.Add(new Include { Pattern = entry.Key });
                }
            }
        }

        private void HandleRemovedClusters(SystemPackaging spec)
        {
            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            var transformation = transformationModule.Items.OfType<DynamicClusterTransformation>().Single();

            var removedExternalClusterIds = transformation.ClusterVisibility
                .Where(e => e.Value == false)
                .Select(e => GetExternalClusterId(e.Key));

            foreach (var externalClusterId in removedExternalClusterIds)
            {
                foreach (var package in spec.Packages)
                {
                    var cluster = package.Clusters.FirstOrDefault(c => c.Name == externalClusterId);
                    if (cluster != null)
                    {
                        package.Clusters.Remove(cluster);
                        break;
                    }
                }
            }
        }

        private void HandleClusterRenames(SystemPackaging spec, IEnumerable<Model.Cluster> potentiallyRenamedClusters)
        {
            var specClusters = spec.Packages
                .SelectMany(p => p.Clusters)
                .ToList();

            var captions = myPresentation.GetModule<ICaptionModule>();
            foreach (var cluster in potentiallyRenamedClusters)
            {
                var externalClusterId = GetExternalClusterId(cluster.Id);

                var specCluster = specClusters.FirstOrDefault(c => c.Name == externalClusterId);
                if (specCluster == null)
                {
                    // this is a new cluster EMPTY not yet there in spec
                    // -> nothing to do - will be handled once nodes are added to this new cluster
                }
                else
                {
                    specCluster.Name = captions.Get(cluster.Id).DisplayText;
                    myClusterIdToExternalNameMapping[cluster.Id] = specCluster.Name;
                }
            }
        }

        private string GetExternalClusterId(string clusterId)
        {
            if (clusterId == null)
            {
                return null;
            }

            string externalClusterId;
            if (!myClusterIdToExternalNameMapping.TryGetValue(clusterId, out externalClusterId))
            {
                externalClusterId = clusterId;
            }
            return externalClusterId;
        }

        private void OnCaptionsChanged(object sender, EventArgs eventArgs)
        {
            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            var renamedClusterIds = myCaptionsJournal.Entries
                .Where(c => transformationModule.Graph.Clusters.Any(owner => owner.Id == c.OwnerId))
                .Select(c => c.OwnerId)
                .ToList();

            if (renamedClusterIds.Count == 0)
            {
                return;
            }

            myCaptionsJournal.Clear();

            var spec = myGetSpec();

            var modelClusters = transformationModule.Graph.Clusters
                .Where(c => renamedClusterIds.Contains(c.Id))
                .ToList();

            HandleClusterRenames(spec, modelClusters);

            mySetSpec(spec);
        }
    }
}
