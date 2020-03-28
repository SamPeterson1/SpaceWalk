using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunShader
{
    public ComputeShader shader;
    private static int FLOAT_BYTES = 4;
    
    public struct Triangle
    {
        public Vector3 pointA;
        public Vector3 pointB;
        public Vector3 pointC;
        public Vector3 color;
    };

    public RunShader(ComputeShader shader)
    {
        this.shader = shader;
    }

    public Triangle[] run(float[] densities, Vector3[] colors, Vector3 offset)
    {

        int kernelHandle = shader.FindKernel("testing");
        ComputeBuffer densitiesBuffer = new ComputeBuffer(densities.Length, 4);
        densitiesBuffer.SetData(densities);
        
        ComputeBuffer trianglesBuffer = new ComputeBuffer(40 * 40 * 40 * 5, FLOAT_BYTES * 12, ComputeBufferType.Append);
        trianglesBuffer.SetCounterValue(0);

        ComputeBuffer colorsBuffer = new ComputeBuffer(40 * 40 * 40, sizeof(float) * 3);
        colorsBuffer.SetData(colors);

        shader.SetBuffer(kernelHandle, "densities", densitiesBuffer);
        shader.SetBuffer(kernelHandle, "triangles", trianglesBuffer);
        shader.SetBuffer(kernelHandle, "colors", colorsBuffer);

        shader.SetFloats("offset", new float[] { offset.x, offset.y, offset.z });
        shader.Dispatch(kernelHandle, 5, 5, 5);
        ComputeBuffer triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        int[] triCountArray = { 0 };

        triCountBuffer.SetData(triCountArray);
        ComputeBuffer.CopyCount(trianglesBuffer, triCountBuffer, 0);
        triCountBuffer.GetData(triCountArray);

        Triangle[] triangles = new Triangle[triCountArray[0]];
        trianglesBuffer.GetData(triangles);

        trianglesBuffer.Dispose();
        triCountBuffer.Dispose();
        densitiesBuffer.Dispose();
        colorsBuffer.Dispose();

        return triangles;
    }

}
