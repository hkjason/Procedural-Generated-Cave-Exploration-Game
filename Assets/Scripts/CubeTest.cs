using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeTest : MonoBehaviour
{

    private void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector2[] uvs = new Vector2[mesh.vertexCount];

        Debug.Log(uvs.Length);

        float cellSize = 1.0f / 8.0f;
        //float uMin = x * cellSize;
        //float vMin = y * cellSize;

        for (int i = 0; i < uvs.Length; i +=4)
        {
            //uvs[i] = new Vector2(uMin + i * cellSize / mesh.vertexCount, vMin + i * cellSize / mesh.vertexCount);

            uvs[i] = new Vector2(0.125f, 0.125f);
            uvs[i+1] = new Vector2(0.125f, 0.25f);
            uvs[i+2] = new Vector2(0.25f, 0.125f);
            uvs[i+3] = new Vector2(0.25f, 0.25f);
        }

        mesh.uv = uvs;

    }


 
}
