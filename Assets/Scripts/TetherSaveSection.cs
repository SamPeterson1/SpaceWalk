using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TetherSaveSection : SaveSection
{
    public static readonly byte tetherIdentifier = 2;
    public TetherNetwork tetherNetwork;

    struct NodeData
    {
        public Vector3 pos;
        public Vector3 supplierPos;
        public bool hasSupplier;
    }

    public TetherSaveSection()
    {
        identifier = tetherIdentifier;
    }

    private void WriteNode(TetherNode node, BinaryWriter writer)
    {
        writer.Write(node.supplier != null);
        Vector3 position = node.GetPos();
        writer.Write(position.x);
        writer.Write(position.y);
        writer.Write(position.z);
        if(node.supplier != null)
        {
            Vector3 connectedPos = node.supplier.GetPos();
            writer.Write(connectedPos.x);
            writer.Write(connectedPos.y);
            writer.Write(connectedPos.z);
        }
        writer.Write(node.isSupplier);
    }

    //bool: has supplier
    //float: x pos
    //float: y pos
    //float: z pos
    //float: supplier x pos (if has supplier)
    //float: supplier y pos (if has supplier)
    //float: supplier z pos (if has supplier)
    //bool: is supplier

    protected override void ReadData(BinaryReader reader)
    {
        Dictionary<Vector3, TetherNode> nodesMap = new Dictionary<Vector3, TetherNode>();

        int numTethers = reader.ReadInt32();
        Debug.Log("NUM TETHERS: " + numTethers);
        NodeData[] nodeData = new NodeData[numTethers];
        for (int i = 0; i < numTethers; i ++)
        {
            NodeData data;
            bool hasSupplier = reader.ReadBoolean();
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            data.pos = new Vector3(x, y, z);
            Debug.Log(data.pos);
            if (hasSupplier)
            {
                x = reader.ReadSingle();
                y = reader.ReadSingle();
                z = reader.ReadSingle();
                data.supplierPos = new Vector3(x, y, z);
                data.hasSupplier = true;
            } else
            {
                data.supplierPos = Vector3.zero;
                data.hasSupplier = false;
            }

            TetherNode node = new TetherNode(null);
            node.loaded = false;
            node.tetherPos = data.pos;
            node.isSupplier = reader.ReadBoolean();

            nodesMap.Add(data.pos, node);
            nodeData[i] = data;
        }

        List<TetherNode> loadedNodes = new List<TetherNode>();
        foreach (NodeData data in nodeData)
        {
            nodesMap.TryGetValue(data.pos, out TetherNode node);
            if (data.hasSupplier)
            {
                nodesMap.TryGetValue(data.supplierPos, out TetherNode supplierNode);
                if(node != null && supplierNode != null)
                {
                    node.supplier = supplierNode;
                }
            }
            loadedNodes.Add(node);
        }

        Debug.Log(loadedNodes.Count);
        tetherNetwork.unloadedNodes = loadedNodes;
        tetherNetwork.loadedNodes = new List<TetherNode>();
    }

    protected override void WriteData(BinaryWriter writer)
    {
        writer.Write(tetherNetwork.loadedNodes.Count + tetherNetwork.unloadedNodes.Count);
        foreach(TetherNode node in tetherNetwork.loadedNodes)
        {
            WriteNode(node, writer);
        }
        foreach(TetherNode node in tetherNetwork.unloadedNodes)
        {
            WriteNode(node, writer);
        }
    }
}
