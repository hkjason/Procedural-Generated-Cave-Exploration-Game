using UnityEngine;

public class Chunk
{
    public GameObject chunkObject;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    MeshRenderer meshRenderer;

    public Vector3Int chunkPosition;

    //Chunk Size 8x8x8
    //Point store 9x9x9 for marching cubes
    public float[] density;
    const int pointArrSize = 729;

    public Chunk(Vector3Int pos)
    {
        chunkObject = new GameObject(pos.x + ", " + pos.y + ", " + pos.z);
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        chunkPosition = pos;

        chunkObject.transform.SetParent(ChunkManager.Instance.transform);

        chunkObject.layer = LayerMask.NameToLayer("Terrain");

        density = new float[pointArrSize];
    }

    public void BuildChunk(Mesh mesh)
    {
        this.meshFilter.mesh = mesh;
        this.meshCollider.sharedMesh = mesh;

        meshRenderer.material = Resources.Load<Material>("Materials/ForTesting");
    }

    public void UpdateChunk(Mesh mesh)
    {
        this.meshFilter.mesh = mesh;
        this.meshCollider.sharedMesh = mesh;
    }

    public void UpdateDensity()
    {
        for (int x = 0; x <= 8; x++)
        {
            for (int y = 0; y <= 8; y++)
            {
                for (int z = 0; z <= 8; z++)
                {
                    int idx = x * 81 + y * 9 + z;
                    density[idx] = CaveGenerator.Instance.GetCave(x + chunkPosition.x, y + chunkPosition.y, z + chunkPosition.z);
                }
            }
        }
    }
}
