using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveFile
{
    Dictionary<byte, SaveSection> sections = new Dictionary<byte, SaveSection>();
    string savePath;
    int numSections = 0;

    public SaveFile(string savePath)
    {
        this.savePath = savePath;
        if (!File.Exists(savePath))
        {
            File.Create(savePath);
        }

        AddSection(new ChunkSaveSection());
        AddSection(new BiomeSaveSection());
        AddSection(new TetherSaveSection());
        AddSection(new ResourceSaveSection());
    }
    
    public void Erase()
    {
        File.Delete(savePath);
        File.Create(savePath);
    }

    public SaveSection GetSaveSection(byte identifier)
    {
        sections.TryGetValue(identifier, out SaveSection retVal);
        return retVal;
    }

    public void ReadData()
    {
        using(BinaryReader reader = new BinaryReader(File.Open(savePath, FileMode.Open)))
        {
            for(int i = 0; i < numSections; i ++)
            {
                byte identifier = reader.ReadByte();
                sections.TryGetValue(identifier, out SaveSection section);
                if(section != null)
                {
                    section.Read(reader);
                } else
                {
                    Debug.LogError("Error while reading file: section not found");
                }
            }
        }
    }

    public void WriteData()
    {
        using(BinaryWriter writer = new BinaryWriter(File.Open(savePath, FileMode.Open)))
        {
            foreach(SaveSection section in sections.Values)
            {
                section.Write(writer);
            }
        }
    }

    void AddSection(SaveSection section)
    {
        sections.Add(section.identifier, section);
        numSections++;
    }
}
