using System;
using System.Collections.Generic;
using System.IO;
using Plainion.GraphViz.Viewer.Abstractions;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Documents
{
    class BasicDocumentProcessor
    {
        private IGraphPresentation myPresentation;
        private List<FailedItem> myFailedItems;

        public BasicDocumentProcessor(IGraphPresentation presentation)
        {
            myPresentation = presentation;

            DocumentCreators = new Dictionary<string, Func<IDocument>>(StringComparer.OrdinalIgnoreCase);
            DocumentCreators[".plain"] = () => new DotPlainDocument();
            DocumentCreators[".graphml"] = () => new GraphMLDocument();
            DocumentCreators[".dgml"] = () => new DgmlDocument();
            DocumentCreators[".dot"] = () => new DotLangDocument();

            AssociatedDocumentCreators = new Dictionary<string, Func<IDocument>>();
            AssociatedDocumentCreators[".tips"] = () => new ToolTipsDocument();

            myFailedItems = new List<FailedItem>();
        }

        // key: File Extension
        public IDictionary<string, Func<IDocument>> DocumentCreators { get; set; }

        // key: File Extension
        public IDictionary<string, Func<IDocument>> AssociatedDocumentCreators { get; set; }

        public IEnumerable<FailedItem> FailedItems
        {
            get { return myFailedItems; }
        }

        public void Process(string path)
        {
            var documents = LoadDocuments(path);

            foreach (var doc in documents)
            {
                Process(doc);
            }
        }

        private IEnumerable<IDocument> LoadDocuments(string path)
        {
            var ext = Path.GetExtension(path);

            if (DocumentCreators.ContainsKey(ext))
            {
                var doc = DocumentCreators[ext]();
                doc.Load(path);

                yield return doc;
            }
            else
            {
                throw new NotSupportedException("Unsupported file extension: " + ext);
            }

            foreach (var entry in AssociatedDocumentCreators)
            {
                var associatedDoc = Path.ChangeExtension(path, entry.Key);
                if (File.Exists(associatedDoc))
                {
                    var doc = entry.Value();
                    doc.Load(associatedDoc);

                    yield return doc;
                }
            }
        }

        private void Process(IDocument document)
        {
            var graphDoc = document as IGraphDocument;
            if (graphDoc != null)
            {
                myPresentation.Graph = graphDoc.Graph;
                myFailedItems.AddRange(graphDoc.FailedItems);
            }

            var styleDoc = document as IStyleDocument;
            if (styleDoc != null)
            {
                var nodeStyles = myPresentation.GetPropertySetFor<NodeStyle>();
                foreach (var style in styleDoc.NodeStyles)
                {
                    nodeStyles.Add(style);
                }

                var edgeStyles = myPresentation.GetPropertySetFor<EdgeStyle>();
                foreach (var style in styleDoc.EdgeStyles)
                {
                    edgeStyles.Add(style);
                }
            }

            var layoutDoc = document as ILayoutDocument;
            if (layoutDoc != null)
            {
                myPresentation.GetModule<IGraphLayoutModule>().Set(layoutDoc.NodeLayouts, layoutDoc.EdgeLayouts);
            }

            var captionDoc = document as ICaptionDocument;
            if (captionDoc != null)
            {
                var captionModule = myPresentation.GetPropertySetFor<Caption>();
                foreach (var caption in captionDoc.Captions)
                {
                    captionModule.Add(caption);
                }
            }

            var tooltipsDoc = document as ToolTipsDocument;
            if (tooltipsDoc != null)
            {
                var toolTipModule = myPresentation.GetPropertySetFor<ToolTipContent>();

                foreach (var tip in tooltipsDoc.ToolTips)
                {
                    toolTipModule.Add(tip);
                }
            }
        }
    }
}
