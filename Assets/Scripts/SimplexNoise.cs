using System.Collections;
using UnityEngine;

public class SimplexNoise
{
    private int _width, _height, _depth, _seed;
    private float magnitude = 4000f;
    private CaveGenerator _caveGenerator;
    private FastNoiseLite _fastNoise;

    public ComputeShader computeShader;
    private int _kernelIdx;
    private ComputeBuffer _noiseBuffer;

    public SimplexNoise(int width, int height, int depth, int seed, ComputeShader noiseComputeShader)
    {
        _width = width;
        _height = height;
        _depth = depth;
        _seed = seed;
        _caveGenerator = CaveGenerator.Instance;

        _fastNoise = new FastNoiseLite();
        _fastNoise.SetSeed(_seed);
        _fastNoise.SetFrequency(0.0001f);

        computeShader = noiseComputeShader;
        _kernelIdx = computeShader.FindKernel("CSSimplex");
    }

    public IEnumerator GenerateNoise()
    {
        _noiseBuffer = new ComputeBuffer((_width + 1) * (_depth + 1) * (_height + 1), sizeof(float));
        _noiseBuffer.SetData(CaveGenerator.Instance.caveGrid);
        computeShader.SetBuffer(_kernelIdx, "noiseBuffer", _noiseBuffer);
        computeShader.SetInt("seed", _seed);
        computeShader.SetInt("seed1", _seed + 1);
        computeShader.SetInt("size", _width);

        CaveGenerator.Instance.generateProgress = 0.05825f;
        yield return null;

        computeShader.Dispatch(_kernelIdx, _width / 8, _depth / 8, _height / 8);

        _noiseBuffer.GetData(CaveGenerator.Instance.caveGrid);

        if (_noiseBuffer != null)
        {
            _noiseBuffer.Release();
        }

        CaveGenerator.Instance.generateProgress = 0.075f;
        yield return null;
    }
    private void OnDestroy()
    {
        if (_noiseBuffer != null)
        {
            _noiseBuffer.Release();
            _noiseBuffer = null;
        }
    }

    public float GetNoise(int x, int y, int z)
    {
        float noise = _fastNoise.GetNoise(x * magnitude, y * magnitude, z * magnitude);

        noise *= 25/23;
        noise += 1;
        noise /= 2;

        return noise;
    }


    /*
    The code below are simplex noise without using compute shader
    Uncomment to use for testing

    public void GenerateNoiseNoCS()
    {
        FastNoiseLite fastNoise = new FastNoiseLite();
        FastNoiseLite fastNoise1 = new FastNoiseLite();
        FastNoiseLite fastNoise2 = new FastNoiseLite();
        FastNoiseLite fastNoise3 = new FastNoiseLite();
        fastNoise.SetSeed(_seed);
        fastNoise1.SetSeed(_seed);
        fastNoise2.SetSeed(_seed);
        fastNoise3.SetSeed(_seed);
        fastNoise.SetFrequency(0.0025f);
        fastNoise1.SetFrequency(0.005f);
        fastNoise2.SetFrequency(0.01f);
        fastNoise3.SetFrequency(0.02f);

        float min = 100000f;
        float max = -1000000f;
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int z = 0; z < _depth; z++)
                {
                    float n1 = fastNoise.GetNoise(x * magnitude, y * magnitude, z * magnitude);
                    float n2 = fastNoise1.GetNoise(x * magnitude / 2, y * magnitude / 2, z * magnitude/ 2);
                    float n3 = fastNoise2.GetNoise(x * magnitude / 4, y * magnitude / 4, z * magnitude / 4);
                    float n4 = fastNoise3.GetNoise(x * magnitude / 8, y * magnitude / 8, z * magnitude / 8);

                    float combinedNoise = n1 + n2 + n3 + n4;
                    combinedNoise /= 3.6f;
                    combinedNoise += 1;
                    combinedNoise /= 2;

                    if (combinedNoise < min) { min = combinedNoise; }
                    if (combinedNoise > max) { max = combinedNoise; }

                    _caveGenerator.MultiplyCave(x, y, z, combinedNoise);

                }
            }
        }
        Debug.Log("min" + min);
        Debug.Log("max " + max);
    }
    */
}

