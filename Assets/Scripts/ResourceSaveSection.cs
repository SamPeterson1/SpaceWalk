using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResourceSaveSection : SaveSection
{
    public static readonly byte resourceIdentifier = 3;
    public ResourceManager resourceManager;

    public ResourceSaveSection()
    {
        identifier = resourceIdentifier;
        resourceManager = GameObject.FindGameObjectWithTag("ResourceManager").GetComponent<ResourceManager>();
    }

    protected override void ReadData(BinaryReader reader)
    {
        int numResources = reader.ReadInt32();
        List<ResourceManager.ResourceData> resources = new List<ResourceManager.ResourceData>();
        for(int i = 0; i < numResources; i ++)
        {
            ResourceManager.ResourceData data;
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            data.pos = new Vector3(x, y, z);

            data.type = (ResourceType)reader.ReadInt32();

            x = reader.ReadSingle();
            y = reader.ReadSingle();
            z = reader.ReadSingle();
            data.rotation = Quaternion.Euler(x, y, z);

            x = reader.ReadSingle();
            y = reader.ReadSingle();
            z = reader.ReadSingle();
            data.scale = new Vector3(x, y, z);

            resources.Add(data);
        }

        resourceManager.SetData(resources);
    }

    protected override void WriteData(BinaryWriter writer)
    {
        List<ResourceManager.ResourceData> resourceData = resourceManager.GetAllResourceData();
        writer.Write(resourceData.Count);
        foreach(ResourceManager.ResourceData data in resourceData)
        {
            writer.Write(data.pos.x);
            writer.Write(data.pos.y);
            writer.Write(data.pos.z);
            writer.Write((int)data.type);
            Vector3 eulerRot = data.rotation.eulerAngles;
            writer.Write(eulerRot.x);
            writer.Write(eulerRot.y);
            writer.Write(eulerRot.z);

            writer.Write(data.scale.x);
            writer.Write(data.scale.y);
            writer.Write(data.scale.z);
        }
    }
}
