using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class NoiseUtility
{
    public static float OctavePerlin(float x, float z, NoiseSettings settings)
    {
        x = x * settings.zoom + settings.zoomOffset;
        z = z * settings.zoom + settings.zoomOffset;

        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float amplitudeSum = 0; 
        for (int i = 0; i < settings.octaves; i++)
        {
            // perlin noise returns the same output for all int values, so add some small offset to make sure the input we give is float
            total += Mathf.PerlinNoise((settings.noiseOffset + x) * frequency, (settings.noiseOffset + z) * frequency)
                //amplitude determines how much influence each layer noise has on the final noise.
                * amplitude;
            amplitudeSum += amplitude;
            // noise values change over space, which leads to different patterns or levels of detail
            // that we are stacking on one another
            frequency *= 2;
            amplitude *= settings.amplitudeMultiplier;
        }
        // normalize result
        return total / amplitudeSum;
    }

    // Make the noise non-linear to create natural looking results
    public static float Redistribution(float noise, NoiseSettings settings)
    {
        //for smoothness: Greater than 1:  makes the values more extreme (pushing higher values further up and pulling lower values down), increasing the contrast of the noise.
        //Less than 1: It flattens the values, making the transition between highs and lows smoother.
        // for intensity:Without redistribution, the noise values generated by Perlin noise might be too
        // close to each other in range (e.g., between 0 and 1), which might not be suitable for some use
        // cases like terrain generation, where you might need higher or lower values in certain areas.

        return Mathf.Pow(noise *settings.intensity, settings.smoothness);
    }
    public static int NormalizeToChunkHeight(float n)
    {
        return (int)(n * EnvironmentConstants.chunkHeight);
    }

    //Normalize from 0 to chunk hights. We are working with voxel units, so the value must be int
    public static int GetNormalizedNoise(Vector2 globalPos, NoiseSettings settings)
    {
        float noise = OctavePerlin(globalPos.x, globalPos.y, settings);
        noise = Redistribution(noise,settings);
        return (int)NormalizeToChunkHeight(noise);
    }

    public static float GetNoise(Vector2 globalPos, NoiseSettings settings)
    {
        float noise = OctavePerlin(globalPos.x, globalPos.y, settings);
        return Redistribution(noise, settings);
    }
}
