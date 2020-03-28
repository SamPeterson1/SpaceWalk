using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    SaveFile saveFile;
    public bool justLoaded;

    void Awake()
    {
        saveFile = new SaveFile("C:/Save/saveData.dat");
    }

    // Update is called once per frame
    void Update()
    {
        justLoaded = false;
        if(Input.GetKeyDown(KeyCode.P))
        {
            (GetSaveSection(ChunkSaveSection.chunkIdentifier) as ChunkSaveSection).deformations = TerrainChunk.pastData;
            saveFile.WriteData();
        } else if(Input.GetKeyDown(KeyCode.L))
        {
            saveFile.ReadData();
            justLoaded = true;
        }
    }

    public SaveSection GetSaveSection(byte identifier)
    {
        return saveFile.GetSaveSection(identifier);
    }
}
