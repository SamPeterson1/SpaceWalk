using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ColorGenerator
{

    public float snowLevel;

    public Color GetColor(Vector3 vertex)
    {
        float elevation = Mathf.Abs(vertex.magnitude) - 1000f;
        if(elevation > snowLevel)
        {
            return Color.gray;
        }
        //rgb(255,61,31)
        return new Color(255f/255f, 131f/ 255f, 59f/ 256f);
    }

    public Color[] GenColors(Mesh mesh)
    {
        Color[] colors = new Color[mesh.vertexCount];
        Vector3[] vertices = mesh.vertices;
        for(int i = 0; i < vertices.Length; i ++)
        {
            Vector3 vertex = vertices[i];
            float toCenter = Mathf.Abs(vertex.magnitude) - 1000f;
            if (toCenter < -10f)
            {
                colors[i] = Color.white;
            }
            else
            {
                colors[i] = Color.red;
            }
        }

        return colors;
    }
}
