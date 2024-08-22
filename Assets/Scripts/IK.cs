using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK : MonoBehaviour
{
    //For Spider Inverse Kinemetic
    //Implementation was not fully successful
    /*
    public LayerMask groundLayer;
    public float walkDistance;
    public float stepTime;

    public SpiderLeg[] spiderLeg;

    public int currentIdx;

    private Coroutine[] coroutines;
    public Transform bodyRayCastPoint;

    public Player player;

    public Rigidbody spiderRb;

    public float groundDistance;
    [Range(0f, 1f)]
    public float rotationRatio;
    [Range(0f, 1f)]
    public float positionRatio;

    RaycastHit lastHit;
    Vector3 lastPos;

    public float speed;

    private Coroutine posCoroutine;
    private Coroutine rotCoroutine;

    public float upDist = 0.4f;
    public float coolDown = 0.5f;
    RaycastHit hit;

    float currTime;

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
    private void OnEnable()
    {
        transform.rotation = Quaternion.LookRotation(-CaveGenerator.Instance.spiderHit.normal, Vector3.up);
        Debug.Log("spiderHIt: " + CaveGenerator.Instance.spiderHit.point);

        transform.position = CaveGenerator.Instance.spiderHit.point + CaveGenerator.Instance.spiderHit.normal * groundDistance;

        currTime = -10000;

        Vector3 raycastOrigin = bodyRayCastPoint.position;

        Vector3 raycastDirection = transform.up * -1;
        if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, 2f, groundLayer))
        {
            if (lastHit.collider == null)
            {
                lastHit = hit;
            }
        }
    }

    void FixedUpdate()
    {
        UpdateSpider();

        //UpdateSpiderIK();

    }

    void UpdateSpider()
    {
        Vector3 raycastOrigin = bodyRayCastPoint.position;

        Vector3 raycastDirection = transform.up * -1;

        if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, 2f, groundLayer))
        {
            if (lastHit.collider == null)
            {
                lastHit = hit;
            }

            float distDiff = upDist - (hit.point - raycastOrigin).magnitude;

            Vector3 moveTo = hit.normal * (upDist - (hit.point - raycastOrigin).magnitude);

            if (Mathf.Abs(distDiff) > 0.01f)
            {
                transform.position += moveTo;
            }

            Debug.DrawRay(raycastOrigin, raycastDirection, Color.blue);

            if (hit.normal != lastHit.normal)
            {
                if (Time.realtimeSinceStartup - currTime < coolDown)
                {
                    hit = lastHit;
                }
                else
                { 
                    currTime = Time.realtimeSinceStartup;
                    lastHit = hit;
                }
            }


            Vector3Int playerPos = player.GetCurrentGridPos();
            Vector3 hitVec = hit.point * 4;
            Vector3Int spiderGridPos = new Vector3Int(Mathf.FloorToInt(hitVec.x), Mathf.FloorToInt(hitVec.y), Mathf.FloorToInt(hitVec.z));

            float currTimee = Time.realtimeSinceStartup;
            List<Vector3Int> path = AStar.Instance.HPASPathFind(spiderGridPos, playerPos);
            Debug.Log("AstarTime:" + (Time.realtimeSinceStartup - currTimee));


            Vector3Int dirInt = (path[1] - path[0]);
            Vector3 direction = new Vector3(dirInt.x, dirInt.y, dirInt.z).normalized;
            //Vector3 direction = transform.forward;
            //transform.rotation = spiderRotation;



            if (hit.normal != Vector3.zero)
            {
                Vector3 slopeAdjustedDirection = Vector3.ProjectOnPlane(direction, hit.normal).normalized;

                if (posCoroutine != null)
                {
                    StopCoroutine(posCoroutine);
                }
                posCoroutine = StartCoroutine(LerpPosition(transform.position, transform.position + slopeAdjustedDirection * speed));

                //if (Physics.Raycast(raycastOrigin, angledDirection, out RaycastHit rhit, 2f, groundLayer))
                //{
                    //Debug.DrawRay(raycastOrigin, angledDirection, Color.green);
                //}


                //transform.position += slopeAdjustedDirection * speed * Time.fixedDeltaTime;

                Debug.DrawRay(transform.position, slopeAdjustedDirection, Color.red);

                Quaternion spiderRotation = Quaternion.LookRotation(slopeAdjustedDirection, hit.normal);

                if (rotCoroutine != null)
                {
                    StopCoroutine(rotCoroutine);
                }
                rotCoroutine = StartCoroutine(LerpRotation(transform.rotation, spiderRotation));
            }

        }
    }

    void UpdateSpiderIK()
    {
        Vector3 hitPos;
        Vector3 hitPos1;
        bool result;
        bool result1;
        switch (currentIdx)
        {
            case 0:
                result = RayCastToGround(0, out hitPos);
                result1 = RayCastToGround(7, out hitPos1);

                if (result && result1)
                {
                    if (Vector3.Distance(hitPos, spiderLeg[0].legPoints[3].position) > walkDistance
                    || Vector3.Distance(hitPos1, spiderLeg[7].legPoints[3].position) > walkDistance)
                    {
                        if (coroutines[currentIdx] != null)
                        {
                            StopCoroutine(coroutines[currentIdx]);
                        }
                        coroutines[currentIdx] = StartCoroutine(LerpDistination(0, 7, hitPos, hitPos1));
                    }
                }
                else
                {
                    if (result == false && result1 == false)
                    {
                        if (Vector3.Distance(spiderLeg[0].legStartPos.position, spiderLeg[0].legPoints[3].position) > walkDistance
                        || Vector3.Distance(spiderLeg[7].legStartPos.position, spiderLeg[7].legPoints[3].position) > walkDistance)
                        {
                            if (coroutines[currentIdx] != null)
                            {
                                StopCoroutine(coroutines[currentIdx]);
                            }
                            coroutines[currentIdx] = StartCoroutine(LerpDistination(0, 7, spiderLeg[0].legStartPos.position, spiderLeg[7].legStartPos.position));
                        }
 
                    }
                    else if (result == true && result1 == false)
                    {
                        if (Vector3.Distance(hitPos, spiderLeg[0].legPoints[3].position) > walkDistance
                        || Vector3.Distance(spiderLeg[7].legStartPos.position, spiderLeg[7].legPoints[3].position) > walkDistance)
                        {
                            if (coroutines[currentIdx] != null)
                            {
                                StopCoroutine(coroutines[currentIdx]);
                            }
                            coroutines[currentIdx] = StartCoroutine(LerpDistination(0, 7, hitPos, spiderLeg[7].legStartPos.position));


                        }
                        
                    }
                    else if (result == false && result1 == true)
                    {
                        if (Vector3.Distance(spiderLeg[0].legStartPos.position, spiderLeg[0].legPoints[3].position) > walkDistance
                        || Vector3.Distance(hitPos1, spiderLeg[7].legPoints[3].position) > walkDistance)
                        {
                            if (coroutines[currentIdx] != null)
                            {
                                StopCoroutine(coroutines[currentIdx]);
                            }
                            coroutines[currentIdx] = StartCoroutine(LerpDistination(0, 7, spiderLeg[0].legStartPos.position, hitPos1));


                        }
                    }
                }
                currentIdx = 1;
                break;
            case 1:
                result = RayCastToGround(1, out hitPos);
                result1 = RayCastToGround(6, out hitPos1);

                if (result && result1)
                {
                    if (Vector3.Distance(hitPos, spiderLeg[1].legPoints[3].position) > walkDistance
                    || Vector3.Distance(hitPos1, spiderLeg[6].legPoints[3].position) > walkDistance)
                    {
                        if (coroutines[currentIdx] != null)
                        {
                            StopCoroutine(coroutines[currentIdx]);
                        }
                        coroutines[currentIdx] = StartCoroutine(LerpDistination(1, 6, hitPos, hitPos1));

                    }
                }
                else
                {
                    if (result == false && result1 == false)
                    {
                        if (Vector3.Distance(spiderLeg[1].legStartPos.position, spiderLeg[1].legPoints[3].position) > walkDistance
                        || Vector3.Distance(spiderLeg[6].legStartPos.position, spiderLeg[6].legPoints[3].position) > walkDistance)
                        {
                            if (coroutines[currentIdx] != null)
                            {
                                StopCoroutine(coroutines[currentIdx]);
                            }
                            coroutines[currentIdx] = StartCoroutine(LerpDistination(1, 6, spiderLeg[1].legStartPos.position, spiderLeg[6].legStartPos.position));

                        }

                    }
                    else if (result == true && result1 == false)
                    {
                        if (Vector3.Distance(hitPos, spiderLeg[1].legPoints[3].position) > walkDistance
                        || Vector3.Distance(spiderLeg[6].legStartPos.position, spiderLeg[6].legPoints[3].position) > walkDistance)
                        {
                            if (coroutines[currentIdx] != null)
                            {
                                StopCoroutine(coroutines[currentIdx]);
                            }
                            coroutines[currentIdx] = StartCoroutine(LerpDistination(1, 6, hitPos, spiderLeg[6].legStartPos.position));

                        }

                    }
                    else if (result == false && result1 == true)
                    {
                        if (Vector3.Distance(spiderLeg[1].legStartPos.position, spiderLeg[1].legPoints[3].position) > walkDistance
                        || Vector3.Distance(hitPos1, spiderLeg[6].legPoints[3].position) > walkDistance)
                        {
                            if (coroutines[currentIdx] != null)
                            {
                                StopCoroutine(coroutines[currentIdx]);
                            }
                            coroutines[currentIdx] = StartCoroutine(LerpDistination(1, 6, spiderLeg[1].legStartPos.position, hitPos1));

                        }
                    }
                }
                currentIdx = 2;
                break;
            case 2:
                result = RayCastToGround(2, out hitPos);
                result1 = RayCastToGround(4, out hitPos1);

                if (result && result1)
                {
                    if (Vector3.Distance(hitPos, spiderLeg[2].legPoints[3].position) > walkDistance
                    || Vector3.Distance(hitPos1, spiderLeg[4].legPoints[3].position) > walkDistance)
                    {
                        if (coroutines[currentIdx] != null)
                        {
                            StopCoroutine(coroutines[currentIdx]);
                        }
                        coroutines[currentIdx] = StartCoroutine(LerpDistination(2, 4, hitPos, hitPos1));

                    }
                }
                else
                {
                    if (result == false && result1 == false)
                    {
                        if (Vector3.Distance(spiderLeg[2].legStartPos.position, spiderLeg[2].legPoints[3].position) > walkDistance
                        || Vector3.Distance(spiderLeg[4].legStartPos.position, spiderLeg[4].legPoints[3].position) > walkDistance)
                        {
                            if (coroutines[currentIdx] != null)
                            {
                                StopCoroutine(coroutines[currentIdx]);
                            }
                            coroutines[currentIdx] = StartCoroutine(LerpDistination(2, 4, spiderLeg[2].legStartPos.position, spiderLeg[4].legStartPos.position));

                        }

                    }
                    else if (result == true && result1 == false)
                    {
                        if (Vector3.Distance(hitPos, spiderLeg[2].legPoints[3].position) > walkDistance
                        || Vector3.Distance(spiderLeg[4].legStartPos.position, spiderLeg[4].legPoints[3].position) > walkDistance)
                        {
                            if (coroutines[currentIdx] != null)
                            {
                                StopCoroutine(coroutines[currentIdx]);
                            }
                            coroutines[currentIdx] = StartCoroutine(LerpDistination(2, 4, hitPos, spiderLeg[4].legStartPos.position));
                        }

                    }
                    else if (result == false && result1 == true)
                    {
                        if (Vector3.Distance(spiderLeg[2].legStartPos.position, spiderLeg[2].legPoints[3].position) > walkDistance
                        || Vector3.Distance(hitPos1, spiderLeg[4].legPoints[3].position) > walkDistance)
                        {
                            if (coroutines[currentIdx] != null)
                            {
                                StopCoroutine(coroutines[currentIdx]);
                            }
                            coroutines[currentIdx] = StartCoroutine(LerpDistination(2, 4, spiderLeg[2].legStartPos.position, hitPos1));

                        }
                    }
                }
                currentIdx = 3;
                break;
            case 3:
                result = RayCastToGround(3, out hitPos);
                result1 = RayCastToGround(5, out hitPos1);

                if (result && result1)
                {
                    if (Vector3.Distance(hitPos, spiderLeg[3].legPoints[3].position) > walkDistance
                    || Vector3.Distance(hitPos1, spiderLeg[5].legPoints[3].position) > walkDistance)
                    {
                        if (coroutines[currentIdx] != null)
                        {
                            StopCoroutine(coroutines[currentIdx]);
                        }
                        coroutines[currentIdx] = StartCoroutine(LerpDistination(3, 5, hitPos, hitPos1));

                    }
                }
                else
                {
                    if (result == false && result1 == false)
                    {
                        if (Vector3.Distance(spiderLeg[3].legStartPos.position, spiderLeg[3].legPoints[3].position) > walkDistance
                        || Vector3.Distance(spiderLeg[5].legStartPos.position, spiderLeg[5].legPoints[3].position) > walkDistance)
                        {
                            if (coroutines[currentIdx] != null)
                            {
                                StopCoroutine(coroutines[currentIdx]);
                            }
                            coroutines[currentIdx] = StartCoroutine(LerpDistination(3, 5, spiderLeg[3].legStartPos.position, spiderLeg[5].legStartPos.position));


                        }

                    }
                    else if (result == true && result1 == false)
                    {
                        if (Vector3.Distance(hitPos, spiderLeg[3].legPoints[3].position) > walkDistance
                        || Vector3.Distance(spiderLeg[5].legStartPos.position, spiderLeg[5].legPoints[3].position) > walkDistance)
                        {
                            if (coroutines[currentIdx] != null)
                            {
                                StopCoroutine(coroutines[currentIdx]);
                            }
                            coroutines[currentIdx] = StartCoroutine(LerpDistination(3, 5, hitPos, spiderLeg[5].legStartPos.position));


                        }

                    }
                    else if (result == false && result1 == true)
                    {
                        if (Vector3.Distance(spiderLeg[3].legStartPos.position, spiderLeg[3].legPoints[3].position) > walkDistance
                        || Vector3.Distance(hitPos1, spiderLeg[5].legPoints[3].position) > walkDistance)
                        {
                            if (coroutines[currentIdx] != null)
                            {
                                StopCoroutine(coroutines[currentIdx]);
                            }
                            coroutines[currentIdx] = StartCoroutine(LerpDistination(3, 5, spiderLeg[3].legStartPos.position, hitPos1));


                        }
                    }
                    currentIdx = 0;
                }
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

   bool RayCastToGround(int idx, out Vector3 result)
   {
        Vector3 raycastOrigin = spiderLeg[idx].rayCastPoint.position;

        Vector3 raycastDirection = spiderLeg[idx].rayCastPoint.transform.up * -1;

        RaycastHit hit;

        if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, 2f, groundLayer))
        {
            Debug.DrawRay(raycastOrigin, raycastDirection * hit.distance, Color.red);
            result = hit.point;
            return true;
        }
        result = Vector3.zero;
        return false;
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

    IEnumerator LerpPosition(Vector3 targetPosition1, Vector3 targetPosition2)
    {
        float duration = 0f;

        while (duration < positionRatio)
        {
            transform.position = Vector3.Lerp(targetPosition1, targetPosition2, duration / positionRatio);

            duration += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator LerpRotation(Quaternion targetRotation1, Quaternion targetRotation2)
    {
        float duration = 0f;

        while (duration < rotationRatio)
        {
            transform.rotation = Quaternion.Lerp(targetRotation1, targetRotation2, duration / rotationRatio);

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
    public Transform legStartPos;

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
    */
}
