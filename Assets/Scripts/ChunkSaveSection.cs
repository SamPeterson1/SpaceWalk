using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ChunkSaveSection : SaveSection
{
    public static readonly byte chunkIdentifier = 0;
    public Dictionary<Vector3, TerrainDeformation> deformations = new Dictionary<Vector3, TerrainDeformation>();

    public ChunkSaveSection()
    {
        identifier = chunkIdentifier;
    }

    protected override void ReadData(BinaryReader reader)
    {
        deformations = new Dictionary<Vector3, TerrainDeformation>();
        int numChunks = reader.ReadInt32();
        for (int i = 0; i < numChunks; i++)
        {
            Debug.Log("loading chunk");
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            Vector3 chunkPos = new Vector3(x, y, z);

            int numPoints = reader.ReadInt32();
            Debug.Log(numPoints);
            Dictionary<int, float> points = new Dictionary<int, float>();
            for (int ii = 0; ii < numPoints; ii++)
            {
                int index = reader.ReadInt32();
                float density = reader.ReadSingle();
                points.Add(index, density);
            }
            deformations.Add(chunkPos, new TerrainDeformation(points));
        }

        TerrainChunk.pastData = deformations;
    }

    protected override void WriteData(BinaryWriter writer)
    {
        
        Debug.Log(deformations.Count + " num chunks");
        writer.Write(deformations.Count);
        foreach (Vector3 chunkPos in deformations.Keys)
        {
            deformations.TryGetValue(chunkPos, out TerrainDeformation deformation);
            Dictionary<int, float> points = deformation.points;
            writer.Write(chunkPos.x);
            writer.Write(chunkPos.y);
            writer.Write(chunkPos.z);
            Debug.Log(points.Keys.Count);
            writer.Write(points.Keys.Count);
            foreach (int index in points.Keys)
            {
                points.TryGetValue(index, out float density);
                writer.Write(index);
                writer.Write(density);
            }
        }
    }
}
