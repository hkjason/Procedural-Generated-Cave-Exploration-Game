using UnityEngine;

public class Chunk
{
        public GameObject chunkObject;
        MeshFilter meshFilter;
        MeshCollider meshCollider;
        MeshRenderer meshRenderer;

        public Vector3Int chunkPosition;

        //Chunk Size 8x8x8
        //Point store 10x10x10 for marching cubes
        public float[] density;
        const int pointArrSize = 729;

        public Chunk(Vector3Int pos)
        {
            chunkObject = new GameObject();
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

}
