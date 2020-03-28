using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public Dictionary<ResourceType, ResourceSettings> resources;
    public TextAsset resourcesJSON;
    public TerrainGenerator terrainGenerator;
    public BiomeGenerator biomeGenerator;
    public Player player;
    private Dictionary<Vector3, List<ResourceData>> unloadedResources;
    private Dictionary<Vector3, List<ResourceUnit>> loadedResources;
    private List<Vector3> generatedChunks = new List<Vector3>();
    private bool needsUpdate = false;

    public struct ResourceData
    {
        public Vector3 pos;
        public Quaternion rotation;
        public Vector3 scale;
        public ResourceType type;
    }

    void Start()
    {
        terrainGenerator = GameObject.FindGameObjectWithTag("Generator").GetComponent<TerrainGenerator>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        biomeGenerator = terrainGenerator.shape.biomeGenerator;
        resources = new Dictionary<ResourceType, ResourceSettings>();
        unloadedResources = new Dictionary<Vector3, List<ResourceData>>();
        loadedResources = new Dictionary<Vector3, List<ResourceUnit>>();
        foreach (ResourceSettings setting in LoadResourcesFromFile())
        {
            resources.Add(setting.type, setting);
        }
    }

    private void LateUpdate()
    {
        if(player.changedChunks())
        {
            needsUpdate = true;
        }
        if (needsUpdate && player.getDeltaChunk() == Vector3.zero && terrainGenerator.DoneLoading())
        {
            needsUpdate = false;
            Vector3 off = TerrainChunk.GetChunkFromPos(player.transform.position);
            for (int x = -2 + (int)off.x; x <= 2 + off.x; x++)
            {
                for (int y = -2 + (int)off.y; y <= 2 + off.y; y++)
                {
                    for (int z = -2 + (int)off.z; z <= 2 + off.z; z++)
                    {
                        Vector3 pos = new Vector3(x, y, z);
                        if (!generatedChunks.Contains(pos))
                        {
                            if (terrainGenerator.HasTerrain(pos))
                            {
                                SpawnResourceInChunk(pos);
                                generatedChunks.Add(pos);
                            }
                        }
                    }
                }
            }
            LoadResources();
            UnloadResources();
        }     
    }

    ResourceType RandomType(List<float> weights, List<ResourceType> types, float spawnRate)
    {
        float[] maxVals = new float[weights.Count];
        float randRange = 10.0f;
        float lastMax = 0;
        for(int i = 0; i < weights.Count; i ++)
        {
            float probability = weights[i] * spawnRate;
            maxVals[i] = randRange * probability + lastMax;
            lastMax += maxVals[i];
            Debug.Log(maxVals[i] + " " + weights[i] + " " + spawnRate);
        }

        float randFloat = Random.Range(0.0f, randRange);
        for (int i = 0; i < maxVals.Length; i++)
        {
            if (randFloat < maxVals[i] && (i == 0 || randFloat > maxVals[i-1])) return types[i];
        }

        return ResourceType.NONE;
    }

    void LoadResources()
    {
        List<Vector3> loaded = new List<Vector3>();
        foreach (Vector3 chunkPos in unloadedResources.Keys)
        {
            if (InRange(chunkPos))
            {
                unloadedResources.TryGetValue(chunkPos, out List<ResourceData> data);
                List<ResourceUnit> resourceUnits = new List<ResourceUnit>();
                List<ResourceData> invalid = new List<ResourceData>();
                foreach (ResourceData resourceData in data)
                {
                    if (terrainGenerator.InTerrain(resourceData.pos, 0f)) {
                        resources.TryGetValue(resourceData.type, out ResourceSettings settings);
                        GameObject resourcePrefab = (GameObject)Resources.Load("Prefabs/" + settings.resourcePrefab);
                        Debug.Log((settings == null) + " UH OH");
                        ResourceUnit unit = Instantiate(resourcePrefab, resourceData.pos, resourceData.rotation).GetComponent<ResourceUnit>();
                        unit.transform.localScale = resourceData.scale;
                        unit.type = settings.type;
                        resourceUnits.Add(unit);
                    } else
                    {
                        invalid.Add(resourceData);
                    }
                }

                foreach(ResourceData resourceData in invalid)
                {
                    data.Remove(resourceData);
                }

                loaded.Add(chunkPos);
                if (!loadedResources.ContainsKey(chunkPos))
                {
                    loadedResources.Add(chunkPos, resourceUnits);
                } else
                {
                    loadedResources.TryGetValue(chunkPos, out List<ResourceUnit> units);
                    foreach(ResourceUnit unit in resourceUnits)
                    {
                        units.Add(unit);
                    }
                }
            }
        }

        foreach (Vector3 remove in loaded)
        {
            unloadedResources.Remove(remove);
        }
    }

    void UnloadResources()
    {
        List<Vector3> removed = new List<Vector3>();
        foreach (Vector3 chunkPos in loadedResources.Keys)
        {
            if (!InRange(chunkPos))
            {
                loadedResources.TryGetValue(chunkPos, out List<ResourceUnit> units);
                List<ResourceData> resourceData = new List<ResourceData>();
                foreach (ResourceUnit unit in units)
                {
                    if (unit != null)
                    {
                        resourceData.Add(GetDataFromUnit(unit));
                        Destroy(unit.gameObject);
                    }
                }
                removed.Add(chunkPos);
                if (!unloadedResources.ContainsKey(chunkPos))
                {
                    unloadedResources.Add(chunkPos, resourceData);
                }
                else
                {
                    unloadedResources.TryGetValue(chunkPos, out List<ResourceData> dataList);
                    foreach (ResourceData data in resourceData)
                    {
                        dataList.Add(data);
                    }
                }
            }
        }

        foreach (Vector3 remove in removed)
        {
            loadedResources.Remove(remove);
        }
    }

    bool InRange(Vector3 chunkPos)
    {
        Vector3 displacement = chunkPos - TerrainChunk.GetChunkFromPos(player.transform.position);
        return !(Mathf.Abs(displacement.x) >= 3 || Mathf.Abs(displacement.y) >= 3 || Mathf.Abs(displacement.z) >= 3);
    }

    List<ResourceSettings> LoadResourcesFromFile()
    {
        string text = resourcesJSON.text;
        text = text.Replace("\n", "");
        string[] array = text.Split(new string[] { "RESOURCE" }, System.StringSplitOptions.RemoveEmptyEntries);
        List<ResourceSettings> resources = new List<ResourceSettings>();
        foreach (string str in array)
        {
            string toBiome = str.Replace(" ", "");
            resources.Add(JsonUtility.FromJson<ResourceSettings>(toBiome));
        }

        return resources;
    }

    void SpawnResourceInChunk(Vector3 chunkPos)
    {
        if (!generatedChunks.Contains(chunkPos) && terrainGenerator.ChunkExistsAtChunkPos(chunkPos))
        {
            SpawnResource(terrainGenerator.RandomSurfacePoint(chunkPos));
        }
    }

    void SpawnResource(Vector3 pos)
    {
        Biome biome = biomeGenerator.GetBiomeAt(pos);
        Debug.Log(biome.type);
        ResourceType resourceType = RandomType(biome.individualSpawnRates, biome.resources, biome.resourceSpawnRate);
        if (resourceType != ResourceType.NONE)
        {
            InitResource(pos, resourceType);
        }
    }

    void InitResource(Vector3 pos, ResourceType type)
    {
        resources.TryGetValue(type, out ResourceSettings settings);
        for (int i = 0; i < settings.count; i++)
        {
            Vector3 randPoint = Random.insideUnitSphere;
            randPoint *= settings.radius;
            ResourceData data;
            data.pos = randPoint + pos;
            data.type = settings.type;
            data.rotation = Random.rotation;
            data.scale = Random.Range(settings.minScale, settings.maxScale) * Vector3.one;
            Vector3 chunkPos = TerrainChunk.GetChunkFromPos(data.pos);
            if (unloadedResources.ContainsKey(chunkPos))
            {
                unloadedResources.TryGetValue(chunkPos, out List<ResourceData> dataList);
                dataList.Add(data);
            }
            else
            {
                List<ResourceData> dataList = new List<ResourceData>() { data };
                unloadedResources.Add(chunkPos, dataList);
            }
        }
    }

    ResourceData GetDataFromUnit(ResourceUnit unit)
    {
        ResourceData retVal;
        retVal.pos = unit.transform.position;
        retVal.rotation = unit.transform.rotation;
        retVal.type = unit.type;
        retVal.scale = unit.transform.localScale;
        return retVal;
    }

    public void SetData(List<ResourceData> data)
    {
        Debug.Log("FOO)!>");
        needsUpdate = true;

        foreach (List<ResourceUnit> units in loadedResources.Values)
        {
            foreach (ResourceUnit unit in units)
            {
                if (unit != null)
                {
                    Destroy(unit.gameObject);
                }
            }
        }

        loadedResources = new Dictionary<Vector3, List<ResourceUnit>>();
        unloadedResources = new Dictionary<Vector3, List<ResourceData>>();

        foreach (ResourceData resourceData in data)
        {
            Vector3 chunkPos = TerrainChunk.GetChunkFromPos(resourceData.pos);
            if (unloadedResources.ContainsKey(chunkPos))
            {
                unloadedResources.TryGetValue(chunkPos, out List<ResourceData> append);
                append.Add(resourceData);
            }
            else
            {
                unloadedResources.Add(chunkPos, new List<ResourceData>() { resourceData });
            }
        }

        foreach (ResourceData resourceData in data)
        {
            Vector3 chunkPos = TerrainChunk.GetChunkFromPos(resourceData.pos);
            if(unloadedResources.ContainsKey(chunkPos))
            {
                unloadedResources.TryGetValue(chunkPos, out List<ResourceData> append);
                append.Add(resourceData);
            } else
            {
                unloadedResources.Add(chunkPos, new List<ResourceData>() { resourceData });
            }
        }
    }

    public List<ResourceData> GetAllResourceData()
    {
        List<ResourceData> resourceData = new List<ResourceData>();
        foreach(List<ResourceData> data in unloadedResources.Values)
        {
            resourceData.AddRange(data);
        }
        foreach(List<ResourceUnit> resourceUnits in loadedResources.Values)
        {
            foreach(ResourceUnit unit in resourceUnits)
            {
                if (unit != null)
                {
                    resourceData.Add(GetDataFromUnit(unit));
                }
            }
        }

        return resourceData;
    }

}
