using UnityEngine;


public class SimplexNoise
{
    private int _width, _height, _depth, _seed;
    private float magnitude = 1000f;
    private CaveGenerator _caveGenerator;
    FastNoiseLite fastNoise;
    FastNoiseLite fastNoise1;

    public SimplexNoise(int width, int height, int depth, int seed)
    {
        _width = width;
        _height = height;
        _depth = depth;
        _seed = seed;
        _caveGenerator = CaveGenerator.Instance;

        fastNoise = new FastNoiseLite();
        fastNoise1 = new FastNoiseLite();
        fastNoise.SetSeed(_seed);
        fastNoise.SetSeed(_seed + 1);
    }

    public void GenerateNoise()
    {
        float[,,] noiseGrid = new float[_width, _height, _depth];


        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int z = 0; z < _depth; z++)
                {

                    float noise = fastNoise.GetNoise((float)x * magnitude, (float)y * magnitude, (float)z * magnitude);
                    float noise1 = fastNoise1.GetNoise((float)x * magnitude, (float)y * magnitude, (float)z * magnitude);

                    //blend noise
                    float blendFactor = 0.5f;
                    float combinedNoise = Mathf.Lerp(noise, noise1, blendFactor);

                    combinedNoise += 1f;
                    combinedNoise /= 2f;

                    //noiseGrid[x, y, z] = _caveGenerator.caveGrid[x, y, z] * combinedNoise;
                    _caveGenerator.caveGrid[x, y, z] *= combinedNoise;
                    /*
                    bool positive = (_caveGenerator.caveGrid[x, y, z] + combinedNoise > 0);
                    if (_caveGenerator.caveGrid[x, y, z] < 0 && positive)
                    {
                        continue;
                    }
                    if (_caveGenerator.caveGrid[x, y, z] > 0 && !positive)
                    {
                        continue;
                    }
                    */
                }
            }
        }
    }

    public float GetNoise(int x, int y, int z)
    {
        float noise = fastNoise.GetNoise((float)x * magnitude, (float)y * magnitude, (float)z * magnitude);
        float noise1 = fastNoise1.GetNoise((float)x * magnitude, (float)y * magnitude, (float)z * magnitude);

        //blend noise
        float blendFactor = 0.5f;
        float combinedNoise = Mathf.Lerp(noise, noise1, blendFactor);

        combinedNoise += 1f;
        combinedNoise /= 2f;

        return combinedNoise;
    }
}

