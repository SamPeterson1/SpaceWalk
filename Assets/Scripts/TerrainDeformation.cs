using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainDeformation
{
    public Dictionary<int, float> points = new Dictionary<int, float>();

    public TerrainDeformation()
    {

    }

    public bool Modified()
    {
        return points.Keys.Count > 0;
    }

    public TerrainDeformation(Dictionary<int, float> points)
    {
        this.points = points;
    }

    public TerrainDeformation copyOf()
    {
        Dictionary<int, float> clonePoints = new Dictionary<int, float>();
        foreach(int i in points.Keys)
        {
            float val;
            points.TryGetValue(i, out val);
            clonePoints.Add(i, val);
        }

        return new TerrainDeformation(clonePoints);
    }

    public float[] modify(float[] densities)
    {
        foreach(int i in points.Keys)
        {
            float val;
            points.TryGetValue(i, out val);
            if (i >= densities.Length || i < 0)
            {
                Debug.Log(i);
            }
            else
            {
                densities[i] = val;
            }
        }
        return densities;
    }

    public void LogDensity(int index, float value)
    {
        if(points.ContainsKey(index))
        {
            points.Remove(index);
        }

        points.Add(index, value);
    }
}
