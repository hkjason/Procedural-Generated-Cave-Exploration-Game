using System.Collections;
using TMPro;
using UnityEngine;

public class IK : MonoBehaviour
{
    public LayerMask groundLayer;
    public float walkDistance;
    public float stepTime;

    public SpiderLeg[] spiderLeg;

    public int currentIdx;

    private Coroutine[] coroutines;

    void Start()
    {
        currentIdx = 0;
        coroutines = new Coroutine[4];

        foreach (var leg in spiderLeg) 
        {
            leg.Setup();
            leg.legPoint = leg.legPoints[3].position;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 hitPos;
        Vector3 hitPos1;
        switch (currentIdx)
        {
            case 0:
                hitPos = RayCastToGround(0);
                hitPos1 = RayCastToGround(7);
                if (Vector3.Distance(hitPos, spiderLeg[0].legPoints[3].position) > walkDistance
                 || Vector3.Distance(hitPos1, spiderLeg[7].legPoints[3].position) > walkDistance)
                {
                    if (coroutines[currentIdx] != null)
                    {
                        StopCoroutine(coroutines[currentIdx]);
                    }
                    coroutines[currentIdx] = StartCoroutine(LerpDistination(0, 7, hitPos, hitPos1));
                }
                currentIdx = 1;
                break;
            case 1:
                hitPos = RayCastToGround(1);
                hitPos1 = RayCastToGround(6);
                if (Vector3.Distance(hitPos, spiderLeg[1].legPoints[3].position) > walkDistance
                 || Vector3.Distance(hitPos1, spiderLeg[6].legPoints[3].position) > walkDistance)
                {
                    if (coroutines[currentIdx] != null)
                    {
                        StopCoroutine(coroutines[currentIdx]);
                    }
                    coroutines[currentIdx] = StartCoroutine(LerpDistination(1, 6, hitPos, hitPos1));
                }
                currentIdx = 2;
                break;
            case 2:
                hitPos = RayCastToGround(2);
                hitPos1 = RayCastToGround(4);
                if (Vector3.Distance(hitPos, spiderLeg[2].legPoints[3].position) > walkDistance
                 || Vector3.Distance(hitPos1, spiderLeg[4].legPoints[3].position) > walkDistance)
                {
                    if (coroutines[currentIdx] != null)
                    {
                        StopCoroutine(coroutines[currentIdx]);
                    }
                    coroutines[currentIdx] = StartCoroutine(LerpDistination(2, 4, hitPos, hitPos1));
                }
                currentIdx = 3;
                break;
            case 3:
                hitPos = RayCastToGround(3);
                hitPos1 = RayCastToGround(5);
                if (Vector3.Distance(hitPos, spiderLeg[3].legPoints[3].position) > walkDistance
                 || Vector3.Distance(hitPos1, spiderLeg[5].legPoints[3].position) > walkDistance)
                {
                    if (coroutines[currentIdx] != null)
                    {
                        StopCoroutine(coroutines[currentIdx]);
                    }
                    coroutines[currentIdx] = StartCoroutine(LerpDistination(3, 5, hitPos, hitPos1));
                }
                currentIdx = 0;
                break;
            default:
                currentIdx = 0;
                break;
        }


        for (int idx = 0; idx < spiderLeg.Length; idx++)
        {
            InitializePositions(idx);

            Vector3 point = CalculateIK(spiderLeg[idx].legPoints[2], spiderLeg[idx].legPoints[3], spiderLeg[idx].distance[2], spiderLeg[idx].legPoint, false, 2);
            Vector3 point2 = CalculateIK(spiderLeg[idx].legPoints[1], spiderLeg[idx].legPoints[2], spiderLeg[idx].distance[1], point, false, 1);
            CalculateIK(spiderLeg[idx].legPoints[0], spiderLeg[idx].legPoints[1], spiderLeg[idx].distance[0], point2, false, 0);
        }
    }

    void InitializePositions(int idx)
    {
        spiderLeg[idx].legPoints[2].localRotation = spiderLeg[idx].originalQuaternion[2];
        spiderLeg[idx].legPoints[1].localRotation = spiderLeg[idx].originalQuaternion[1];
        spiderLeg[idx].legPoints[0].localRotation = spiderLeg[idx].originalQuaternion[0];

        Vector3 initPosPoint2 = spiderLeg[idx].legPoint - spiderLeg[idx].direction[0] * spiderLeg[idx].distance[2];
        Vector3 initPosPoint1 = initPosPoint2 - spiderLeg[idx].direction[1] * spiderLeg[idx].distance[1];

        CalculateIKHalf(spiderLeg[idx].legPoints[0], spiderLeg[idx].legPoints[1], spiderLeg[idx].distance[0], initPosPoint1, false, 0);
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

    Vector3 RayCastToGround(int idx)
    {
        Vector3 raycastOrigin = spiderLeg[idx].rayCastPoint.position;

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

    IEnumerator LerpDistination(int idx1, int idx2, Vector3 targetPosition1, Vector3 targetPosition2)
    {
        float duration = 0f;
        Vector3 initialPosition1 = spiderLeg[idx1].legPoint;
        Vector3 initialPosition2 = spiderLeg[idx2].legPoint;

        while (duration < stepTime)
        {
            spiderLeg[idx1].legPoint = Vector3.Lerp(initialPosition1, targetPosition1, duration / stepTime);
            spiderLeg[idx2].legPoint = Vector3.Lerp(initialPosition2, targetPosition2, duration / stepTime);

            duration += Time.deltaTime;
            yield return null;
        }
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

    public Vector3 legPoint;

    public Transform rayCastPoint;

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
