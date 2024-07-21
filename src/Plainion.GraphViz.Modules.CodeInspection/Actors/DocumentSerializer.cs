using System.IO;
using MessagePack;

namespace Plainion.GraphViz.Modules.CodeInspection.Actors
{
    /// <summary>
    /// Most "analysis documents" cannot be directly serialized and transported through Akka.Net. 
    /// Use this serializer to serialize the document to byte[] or file and send that through Akka.Net messages.
    /// </summary>
    class DocumentSerializer
    {
        public byte[] Serialize<T>(T doc)
        {
            return MessagePackSerializer.Serialize(doc);
        }

        public void Serialize<T>(T doc, string file)
        {
            using var stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write);

            MessagePackSerializer.Serialize(stream, doc);
        }

        public T Deserialize<T>(byte[] blob)
        {
            return MessagePackSerializer.Deserialize<T>(blob);
        }

        public T Deserialize<T>(string file)
        {
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                return MessagePackSerializer.Deserialize<T>(stream);
            }
        }
    }
}
