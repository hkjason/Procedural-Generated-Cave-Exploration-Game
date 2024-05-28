using UnityEngine;

public class Chunk
{
        public GameObject chunkObject;
        MeshFilter meshFilter;
        MeshCollider meshCollider;
        MeshRenderer meshRenderer;

        public Vector3Int chunkPosition;

        public Chunk(Vector3Int pos, Mesh mesh)
        {
            chunkObject = new GameObject();
            meshFilter = chunkObject.AddComponent<MeshFilter>();
            meshCollider = chunkObject.AddComponent<MeshCollider>();
            meshRenderer = chunkObject.AddComponent<MeshRenderer>();

            chunkPosition = pos;
            this.meshFilter.mesh = mesh;
            this.meshCollider.sharedMesh = mesh;

            chunkObject.transform.SetParent(ChunkManager.Instance.transform);

            meshRenderer.material = Resources.Load<Material>("Materials/ForTesting");
            chunkObject.layer = LayerMask.NameToLayer("Terrain");

            //chunkObject.transform.position = pos;
        }

        public void UpdateChunk(Mesh mesh)
        {
            this.meshFilter.mesh = mesh;
            this.meshCollider.sharedMesh = mesh;
        }

}
