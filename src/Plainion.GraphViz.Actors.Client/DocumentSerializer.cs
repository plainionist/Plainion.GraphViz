﻿using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json;
using Plainion.GraphViz.Dot;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Actors.Client;

/// <summary>
/// Most "analysis documents" cannot be directly serialized and transported through Akka.Net. 
/// Use this serializer to serialize the document to byte[] or file and send that through Akka.Net messages.
/// </summary>
public class DocumentSerializer
{
    public byte[] Serialize<T>(T doc)
    {
        string jsonString = JsonConvert.SerializeObject(doc);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonString);

        return Compress(jsonBytes);
    }

    public void Serialize<T>(T doc, string file)
    {
        string jsonString = JsonConvert.SerializeObject(doc);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
        byte[] compressedBytes = Compress(jsonBytes);

        using var stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write);
        stream.Write(compressedBytes, 0, compressedBytes.Length);
    }

    public T Deserialize<T>(byte[] blob)
    {
        byte[] decompressedBytes = Decompress(blob);
        string jsonString = System.Text.Encoding.UTF8.GetString(decompressedBytes);

        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new TupleStringConverter());

        return JsonConvert.DeserializeObject<T>(jsonString, settings);
    }

    public T Deserialize<T>(string file)
    {
        byte[] compressedBytes;

        using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            compressedBytes = memoryStream.ToArray();
        }

        return Deserialize<T>(compressedBytes);
    }

    private byte[] Compress(byte[] data)
    {
        using var compressedStream = new MemoryStream();
        using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
        {
            zipStream.Write(data, 0, data.Length);
        }

        return compressedStream.ToArray();
    }

    private byte[] Decompress(byte[] data)
    {
        using var compressedStream = new MemoryStream(data);
        using var decompressedStream = new MemoryStream();
        using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
        {
            zipStream.CopyTo(decompressedStream);
        }

        return decompressedStream.ToArray();
    }

    public static void Serialize(string file, IGraphPresentation presentation)
    {
        var graph = presentation.GetModule<ITransformationModule>().Graph;

        if (graph.Nodes.Any(presentation.Picking.Pick))
        {
            Console.WriteLine("Dumping graph ...");

            var writer = new DotWriter(file)
            {
                PrettyPrint = true
            };

            writer.Write(graph, presentation.Picking, presentation);
        }
        else
        {
            Console.WriteLine("Graph is empty");

            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }

}
