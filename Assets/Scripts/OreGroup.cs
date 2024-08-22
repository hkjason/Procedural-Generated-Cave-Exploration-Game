using System.Collections.Generic;
using UnityEngine;
public class OreGroup : MonoBehaviour
{
    public List<Ore> ores;
    public MeshFilter oreGroupMf;
    public MeshCollider oreGroupMc;

    public void SetOreGroup(List<Ore> oreList, MeshFilter mf, MeshCollider mc)
    { 
        ores = oreList;
        oreGroupMf = mf;
        oreGroupMc = mc;
    }

}

public class Ore
{
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;
    public Vector3 oreLocation;

    public Ore(MeshFilter mf, MeshCollider mc, Vector3 oreLoc)
    {
        meshFilter = mf;
        meshCollider = mc;
        oreLocation = oreLoc;
    }
}
