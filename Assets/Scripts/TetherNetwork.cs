using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetherNetwork : MonoBehaviour
{
    public struct TetherData
    {
        public Vector3 pos;
        public bool isSupplier;
        public bool tempSupplier;
    }

    public Dictionary<Vector3, List<Tether>> tethers;
    public List<TetherNode> loadedNodes = new List<TetherNode>();
    public List<TetherNode> unloadedNodes = new List<TetherNode>();
    public Player player;
    public TerrainGenerator generator;
    public GameObject tetherPrefab;
    public GameObject connectionPrefab;
    public bool updated = false;
    public bool updatingOxygen = false;
    public int loadRange;
    public float connectDist;

    float Dist(TetherNode a, TetherNode b)
    {
        Vector3 posA = a.tetherObject.transform.position;
        Vector3 posB = b.tetherObject.transform.position;
        return (posA - posB).magnitude;
    }

    public bool InRange(TetherNode a, TetherNode b)
    {
        return (Dist(a, b) < connectDist);
    }

    
    public void PlaceTether(Vector3 position, bool makeSupplier)
    {
        GameObject tetherObject = Instantiate(tetherPrefab, position, Quaternion.identity);
        TetherNode node = tetherObject.GetComponent<Tether>().CreateNode();
        node.isSupplier = makeSupplier;
        loadedNodes.Add(node);
        updated = true;
    }

    public void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        generator = GameObject.FindGameObjectWithTag("Generator").GetComponent<TerrainGenerator>();
        tethers = new Dictionary<Vector3, List<Tether>>();

        SaveManager saveManager = GameObject.FindGameObjectWithTag("SaveManager").GetComponent<SaveManager>();
        TetherSaveSection tetherSaveSection = saveManager.GetSaveSection(TetherSaveSection.tetherIdentifier) as TetherSaveSection;
        tetherSaveSection.tetherNetwork = this;
    }

    public void Update()
    {

        foreach(TetherNode tetherNode in loadedNodes)
        {
            tetherNode.UpdateHasOxygen();
        }

        if(!updatingOxygen)
        {
            foreach(TetherNode tetherNode in loadedNodes)
            {
                tetherNode.UpdateConnections();
            }
        }

        updatingOxygen = false;

        if (generator.DoneLoading())
        {
            UnloadTethers();
            LoadTethers();
        }
    }

    void UnloadTethers()
    {
        List<TetherNode> unloaded = new List<TetherNode>();
        foreach(TetherNode tetherNode in loadedNodes)
        {
            Vector3 chunkPos = TerrainChunk.GetChunkFromPos(tetherNode.tetherPos);
            if (!InRange(chunkPos))
            {
                unloaded.Add(tetherNode);
                unloadedNodes.Add(tetherNode);
                tetherNode.Unload();
            }
        }

        foreach (TetherNode tetherNode in unloaded)
        {
            loadedNodes.Remove(tetherNode);
        }
    }

    void LoadTethers()
    {
        List<TetherNode> loaded = new List<TetherNode>();
        foreach(TetherNode tetherNode in unloadedNodes)
        {
            Vector3 chunkPos = TerrainChunk.GetChunkFromPos(tetherNode.tetherPos);
            if (InRange(chunkPos))
            {
                loaded.Add(tetherNode);
                loadedNodes.Add(tetherNode);
                tetherNode.Reload();
            }
        }

        foreach (TetherNode tetherNode in loaded)
        {
            unloadedNodes.Remove(tetherNode);
        }

        if (loaded.Count > 0)
        {
            foreach (TetherNode tetherNode in loadedNodes)
            {
                tetherNode.RemakeConnections();
            }
        }
    }

    bool InRange(Vector3 chunkPos)
    {
        Vector3 displacement = chunkPos - TerrainChunk.GetChunkFromPos(player.transform.position);
        return Mathf.Abs(displacement.x) < loadRange && Mathf.Abs(displacement.y) < loadRange && Mathf.Abs(displacement.z) < loadRange;
    }

    /*
    public void AddTether(Tether tether, Vector3 chunkPos)
    {
        if(tethers.ContainsKey(chunkPos))
        {
            tethers.TryGetValue(chunkPos, out List<Tether> tethersInChunk);
            tethersInChunk.Add(tether);
        } else
        {
            List<Tether> tethersInChunk = new List<Tether>();
            tethersInChunk.Add(tether);
            tethers.Add(chunkPos, tethersInChunk);
        }
    }

    public void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        generator = GameObject.FindGameObjectWithTag("Generator").GetComponent<TerrainGenerator>();
    }

    public void Update()
    {
        Debug.Log("SUM: " + (unloadedTethers.Keys.Count + tethers.Keys.Count));
        if (!updatingOxygen)
        {
            updated = false;
        }
        updatingOxygen = false;
        List<Vector3> toUnload = new List<Vector3>();
        foreach(Vector3 key in tethers.Keys)
        {
            if (!InRange(key))
            {
                toUnload.Add(key);
            }
        }

        foreach(Vector3 chunk in toUnload)
        {
            UnloadTethersInChunk(chunk);
        }

        List<Vector3> toLoad = new List<Vector3>();
        foreach (Vector3 key in unloadedTethers.Keys)
        {
            if (generator.DoneLoading() && InRange(key))
            {
                toLoad.Add(key);
            }
        }

        foreach(Vector3 chunk in toLoad)
        {
            LoadTethersInChunk(chunk);
        }
    }

    bool InRange(Vector3 chunkPos)
    {
        Vector3 displacement = chunkPos - TerrainChunk.getChunkFromPos(player.transform.position);
        return Mathf.Abs(displacement.x) < loadRange && Mathf.Abs(displacement.y) < loadRange && Mathf.Abs(displacement.z) < loadRange;
    }

    public void CreateTether(TetherData data)
    {
        Tether tetherComponent = Instantiate(tetherPrefab, data.pos, Quaternion.identity).GetComponent<Tether>();
        tetherComponent.hasOxygen = false;
        tetherComponent.firstTether = false;
        tetherComponent.supplier = null;
        if (data.isSupplier)
        {
            tetherComponent.MakeSupplier();
        }
        tetherComponent.tempSupplier = data.tempSupplier;
    }

    public void LoadTethersInChunk(Vector3 chunkPos)
    {
        if (!unloadedTethers.ContainsKey(chunkPos)) return;
        updated = true;
        unloadedTethers.TryGetValue(chunkPos, out List<TetherData> tethersInChunk);
        foreach (TetherData tether in tethersInChunk)
        {
            Tether tetherComponent = Instantiate(tetherPrefab, tether.pos, Quaternion.identity).GetComponent<Tether>();
            tetherComponent.hasOxygen = false;
            tetherComponent.firstTether = false;
            tetherComponent.supplier = null;
            if (tether.isSupplier)
            {
                tetherComponent.MakeSupplier();
            }
            tetherComponent.tempSupplier = tether.tempSupplier;
        }

        unloadedTethers.Remove(chunkPos);
    }

    public void UnloadTethersInChunk(Vector3 chunkPos)
    {
        tethers.TryGetValue(chunkPos, out List<Tether> tethersInChunk);
        if (tethersInChunk != null)
        {
            updated = true;
            List<TetherData> dataList = new List<TetherData>();
            foreach (Tether tether in tethersInChunk)
            {
                TetherData data;
                data.tempSupplier = tether.tempSupplier;
                foreach (Tether tetherInNextchunk in tether.supplies)
                {
                    if (tetherInNextchunk != null)
                    {
                        if (TerrainChunk.getChunkFromPos(tetherInNextchunk.transform.position) != TerrainChunk.getChunkFromPos(tether.transform.position))
                        {
                            tetherInNextchunk.tempSupplier = true;
                            break;
                        }
                    }
                }
                data.pos = tether.transform.position;
                data.isSupplier = tether.firstTether;
                
                dataList.Add(data);
                Destroy(tether.gameObject);
            }

            tethers.Remove(chunkPos);
            if (unloadedTethers.ContainsKey(chunkPos))
            {
                unloadedTethers.Remove(chunkPos);
            }

            unloadedTethers.Add(chunkPos, dataList);
        }
    }

    public void Load(TetherData[] data)
    {
        List<Vector3> removeMe = new List<Vector3>();
        foreach(Vector3 position in tethers.Keys)
        {
            removeMe.Add(position);
        }
        foreach(Vector3 position in removeMe)
        {
            UnloadTethersInChunk(position);
        }
        tethers = new Dictionary<Vector3, List<Tether>>();
        unloadedTethers = new Dictionary<Vector3, List<TetherData>>();
        foreach(TetherData tetherData in data)
        {
            CreateTether(tetherData);
            Debug.Log("HI");
        }
    }

    public void Save()
    {
        List<TetherData> positions = new List<TetherData>();
        foreach(List<Tether> tethersInChunk in tethers.Values)
        {
            foreach(Tether tether in tethersInChunk)
            {
                TetherData tetherData;
                tetherData.pos = tether.transform.position;
                tetherData.isSupplier = tether.firstTether;
                tetherData.tempSupplier = false;
                positions.Add(tetherData);
            }
        }
        foreach (List<TetherData> tethersInChunk in unloadedTethers.Values)
        {
            foreach (TetherData tether in tethersInChunk)
            {
                positions.Add(tether);
            }
        }
        ChunkIO.WriteTetherData(positions);
    }
    */
}
