using UnityEngine;


public class SimplexNoise
{
    private int _width, _height, _depth, _seed;
    private float magnitude = 1000f;
    private CaveGenerator _caveGenerator;
    private FastNoiseLite _fastNoise;
    private FastNoiseLite _fastNoise1;

    public SimplexNoise(int width, int height, int depth, int seed)
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
    }

    public void GenerateNoise()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int z = 0; z < _depth; z++)
                {

                    float noise = _fastNoise.GetNoise((float)x * magnitude, (float)y * magnitude, (float)z * magnitude);
                    float noise1 = _fastNoise1.GetNoise((float)x * magnitude, (float)y * magnitude, (float)z * magnitude);

                    //blend noise
                    float blendFactor = 0.5f;
                    float combinedNoise = Mathf.Lerp(noise, noise1, blendFactor);

                    combinedNoise += 1f;
                    combinedNoise /= 2f;

                    _caveGenerator.caveGrid[x, y, z] *= combinedNoise;
                }
            }
        }
    }

    public float GetNoise(int x, int y, int z)
    {
        float noise = _fastNoise.GetNoise((float)x * magnitude, (float)y * magnitude, (float)z * magnitude);
        float noise1 = _fastNoise1.GetNoise((float)x * magnitude, (float)y * magnitude, (float)z * magnitude);

        //blend noise
        float blendFactor = 0.5f;
        float combinedNoise = Mathf.Lerp(noise, noise1, blendFactor);

        combinedNoise += 1f;
        combinedNoise /= 2f;

        return combinedNoise;
    }
}

