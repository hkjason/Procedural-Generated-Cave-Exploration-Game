using UnityEngine;

public class IK : MonoBehaviour
{
    public LayerMask groundLayer;

    public SpiderLeg spiderLeg;

    public Transform destination;

    public Transform rayCastPoint;

    public Transform rayCastBackPoint;

    public float walkDistance;


    public Vector3 legPoint;

    public Transform testPoint;

    Vector3 a;
    Vector3 b;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(a, 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(b, 0.1f);
    }

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
        

        InitializePositions();
        
        Vector3 point = CalculateIK(spiderLeg.legPoints[2], spiderLeg.legPoints[3], spiderLeg.distance[2], legPoint, false, 2);
        Vector3 point2 = CalculateIK(spiderLeg.legPoints[1], spiderLeg.legPoints[2], spiderLeg.distance[1], point, false, 1);
        CalculateIK(spiderLeg.legPoints[0], spiderLeg.legPoints[1], spiderLeg.distance[0], point2, false, 0);
        
    }

    void InitializePositions()
    {
        spiderLeg.legPoints[2].localRotation = spiderLeg.originalQuaternion[2];
        spiderLeg.legPoints[1].localRotation = spiderLeg.originalQuaternion[1];
        spiderLeg.legPoints[0].localRotation = spiderLeg.originalQuaternion[0];

        Vector3 initPosPoint2 = legPoint - spiderLeg.direction[0] * spiderLeg.distance[2];
        Vector3 initPosPoint1 = initPosPoint2 - spiderLeg.direction[1] * spiderLeg.distance[1];

        CalculateIKHalf(spiderLeg.legPoints[0], spiderLeg.legPoints[1], spiderLeg.distance[0], initPosPoint1, false, 0);
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

            Quaternion desiredRotation = Quaternion.AngleAxis(angle, rotationAxis) * legPointA.rotation;

            legPointA.rotation = desiredRotation;

        }

        Vector3 newPosition = position - legPointA.transform.up * distance;
        return newPosition;
    }

    void CalculateIKHalf(Transform legPointA, Transform legPointB, float distance, Vector3 position, bool limitAngle, int index)
    {
        if (Vector3.Distance(position, legPointB.position) < 0.1f)
        {
            return;
        }

        Vector3 directionToTarget = position - legPointA.position;

        Vector3 directionAB = legPointB.position - legPointA.position;

        float angle = Vector3.Angle(directionAB, directionToTarget) / 2f;

        if (Mathf.Abs(angle) > 0.001f)
        {
            Vector3 rotationAxis = Vector3.Cross(directionAB, directionToTarget).normalized;

            Quaternion desiredRotation = Quaternion.AngleAxis(angle, rotationAxis) * legPointA.rotation;

            legPointA.rotation = desiredRotation;
        }
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
    public Vector3[] direction;
    public Vector3[] originalPos;
    public Quaternion[] originalQuaternion;

    public void Setup()
    {
        CalculateDistance();
        CalculateDirection();
        CalculateRotationLimit();
    }

    private void CalculateDistance()
    {
        distance = new float[3];
        distance[0] = Vector3.Distance(legPoints[0].position, legPoints[1].position);
        distance[1] = Vector3.Distance(legPoints[1].position, legPoints[2].position);
        distance[2] = Vector3.Distance(legPoints[2].position, legPoints[3].position);
    }

    private void CalculateDirection()
    {
        direction = new Vector3[3];
        direction[0] = legPoints[2].up;
        direction[1] = legPoints[1].up;
    }

    private void CalculateRotationLimit()
    {
        originalPos = new Vector3[3];
        originalPos[2] = legPoints[2].localPosition;
        originalPos[1] = legPoints[1].localPosition;
        originalPos[0] = legPoints[0].localPosition;

        originalQuaternion = new Quaternion[3];
        originalQuaternion[2] = legPoints[2].localRotation;
        originalQuaternion[1] = legPoints[1].localRotation;
        originalQuaternion[0] = legPoints[0].localRotation;
    }
}
