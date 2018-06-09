using Plainion.GraphViz.Dot;
using Plainion.GraphViz.Presentation;
using System;
using System.Linq;

namespace Plainion.GraphViz.Modules.Documents
{
    class GraphToDotLangSynchronizer
    {
        private IGraphPresentation myPresentation;
        private IModuleChangedObserver myTransformationsObserver;
        private IModuleChangedJournal<Caption> myCaptionsJournal;
        private Action<IGraphPresentation> myWriteDocument;

        public void Attach(IGraphPresentation presentation, Action<IGraphPresentation> writeDocument)
        {
            myWriteDocument = writeDocument;

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

            myPresentation = presentation;

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

        private void OnTransformationsChanged(object sender, EventArgs eventArgs)
        {
            myWriteDocument(myPresentation);
        }

        private void OnCaptionsChanged(object sender, EventArgs eventArgs)
        {
            var transformationModule = myPresentation.GetModule<ITransformationModule>();
            var haveClustersRenamed = myCaptionsJournal.Entries
                .Any(c => transformationModule.Graph.Clusters.Any(owner => owner.Id == c.OwnerId));

            myCaptionsJournal.Clear();

            if (!haveClustersRenamed)
            {
                return;
            }

            myWriteDocument(myPresentation);
        }

        public void Detach()
        {
            myPresentation = null;
            myWriteDocument = null;
        }
    }
}
