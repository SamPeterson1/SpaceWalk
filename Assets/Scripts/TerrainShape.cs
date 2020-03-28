using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainShape
{
    readonly Noise noise = new Noise();

    public NoiseSettings settings;
    public ComputeShader shader;
    public BiomeGenerator biomeGenerator;

    public struct DensityPoint
    {
        public float density;
        public Vector3 color;
    }

    public TerrainShape(TextAsset biomesJSON)
    {
        biomeGenerator = new BiomeGenerator(biomesJSON);
        biomeGenerator.GenerateBiomes();
    }

    public void GetDensities(Vector3 offset, out float[] densities, out Vector3[] colors)
    {
        int kernelHandle = shader.FindKernel("CSMain");

        ComputeBuffer densitiesBuffer = new ComputeBuffer(40 * 40 * 40, sizeof(float) * 4);

        ComputeBuffer biomesBuffer = new ComputeBuffer(biomeGenerator.biomePoints.Length, sizeof(int) + sizeof(float) * 12);
        biomesBuffer.SetData(biomeGenerator.biomePoints);

        shader.SetBuffer(kernelHandle, "biomes", biomesBuffer);
        shader.SetBuffer(kernelHandle, "densities", densitiesBuffer);

        shader.SetFloat("xOff", offset.x);
        shader.SetFloat("yOff", offset.y);
        shader.SetFloat("zOff", offset.z);

        LoadSettingsToGPU();
        shader.Dispatch(kernelHandle, 5, 5, 5);

        biomesBuffer.Dispose();
        DensityPoint[] densityPoints = new DensityPoint[40 * 40 * 40];
        densitiesBuffer.GetData(densityPoints);
        densitiesBuffer.Dispose();

        densities = new float[40 * 40 * 40];
        colors = new Vector3[40 * 40 * 40];

        for (int i = 0; i < densityPoints.Length; i++)
        {
            densities[i] = densityPoints[i].density;
            colors[i] = densityPoints[i].color;
        }
    }

    private void LoadSettingsToGPU()
    {
        shader.SetFloat("roughness", settings.roughness);
        shader.SetFloat("persistence", settings.persistence);
        shader.SetFloat("baseRoughness", settings.baseRoughness);
        shader.SetFloat("amplitude", settings.amplitude);
        shader.SetFloat("numLayers", settings.numLayers);
        shader.SetFloat("minRadius", settings.minRadius);
    }

    public float GetDensity(float x, float y, float z)
    {
        Vector3 toCenter = new Vector3(x, y, z);

        float radius = noise.Evaluate(toCenter.normalized * 1000f / 40f)*9f + 1000f;
        return toCenter.magnitude - radius;
    }
}
