using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BiomeSaveSection : SaveSection
{
    public static readonly byte biomeIdentifier = 1;
    public BiomeGenerator biomeGenerator;

    public BiomeSaveSection()
    {
        identifier = biomeIdentifier;
    }

    protected override void ReadData(BinaryReader reader)
    {
        int numBiomes = reader.ReadInt32();
        Vector4[] biomes = new Vector4[numBiomes];
        for (int i = 0; i < numBiomes; i ++)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            int biome = reader.ReadInt32();

            biomes[i] = new Vector4(x, y, z, biome);
        }

        biomeGenerator.GenerateBiomes(biomes);
    }

    protected override void WriteData(BinaryWriter writer)
    {
        writer.Write(biomeGenerator.biomePoints.Length);
        foreach(BiomeGenerator.BiomePoint biomePoint in biomeGenerator.biomePoints)
        {
            Vector3 pos = biomePoint.pos;
            writer.Write(pos.x);
            writer.Write(pos.y);
            writer.Write(pos.z);
            writer.Write(biomePoint.biome);
        }
    }
}
