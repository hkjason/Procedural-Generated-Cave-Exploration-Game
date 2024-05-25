using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Apple;
using static UnityEditor.Experimental.GraphView.GraphView;

public class IK : MonoBehaviour
{
    public LayerMask groundLayer;

    public SpiderLeg spiderLeg;

    public Transform destination;

    public Transform rayCastPoint;

    public Transform rayCastBackPoint;

    public float walkDistance;


    public Vector3 legPoint;


    void Start()
    {
        spiderLeg.Setup();
        legPoint = spiderLeg.legPoints[3].position;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //LegOne raycast
        Vector3 hitPos = RayCastToGround();
        rayCastBackPoint.position = hitPos;
        if (Vector3.Distance(hitPos, spiderLeg.legPoints[3].position) > walkDistance) 
        {
            //Vector3 point = CalculateIK(spiderLeg.legPoints[2], spiderLeg.legPoints[3], spiderLeg.distance[2], hitPos, false);
            //Vector3 point2 = CalculateIK(spiderLeg.legPoints[1], spiderLeg.legPoints[2], spiderLeg.distance[1], point, false);
            //CalculateIK(spiderLeg.legPoints[0], spiderLeg.legPoints[1], spiderLeg.distance[0], point2, false);
            legPoint = hitPos;
        }

        Vector3 point = CalculateIK(spiderLeg.legPoints[2], spiderLeg.legPoints[3], spiderLeg.distance[2], destination.position, false, 2);
        Vector3 point2 = CalculateIK(spiderLeg.legPoints[1], spiderLeg.legPoints[2], spiderLeg.distance[1], point, false, 1);
        CalculateIK(spiderLeg.legPoints[0], spiderLeg.legPoints[1], spiderLeg.distance[0], point2, false, 0);
        
    }


    Vector3 CalculateIK(Transform legPointA, Transform legPointB, float distance, Vector3 position, bool limitAngle, int index)
    {
        if (Vector3.Distance(position, legPointB.position) < 0.1f)
        {
            return legPointA.position;
        }

        Vector3 directionToTarget = position - legPointA.position;

        Vector3 directionAB = legPointB.position - legPointA.position;

        float angle = Vector3.Angle(directionAB, directionToTarget);

        if (Mathf.Abs(angle) > 0.001f)
        {
            Vector3 rotationAxis = Vector3.Cross(directionAB, directionToTarget).normalized;

            Quaternion desiredRotation = Quaternion.AngleAxis(angle, rotationAxis) * legPointA.localRotation;

            legPointA.localRotation = desiredRotation;

        }

        directionToTarget = legPointA.transform.up;

        Vector3 newPosition = position - directionToTarget.normalized * distance;
        return newPosition;
    }

    Quaternion LimitRotation(Quaternion targetRotation, Vector3 minRotation, Vector3 maxRotation)
    {
        
        targetRotation.Normalize();

        Vector3 targetEulerAngles = targetRotation.normalized.eulerAngles;
        Debug.Log("TEA: " + targetEulerAngles);
        Debug.Log("MIN: " + minRotation);
        Debug.Log("MAX: " + maxRotation);
        targetEulerAngles.x = Mathf.Clamp(targetEulerAngles.x, minRotation.x, maxRotation.x);
        targetEulerAngles.y = Mathf.Clamp(targetEulerAngles.y, minRotation.y, maxRotation.y);
        targetEulerAngles.z = Mathf.Clamp(targetEulerAngles.z, minRotation.z, maxRotation.z);

        
        Quaternion clampedRotation = Quaternion.Euler(targetEulerAngles);

        return clampedRotation;
        

    }

    Vector3 RayCastToGround()
    {
        Vector3 raycastOrigin = rayCastPoint.position;

        Vector3 raycastDirection = Vector3.down;

        RaycastHit hit;

        if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, 2f, groundLayer))
        {
            Debug.DrawRay(raycastOrigin, raycastDirection * hit.distance, Color.red);
            return hit.point;
        }
        Debug.Log("ERROR");
        return Vector3.zero;
    }
}

[System.Serializable]
public class SpiderLeg
{
    public Transform[] legPoints;
    public float[] distance;
    public Vector3[] rotationMin;
    public Vector3[] rotationMax;

    public Vector3[] originalVector;
    public Quaternion[] originalQuaternion;

    public void Setup()
    {
        CalculateDistance();
        CalculateRotationLimit();
    }

    private void CalculateDistance()
    {
        distance = new float[3];
        distance[0] = Vector3.Distance(legPoints[0].position,legPoints[1].position);
        distance[1] = Vector3.Distance(legPoints[1].position, legPoints[2].position);
        distance[2] = Vector3.Distance(legPoints[2].position, legPoints[3].position);
    }

    private void CalculateRotationLimit()
    {
        originalVector = new Vector3[3];
        originalVector[2] = legPoints[3].position - legPoints[2].position;
        originalVector[1] = legPoints[2].position - legPoints[1].position;
        originalVector[0] = legPoints[1].position - legPoints[0].position;

        originalQuaternion = new Quaternion[3];
        originalQuaternion[2] = legPoints[2].rotation;
        originalQuaternion[1] = legPoints[1].rotation;
        originalQuaternion[0] = legPoints[0].rotation;

        rotationMin = new Vector3[2];
        rotationMax = new Vector3[2];

        for (int i = 0; i < 2; i++)
        {
            Vector3 originalRotation = legPoints[i + 1].localRotation.eulerAngles;

            legPoints[i + 1].localEulerAngles = originalRotation + rotationLimitTable[i * 2] ;
            rotationMin[i] = legPoints[i + 1].rotation.eulerAngles;
            legPoints[i + 1].localEulerAngles = originalRotation + rotationLimitTable[i * 2 + 1];
            rotationMax[i] = legPoints[i + 1].rotation.eulerAngles;

            legPoints[i + 1].localEulerAngles = originalRotation;

        }

    }

    Vector3[] rotationLimitTable =
    {
        new Vector3(-22, -62, -53),
        new Vector3(9, 8 , 0),
        new Vector3(-50, 20, -5),
        new Vector3(-20, 30, 5),
    };
}
