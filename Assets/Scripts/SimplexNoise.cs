using System.Drawing;
using UnityEngine;


public class SimplexNoise
{
    private int _width, _height, _depth, _seed;
    private float magnitude = 1000f;
    private CaveGenerator _caveGenerator;
    private FastNoiseLite _fastNoise;
    private FastNoiseLite _fastNoise1;

    public ComputeShader computeShader;
    private int _kernelIdx;


    public SimplexNoise(int width, int height, int depth, int seed, ComputeShader noiseComputeShader)
    {
        _width = width;
        _height = height;
        _depth = depth;
        _seed = seed;
        _caveGenerator = CaveGenerator.Instance;

        _fastNoise = new FastNoiseLite();
        _fastNoise1 = new FastNoiseLite();
        _fastNoise.SetSeed(_seed);
        _fastNoise.SetSeed(_seed + 1);

        computeShader = noiseComputeShader;
        _kernelIdx = computeShader.FindKernel("CSSimplex");
    }

    public void GenerateNoiseNoCS()
    {
        float min1 = 100000f;
        float max1 = -1000000f;
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int z = 0; z < _depth; z++)
                {

                    float noise = _fastNoise.GetNoise(x * magnitude, y * magnitude, z * magnitude);
                    float noise1 = _fastNoise1.GetNoise(x * magnitude, y * magnitude, z * magnitude);

                    //blend noise
                    float blendFactor = 0.5f;
                    float combinedNoise = Mathf.Lerp(noise, noise1, blendFactor);
                    //float combinedNoise = (noise + noise1) / 2;

                    if (combinedNoise < min1) { min1 = combinedNoise; }
                    if (combinedNoise > max1) { max1 = combinedNoise; }

                    combinedNoise += 1f;
                    combinedNoise /= 2f;

                    _caveGenerator.MultiplyCave(x, y, z, combinedNoise);

                }
            }
        }
        Debug.Log("min1" + min1);
        Debug.Log("max1 " + max1);

        float min = 100000f;
        float max = -1000000f;
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                for (int k = 0; k < _width; k++)
                {
                    float temp = _caveGenerator.GetCave(i, j, k);
                    if (temp < min) { min = temp; }
                    if (temp > max) { max = temp; }
                }
            }
        }

        Debug.Log("min noise:" + min);
        Debug.Log("max noise:" + max);
    }

    public void GenerateNoise()
    {
        ComputeBuffer _noiseBuffer = new ComputeBuffer((_width + 1) * (_depth + 1) * (_height + 1), sizeof(float));
        _noiseBuffer.SetData(CaveGenerator.Instance.caveGrid);
        computeShader.SetBuffer(_kernelIdx, "noiseBuffer", _noiseBuffer);
        computeShader.SetInt("seed", _seed);
        computeShader.SetInt("seed1", _seed + 1);
        computeShader.SetInt("size", _width);
        computeShader.Dispatch(_kernelIdx, _width / 8, _depth / 8, _height / 8);

        _noiseBuffer.GetData(CaveGenerator.Instance.caveGrid);

        float min = 100000f;
        float max = -1000000f;
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                for (int k = 0; k < _width; k++)
                {
                    float temp = _caveGenerator.GetCave(i, j, k);
                    if (temp < min) { min = temp; }
                    if (temp > max) { max = temp; }
                }
            }
        }

        Debug.Log("min noise:" + min);
        Debug.Log("max noise:" + max);


        if (_noiseBuffer != null)
        {
            _noiseBuffer.Release();
        }
    }

    public float GetNoise(int x, int y, int z)
    {
        float noise = _fastNoise.GetNoise(x * magnitude, y * magnitude, z * magnitude);
        float noise1 = _fastNoise1.GetNoise(x * magnitude, y * magnitude, z * magnitude);

        //blend noise
        //float blendFactor = 0.5f;
        //float combinedNoise = Mathf.Lerp(noise, noise1, blendFactor);
        float combinedNoise = (noise + noise1) / 2;

        combinedNoise += 1f;
        combinedNoise /= 2f;

        Debug.Log("combinedNoise: " + combinedNoise);
        return combinedNoise;
    }
}

