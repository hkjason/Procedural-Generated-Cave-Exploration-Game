using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace GK
{
    public class ConvexHull : MonoBehaviour
    {
        public GameObject RockPrefab;
        public CaveVisualisor caveVisualisor;

        public List<MeshFilter> meshFilters;
        public CombineInstance[] combine;


        public List<Vector3Int> oreVisitList;
        public List<Mesh> meshVisitList;

        public List<Ore> currentOreList;

        public LayerMask groundLayerMask;

        public int totalOreCount = 0;
        public int oreCountInGame = 400;

        public int playerOreCount = 0;

        public List<OreGroup> oreGroups;

        Vector3Int[] neighbourTable = new Vector3Int[6]
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(0, 0, -1),

        };



        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                Debug.Log("5");
                int oreAdded = 0;
                oreGroups = new List<OreGroup>();

                while (oreAdded < oreCountInGame && CaveGenerator.Instance.orePoints.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, CaveGenerator.Instance.orePoints.Count);
                    currentOreList = new List<Ore>();

                    oreVisitList = new List<Vector3Int>();
                    meshVisitList = new List<Mesh>();
                    totalOreCount = UnityEngine.Random.Range(8, 61);
                    oreAdded += totalOreCount;

                    meshFilters = new List<MeshFilter>();
                    int posx = Mathf.FloorToInt(CaveGenerator.Instance.orePoints[randomIndex].x);
                    int posy = Mathf.FloorToInt(CaveGenerator.Instance.orePoints[randomIndex].y);
                    int posz = Mathf.FloorToInt(CaveGenerator.Instance.orePoints[randomIndex].z);

                    ConvexHulling(new Vector3Int(posx, posy, posz), false);
                    CombineOre(meshFilters);

                    CaveGenerator.Instance.orePoints.RemoveAt(randomIndex);
                }
            }
        }

        bool ConvexHulling(Vector3Int pointForOre, bool isGrow, Mesh originalMesh = default)
        {
            if (totalOreCount == 0)
            {
                Debug.Log("OC == 0");
                return false;
            }

            Vector3Int growPoint;
            Mesh caveMesh;
            Vector3 firstPoint = new Vector3();
            Vector3 secondPoint = new Vector3();

            if (isGrow)
            {
                int maxTry = 0;
                bool connected = false;
                do
                {
                    growPoint = RandomNeightbour(pointForOre);

                    if (oreVisitList.Contains(growPoint))
                    {
                        Debug.Log("Dup Pos");
                        return false;
                    }

                    caveMesh = caveVisualisor.TurboMarchingCube(growPoint);

                    Vector3[] originalMeshVerts = originalMesh.vertices;
                    Vector3[] newMeshVerts = caveMesh.vertices;
                    int count = 0;
                    //Check connected mesh
                    for (int i = 0; i < newMeshVerts.Length; i++)
                    {
                        if (originalMeshVerts.Contains(newMeshVerts[i]))
                        {
                            count++;
                            if (count == 1)
                            {
                                firstPoint = newMeshVerts[i];
                            }
                            if (count == 2)
                            {
                                secondPoint = newMeshVerts[i];
                            }


                            if (count >= 2)
                            {
                                connected = true;
                                break;
                            }
                        }
                    }

                    maxTry++;

                } while (!connected && maxTry < 6);

                if (!connected)
                {
                    Debug.Log("Not connected");
                    return false;
                }

                GenerateMidPointOre(firstPoint, secondPoint);
            }
            else
            {
                growPoint = pointForOre;
                caveMesh = caveVisualisor.TurboMarchingCube(growPoint);
            }



            List<Vector3> randomCentres;

            int numOfOre = RandomOreCount(8, 11);
            randomCentres = GetRandomPointsOnMesh(caveMesh, numOfOre);


            for (int x = 0; x < randomCentres.Count; x++)
            {
                var calc = new ConvexHullCalculator();
                var verts = new List<Vector3>();
                var tris = new List<int>();
                var normals = new List<Vector3>();
                var points = new List<Vector3>();

                points.Clear();


                List<Vector3> randomPointsOnPlane = GetRandomPointsOnMesh(caveMesh, 20, randomCentres[x], UnityEngine.Random.Range(0.05f, 0.2f), UnityEngine.Random.Range(0.05f, 0.2f), UnityEngine.Random.Range(0.05f, 0.2f));
                List<Vector3> randomPointsNormal = GetNormalsAtRandomPoints(caveMesh, randomPointsOnPlane);
                List<Vector3> randomPoints = GetRandomPointsAroundNormal(randomPointsOnPlane, randomPointsNormal, 0.1f, 0.3f, 0, 50);

                points.AddRange(randomPointsOnPlane);
                points.AddRange(randomPoints);

                for (int pointIdx = 0; pointIdx < points.Count; pointIdx++)
                {
                    Vector3 tem = points[pointIdx];
                    tem.x = Mathf.Round(tem.x * 100f) / 100f;
                    tem.y = Mathf.Round(tem.y * 100f) / 100f;
                    tem.z = Mathf.Round(tem.z * 100f) / 100f;
                    points[pointIdx] = tem;
                }


                if (points.Count < 4)
                {
                    Debug.Log("Points < 4");
                    return false; //Generation fail
                }

                calc.GenerateHull(points, true, ref verts, ref tris, ref normals);

                var rock = Instantiate(RockPrefab);

                rock.transform.localPosition = Vector3.zero;
                rock.transform.localRotation = Quaternion.identity;
                rock.transform.localScale = Vector3.one;

                var mesh = new Mesh();

                mesh.SetVertices(verts);
                mesh.SetTriangles(tris, 0);
                mesh.SetNormals(normals);

                MeshFilter mf = rock.GetComponent<MeshFilter>();
                mf.sharedMesh = mesh;
                meshFilters.Add(mf);

                oreVisitList.Add(growPoint);
                meshVisitList.Add(caveMesh);

                Vector3 midPoint = Vector3.zero;
                foreach (Vector3 vertPoints in verts)
                {
                    midPoint += vertPoints;
                }
                midPoint /= verts.Count;

                Ore generatedOre = new Ore(mf, midPoint);
                currentOreList.Add(generatedOre);
            }

            bool doneGrow = false;
            int maxTryGrow = 0;
            while (totalOreCount > 0 && !doneGrow && maxTryGrow < 20)
            {
                int randomIndex = UnityEngine.Random.Range(0, oreVisitList.Count);
                Vector3Int pointForGrow = oreVisitList[randomIndex];
                Mesh meshForGrow = meshVisitList[randomIndex];

                doneGrow = ConvexHulling(pointForGrow, true, meshForGrow);

                maxTryGrow++;
            }

            return true;
        }

        void CombineOre(List<MeshFilter> meshFilters)
        {
            combine = new CombineInstance[meshFilters.Count];

            for (int i = 0; i < meshFilters.Count; i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            }

            Mesh combineMesh = new Mesh();
            combineMesh.CombineMeshes(combine);

            var combinedRock = Instantiate(RockPrefab);

            combinedRock.transform.SetParent(transform, false);
            combinedRock.transform.localPosition = Vector3.zero;
            combinedRock.transform.localRotation = Quaternion.identity;
            combinedRock.transform.localScale = Vector3.one;

            MeshFilter combinedRockMf = combinedRock.GetComponent<MeshFilter>();
            combinedRockMf.sharedMesh = combineMesh;

            MeshCollider combinedRockMc = combinedRock.GetComponent<MeshCollider>();
            combinedRockMc.sharedMesh = combineMesh;

            foreach (MeshFilter mf in meshFilters)
            {
                mf.transform.SetParent(combinedRock.transform, false);
                mf.gameObject.SetActive(false);
            }

            OreGroup oreGroup = combinedRock.AddComponent<OreGroup>(); 
            oreGroup.SetOreGroup(oreVisitList, currentOreList, combinedRockMf, combinedRockMc);
            oreGroups.Add(oreGroup);
        }

        public void UpdateOre(Vector3 hitPoint)
        {
            Vector3Int hitPointToInt = new Vector3Int(Mathf.FloorToInt(hitPoint.x), Mathf.FloorToInt(hitPoint.y), Mathf.FloorToInt(hitPoint.z));
            
            for (int i = 0; i < oreGroups.Count; i++)
            {
                if (oreGroups[i].oreGroupLocations.Contains(hitPointToInt))
                {
                    float radius = UnityEngine.Random.Range(0.7f, 1.0f);

                    List<Ore> oresWithinRadius = new List<Ore>();

                    for (int oreIdx = 0; oreIdx < oreGroups[i].ores.Count; oreIdx++)
                    {
                        Ore ore = oreGroups[i].ores[oreIdx];
                        if (Vector3.Distance(hitPoint, ore.oreLocation) <= radius)
                        {
                            oresWithinRadius.Add(ore);
                        }
                    }

                    // Random destruction magnitude from 7 to 10
                    int destructionMagnitude = UnityEngine.Random.Range(7, 11);

                    // Destroy points based on the magnitude
                    if (oresWithinRadius.Count <= destructionMagnitude)
                    {
                        // Destroy all ores within the radius
                        foreach (Ore ore in oresWithinRadius)
                        {
                            playerOreCount++;
                            Destroy(ore.meshFilter.gameObject);
                            oreGroups[i].ores.Remove(ore);
                        }
                    }
                    else
                    {
                        // Destroy a number of ores closest to the hitPoint
                        oresWithinRadius.Sort((a, b) => Vector3.Distance(hitPoint, a.oreLocation).CompareTo(Vector3.Distance(hitPoint, b.oreLocation)));
                        for (int j = 0; j < destructionMagnitude; j++)
                        {
                            playerOreCount++;
                            Destroy(oresWithinRadius[j].meshFilter.gameObject);
                            oreGroups[i].ores.Remove(oresWithinRadius[j]);
                        }
                    }

                    RebuildOre(oreGroups[i]);

                    return;
                }
            }
        }

        void RebuildOre(OreGroup oreGroup)
        {

            combine = new CombineInstance[oreGroup.ores.Count];

            for (int i = 0; i < oreGroup.ores.Count; i++)
            {
                combine[i].mesh = oreGroup.ores[i].meshFilter.sharedMesh;
                combine[i].transform = oreGroup.ores[i].meshFilter.transform.localToWorldMatrix;
            }

            Mesh combineMesh = new Mesh();
            combineMesh.CombineMeshes(combine);
            oreGroup.oreGroupMf.sharedMesh = combineMesh;
            oreGroup.oreGroupMc.sharedMesh = combineMesh;
        }

        List<Vector3> GetRandomPointsOnMesh(Mesh mesh, int count)
        {
            List<Vector3> points = new List<Vector3>();
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            // Compute the areas of the triangles
            float[] areas = new float[triangles.Length / 3];
            float totalArea = 0f;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v0 = vertices[triangles[i]];
                Vector3 v1 = vertices[triangles[i + 1]];
                Vector3 v2 = vertices[triangles[i + 2]];

                float area = Vector3.Cross(v1 - v0, v2 - v0).magnitude * 0.5f;
                areas[i / 3] = area;
                totalArea += area;
            }

            for (int i = 0; i < count; i++)
            {
                points.Add(GetRandomPointOnMesh(vertices, triangles, areas, totalArea));
            }

            return points;
        }

        List<Vector3> GetRandomPointsOnMesh(Mesh mesh, int count, Vector3 center, float radiusX, float radiusY, float radiusZ)
        {
            List<Vector3> points = new List<Vector3>();
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            // Compute the areas of the triangles
            float[] areas = new float[triangles.Length / 3];
            float totalArea = 0f;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v0 = vertices[triangles[i]];
                Vector3 v1 = vertices[triangles[i + 1]];
                Vector3 v2 = vertices[triangles[i + 2]];

                float area = Vector3.Cross(v1 - v0, v2 - v0).magnitude * 0.5f;
                areas[i / 3] = area;
                totalArea += area;
            }

            for (int i = 0; i < count; i++)
            {
                Vector3 randomPoint;
                int maxTry = 0;
                bool found = false;
                do
                {
                    randomPoint = GetRandomPointOnMesh(vertices, triangles, areas, totalArea);
                    maxTry++;
                    if (IsPointInEllipsoid(randomPoint, center, radiusX, radiusY, radiusZ))
                    {
                        found = true;
                    }

                } while (!found && maxTry < 50);

                if (found)
                {
                    points.Add(randomPoint);
                }
            }

            return points;
        }

        Vector3 GetRandomPointOnMesh(Vector3[] vertices, int[] triangles, float[] areas, float totalArea)
        {
            // Randomly select a triangle weighted by area
            float randomArea = UnityEngine.Random.value * totalArea;
            int triangleIndex = -1;
            for (int i = 0; i < areas.Length; i++)
            {
                if (randomArea < areas[i])
                {
                    triangleIndex = i * 3;
                    break;
                }
                randomArea -= areas[i];
            }

            if (triangleIndex == -1)
            {
                triangleIndex = (areas.Length - 1) * 3;
            }

            Vector3 v0 = vertices[triangles[triangleIndex]];
            Vector3 v1 = vertices[triangles[triangleIndex + 1]];
            Vector3 v2 = vertices[triangles[triangleIndex + 2]];

            // Random barycentric coordinates
            float r1 = Mathf.Sqrt(UnityEngine.Random.value);
            float r2 = UnityEngine.Random.value;

            Vector3 randomPoint = (1 - r1) * v0 + (r1 * (1 - r2)) * v1 + (r1 * r2) * v2;

            return randomPoint;
        }

        List<Vector3> GetNormalsAtRandomPoints(Mesh mesh, List<Vector3> points)
        {
            List<Vector3> normals = new List<Vector3>();
            Vector3[] vertices = mesh.vertices;
            Vector3[] meshNormals = mesh.normals;
            int[] triangles = mesh.triangles;

            foreach (Vector3 point in points)
            {
                // Find the triangle that contains this point
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    Vector3 v0 = vertices[triangles[i]];
                    Vector3 v1 = vertices[triangles[i + 1]];
                    Vector3 v2 = vertices[triangles[i + 2]];

                    if (IsPointInTriangle(point, v0, v1, v2))
                    {
                        Vector3 n0 = meshNormals[triangles[i]];
                        Vector3 n1 = meshNormals[triangles[i + 1]];
                        Vector3 n2 = meshNormals[triangles[i + 2]];

                        Vector3 normal = InterpolateNormal(point, v0, v1, v2, n0, n1, n2);

                        normals.Add(normal);
                        break;
                    }
                }
            }

            return normals;
        }

        Vector3 InterpolateNormal(Vector3 p, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 n0, Vector3 n1, Vector3 n2)
        {
            Vector3 v0v1 = v1 - v0;
            Vector3 v0v2 = v2 - v0;
            Vector3 v0p = p - v0;

            float d00 = Vector3.Dot(v0v1, v0v1);
            float d01 = Vector3.Dot(v0v1, v0v2);
            float d11 = Vector3.Dot(v0v2, v0v2);
            float d20 = Vector3.Dot(v0p, v0v1);
            float d21 = Vector3.Dot(v0p, v0v2);

            float denom = d00 * d11 - d01 * d01;
            float v = (d11 * d20 - d01 * d21) / denom;
            float w = (d00 * d21 - d01 * d20) / denom;
            float u = 1.0f - v - w;

            Vector3 normal = u * n0 + v * n1 + w * n2;
            return normal.normalized;
        }

        List<Vector3> GetRandomPointsAroundNormal(List<Vector3> points, List<Vector3> normals, float minDistance, float maxDistance, float minAngle, float maxAngle)
        {
            List<Vector3> randomPoints = new List<Vector3>();

            for (int i = 0; i < points.Count; i++)
            {
                randomPoints.Add(GetRandomPointAroundNormal(points[i], normals[i], minDistance, maxDistance, minAngle, maxAngle));
            }

            return randomPoints;
        }

        Vector3 GetRandomPointAroundNormal(Vector3 point, Vector3 normal, float minDistance, float maxDistance, float minAngle, float maxAngle)
        {
            // Random distance between minDistance and maxDistance
            float distance = UnityEngine.Random.Range(minDistance, maxDistance);

            // Random angle between minAngle and maxAngle in radians
            float angle = UnityEngine.Random.Range(minAngle, maxAngle);

            Vector3 randomDirection;
            Vector3 raycastOrigin = point + (normal * 0.1f);
            Vector3 raycastDirection;

            /*
            int maxTry = 0;
            do
            {
                randomDirection = UnityEngine.Random.insideUnitSphere.normalized;
                raycastDirection = randomDirection;
                maxTry++;

            } while ((Vector3.Angle(point + randomDirection, normal) > angle || Physics.Raycast(raycastOrigin, raycastDirection, distance, groundLayerMask)) && maxTry < 20);
            */
            randomDirection = UnityEngine.Random.insideUnitSphere.normalized;


            // Scale the direction by the distance
            Vector3 offset = randomDirection * distance;

            // Return the new point
            return point + offset;
        }

        void GenerateMidPointOre(Vector3 point1, Vector3 point2)
        {
            int numberOfMidPoints = RandomOreCount(2, 4);

            for (int i = 0; i < numberOfMidPoints; i++)
            {
                float t = UnityEngine.Random.Range(0f, 1f);
                Vector3 randomPoint = Vector3.Lerp(point1, point2, t);
                var calc = new ConvexHullCalculator();
                var verts = new List<Vector3>();
                var tris = new List<int>();
                var normals = new List<Vector3>();
                var points = new List<Vector3>();

                for (int pointCount = 0; pointCount < 7; pointCount++)
                {
                    float distance = UnityEngine.Random.Range(0.05f, 0.3f);
                    Vector3 randomDirection = UnityEngine.Random.onUnitSphere * distance;

                    points.Add(randomPoint + randomDirection);
                }


                calc.GenerateHull(points, true, ref verts, ref tris, ref normals);

                var rock = Instantiate(RockPrefab);

                rock.transform.localPosition = Vector3.zero;
                rock.transform.localRotation = Quaternion.identity;
                rock.transform.localScale = Vector3.one;

                var mesh = new Mesh();

                mesh.SetVertices(verts);
                mesh.SetTriangles(tris, 0);
                mesh.SetNormals(normals);

                MeshFilter mf = rock.GetComponent<MeshFilter>();
                mf.sharedMesh = mesh;
                meshFilters.Add(mf);

                Vector3 midPoint = Vector3.zero;
                foreach (Vector3 vertPoints in verts)
                {
                    midPoint += vertPoints;
                }
                midPoint /= verts.Count;

                Ore generatedOre = new Ore(mf, midPoint);
                currentOreList.Add(generatedOre);
            }
        }

        int RandomOreCount(int min, int max)
        {
            int count = 0;
            int numOfOre = UnityEngine.Random.Range(min, max);
            if (numOfOre < totalOreCount)
            {
                count = numOfOre;
                totalOreCount -= numOfOre;
            }
            else
            {
                count = totalOreCount;
                totalOreCount = 0;
            }
            return count;
        }

        Vector3Int RandomNeightbour(Vector3Int location)
        {
            int randomIndex = UnityEngine.Random.Range(0, neighbourTable.Length);
            Vector3Int offset = neighbourTable[randomIndex];

            return location + offset;
        }

        bool IsPointInTriangle(Vector3 p, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            Vector3 v0v1 = v1 - v0;
            Vector3 v0v2 = v2 - v0;
            Vector3 v0p = p - v0;

            float d00 = Vector3.Dot(v0v1, v0v1);
            float d01 = Vector3.Dot(v0v1, v0v2);
            float d11 = Vector3.Dot(v0v2, v0v2);
            float d20 = Vector3.Dot(v0p, v0v1);
            float d21 = Vector3.Dot(v0p, v0v2);

            float denom = d00 * d11 - d01 * d01;
            float v = (d11 * d20 - d01 * d21) / denom;
            float w = (d00 * d21 - d01 * d20) / denom;
            float u = 1.0f - v - w;

            return (u >= 0) && (v >= 0) && (w >= 0);
        }

        bool IsPointInEllipsoid(Vector3 point, Vector3 center, float radiusX, float radiusY, float radiusZ)
        {
            float dx = point.x - center.x;
            float dy = point.y - center.y;
            float dz = point.z - center.z;

            // Equation of an ellipsoid: (x^2 / rx^2) + (y^2 / ry^2) + (z^2 / rz^2) <= 1
            return (dx * dx) / (radiusX * radiusX) + (dy * dy) / (radiusY * radiusY) + (dz * dz) / (radiusZ * radiusZ) <= 1f;
        }
    }
}