using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ASyncChunkLoader
{
    TerrainChunk[] needUpdate;
    void LoadChunksASync()
    {
        foreach(TerrainChunk chunk in needUpdate)
        {
            chunk.Reload();
        }
    }

    public void LoadChunks(TerrainChunk[] needUpdate)
    {
        this.needUpdate = needUpdate;
        ThreadStart start = new ThreadStart(LoadChunksASync);
        Thread thread = new Thread(start);
        thread.Start();
    }
}
