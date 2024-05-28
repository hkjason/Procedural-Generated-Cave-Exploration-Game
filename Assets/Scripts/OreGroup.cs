using System.Collections.Generic;
using UnityEngine;

public class OreGroup : MonoBehaviour
{
    public List<Vector3Int> oreGroupLocations;
    public List<Ore> ores;
    public MeshFilter oreGroupMf;
    public MeshCollider oreGroupMc;

    //If location hit
    //check nearest meshes
    //
    public void SetOreGroup(List<Vector3Int> oreGroupLocs, List<Ore> oreList, MeshFilter mf, MeshCollider mc)
    { 
        oreGroupLocations = oreGroupLocs;
        ores = oreList;
        oreGroupMf = mf;
        oreGroupMc = mc;
    }

}

public class Ore
{
    public MeshFilter meshFilter;
    public Vector3 oreLocation;

    public Ore(MeshFilter mf, Vector3 oreLoc)
    {
        meshFilter = mf;
        oreLocation = oreLoc;
    }
}
