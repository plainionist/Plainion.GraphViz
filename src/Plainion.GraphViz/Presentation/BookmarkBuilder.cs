using System.IO;

namespace Plainion.GraphViz.Presentation
{
    /// <summary>
    /// Management of GraphPresentation bookmarks.
    /// </summary>
    public class BookmarkBuilder
    {
        public Bookmark Create(IGraphPresentation presentation, string caption)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryPresentationWriter(stream))
                {
                    writer.WriteNodeMasks(presentation.GetModule<NodeMaskModule>());
                    writer.WriteTansformations(presentation.GetModule<TransformationModule>());
                }

                stream.Flush();

                return new Bookmark(caption, stream.ToArray());
            }
        }

        public void Apply(IGraphPresentation presentation, Bookmark bookmark)
        {
            using (var stream = new MemoryStream(bookmark.State))
            {
                using (var reader = new BinaryPresentationReader(stream))
                {
                    var nodeMaskModule = presentation.GetModule<NodeMaskModule>();
                    nodeMaskModule.Clear();
                    reader.ReadNodeMasks(nodeMaskModule);

                    presentation.GetModule<TransformationModule>().Clear();
                    reader.ReadTansformations(presentation);
                }
            }
        }
    }
}
