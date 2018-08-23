using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Analyzers;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Actors
{
    class AnalysisDocumentSerializer
    {
        internal void Serialize(AnalysisDocument doc, string file)
        {
            //SerializeJson(doc, file);
            SerializeBinary(doc, file);
        }

        private void SerializeJson(AnalysisDocument doc, string file)
        {
            var serializer = new JsonSerializer();
            using (var sw = new StreamWriter(file))
            {
                using (var writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, doc);
                }
            }
        }

        private void SerializeBinary(AnalysisDocument doc, string file)
        {
            using (var stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, doc);
            }
        }

        internal AnalysisDocument Deserialize(string file)
        {
            //return DeserializeJson(file);
            return DeserializeBinary(file);
        }

        private AnalysisDocument DeserializeJson(string file)
        {
            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new PrivateSettersContractResolver();

            var serializer = JsonSerializer.Create(settings);
            using (var sr = new StreamReader(file))
            {
                using (var reader = new JsonTextReader(sr))
                {
                    return serializer.Deserialize<AnalysisDocument>(reader);
                }
            }
        }

        private AnalysisDocument DeserializeBinary(string file)
        {
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                var formatter = new BinaryFormatter();
                return (AnalysisDocument)formatter.Deserialize(stream);
            }
        }
    }
}
