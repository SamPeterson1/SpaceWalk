using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeGenerator
{
    public Dictionary<BiomeType, Biome> biomes;
    public TextAsset biomesJSON;
    public BiomePoint[] biomePoints;
    public struct BiomePoint
    {
        public int biome;
        public float roughness;
        public float amplitude;
        public float persistence;
        public float baseRoughness;
        public float numLayers;
        public float minRadius;
        public Vector3 center;
        public Vector3 pos;
    }

    public BiomeGenerator(TextAsset biomesJSON)
    {
        this.biomesJSON = biomesJSON;
        LoadBiomes();
        SaveManager saveManager = GameObject.FindGameObjectWithTag("SaveManager").GetComponent<SaveManager>();
        BiomeSaveSection biomeSaveSection = saveManager.GetSaveSection(BiomeSaveSection.biomeIdentifier) as BiomeSaveSection;
        biomeSaveSection.biomeGenerator = this;
        biomes = new Dictionary<BiomeType, Biome>();
        
        foreach(Biome biome in LoadBiomes())
        {
            biomes.Add(biome.type, biome);
        }
    }

    List<Biome> LoadBiomes()
    {
        string text = biomesJSON.text;
        Debug.Log(text);
        text = text.Replace("\n", "");
        Debug.Log(text);
        string[] array = text.Split(new string[] { "BIOME" }, System.StringSplitOptions.RemoveEmptyEntries);
        List<Biome> biomes = new List<Biome>();
        foreach(string str in array)
        {
            string toBiome = str.Replace(" ", "");
            biomes.Add(JsonUtility.FromJson<Biome>(toBiome));
        }

        return biomes;
    }

    public void GenerateBiomes(Vector4[] biomes)
    {
        biomePoints = new BiomePoint[biomes.Length];
        for(int i = 0; i < biomes.Length; i ++)
        {
            biomePoints[i] = GetBiomePoint(new Vector3(biomes[i].x, biomes[i].y, biomes[i].z), (BiomeType)(int)biomes[i].w);
        }
    }

    public BiomePoint GetBiomePoint(Vector3 pos, BiomeType biomeType)
    {
        BiomePoint biomePoint;
        biomePoint.pos = pos;
        biomes.TryGetValue(biomeType, out Biome biome);

        biomePoint.biome = (int)biome.type;
        NoiseSettings settings = biome.settings;
        biomePoint.amplitude = settings.amplitude;
        biomePoint.baseRoughness = settings.baseRoughness;
        biomePoint.center = settings.center;
        biomePoint.minRadius = settings.minRadius;
        biomePoint.numLayers = settings.numLayers;
        biomePoint.persistence = settings.persistence;
        biomePoint.roughness = settings.roughness;

        return biomePoint;
    }

    public Biome GetBiomeAt(Vector3 pos)
    {
        float minDist = -1;
        Biome closestBiome = null;
        foreach (BiomePoint biomePoint in biomePoints)
        {
            float dist = (biomePoint.pos - pos).magnitude;
            if (minDist == -1 || dist < minDist)
            {
                minDist = dist;
                biomes.TryGetValue((BiomeType)biomePoint.biome, out closestBiome);
            }
        }

        return closestBiome;
    }

    public void GenerateBiomes()
    {
        biomePoints = new BiomePoint[600];
        for(int i = 0; i < 600; i ++)
        {
            Vector3 randPoint = Random.insideUnitSphere * 1000.0f;

            if (i % 3 == 0)
            {
                //this.biomes.TryGetValue(BiomeType.MOUNTAINS, out biome);
                biomePoints[i] = GetBiomePoint(randPoint, BiomeType.MOUNTAINS);
            } else if(i % 3 == 1)
            {
                biomePoints[i] = GetBiomePoint(randPoint, BiomeType.PLAINS);
            } else
            {
                biomePoints[i] = GetBiomePoint(randPoint, BiomeType.ROCKY_HILLS);
            }
        }
    }

}
