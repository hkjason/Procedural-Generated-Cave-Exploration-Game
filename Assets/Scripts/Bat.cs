using System.Collections;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Timeline;


public class Bat : MonoBehaviour
{
    private int hp = 100;

    public Animation anim;

    private Transform player;
    private float idleDuration = 5f;
    private float idleTimer = 0f;

    private float checkCD = 2f;
    private float checkTimer = 0f;

    private float attackCD = 3f;
    private float attackTimer = 0f;

    public GameObject proj;
    public Transform projSpawn;

    public LayerMask terrainLayer;

    public float roamRadius;
    public float wallDistance;

    public float turnRate = 120f;
    public float moveSpeed = 1f;

    bool isRoaming = false;

    [SerializeField] private float bulletSpeed;

    private Vector3 offset = new Vector3(400, 0, 0);

    private enum State { Idle, Roaming, Attacking, Dead}
    private State currentState;

    Coroutine startMoveCoroutine;
    Coroutine moveCoroutine;

    public Rigidbody rb;

    public GameObject markerGO;

    public Transform marker;

    private bool isAttacking;

    private bool targetFound;

    private void Awake()
    {
        //player = GameObject.FindGameObjectWithTag("Player").transform;
        player = CaveGenerator.Instance.player.transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        marker = Instantiate(markerGO, Vector3.zero, Quaternion.identity).transform;

        anim = GetComponent<Animation>();

        hp = 100;

        EnterState(State.Idle);

        checkTimer = 0f;
        checkCD = 2f + Random.Range(0f, 1f);

    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wallDistance);
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.Idle:
                HandleIdleState();
                CheckDistance();
                break;
            case State.Roaming:
                if (!isRoaming)
                { 
                    startMoveCoroutine = StartCoroutine(HandleRoamingState());
                    CheckDistance();
                }
                break;
            case State.Attacking:
                HandleAttackingState();
                CheckDistance();
                break;
        }
    }

    void LateUpdate()
    {
        if (isAttacking)
        {
            marker.localPosition = transform.position + offset;
        }
    }


    public void BatHpChange(int delta)
    {
        int val = hp + delta;

        if (val <= 0)
        {
            hp = 0;
            EnterState(State.Dead);
        }
        else
        {
            hp = val;
        }
    }

    void HandleIdleState()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleDuration)
        {
            EnterState(State.Roaming);
        }
    }

    IEnumerator HandleRoamingState()
    {
        isRoaming = true;

        int tries = 0;
        while (tries < 5)
        {
            Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
            Vector3 newPosition = transform.position + randomDirection;

            if (!Physics.CheckSphere(newPosition, wallDistance, terrainLayer))
            {
                tries = 5;
                moveCoroutine = StartCoroutine(MoveBat(newPosition));
                yield return moveCoroutine;
            }
            tries++;
        }

        isRoaming = false;
        EnterState(State.Idle);
    }

    IEnumerator MoveBat(Vector3 newPos)
    {
        while (Vector3.Distance(transform.position, newPos) > 0.1f)
        {
            // Rotate towards the target position
            Vector3 direction = (newPos - transform.position).normalized;
            float step = turnRate * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, direction, step * Mathf.Deg2Rad, 0f);
            transform.rotation = Quaternion.LookRotation(newDirection);

            // Move towards the target position
            float moveStep = moveSpeed * Time.deltaTime;
            Vector3 potentialPosition = transform.position + direction * moveStep;
            if (Physics.CheckSphere(potentialPosition, wallDistance, terrainLayer))
            {
                yield break;
            }

            transform.position = potentialPosition;

            yield return null; // Wait for the next frame
        }
    }

    void HandleAttackingState()
    { 
        attackTimer += Time.deltaTime;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        float turnRate = 120f;
        float step = turnRate * Time.deltaTime;

        Vector3 newDirection = Vector3.RotateTowards(transform.forward, directionToPlayer, step * Mathf.Deg2Rad, 0f);
        transform.rotation = Quaternion.LookRotation(newDirection);

        if (attackTimer >= attackCD)
        {
            if (Vector3.Angle(transform.forward, directionToPlayer) < 5f)
            {
                RaycastHit hit;

                if (!targetFound)
                {
                    if (Physics.Raycast(transform.position, directionToPlayer, out hit, 12f))
                    {
                        if (hit.transform.gameObject.tag == "Player")
                        {
                            targetFound = true;
                        }
                        else
                        {
                            attackTimer = 2.5f;
                            return;
                        }
                    }
                    else
                    {
                        attackTimer = 2.5f;
                        return;
                    }
                }

                attackTimer = 0;

                //ShootProjectile();
                GameObject bulletInstance = Instantiate(proj, projSpawn.position, Quaternion.identity); //INSTANTIATING THE FLARE PROJECTILE
                bulletInstance.transform.LookAt(player.transform.position);
                bulletInstance.transform.SetParent(transform);
                Rigidbody bullet = bulletInstance.GetComponent<Rigidbody>();
                bullet.AddForce(bulletInstance.transform.forward * bulletSpeed);
            }
        }
    }

    


    void CheckDistance()
    {
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkCD)
        {
            checkTimer = 0f;

            float dist = Vector3.Distance(player.position, transform.position);

            if (dist < 8)
            {
                if (currentState != State.Attacking)
                    EnterState(State.Attacking);
            }
            else if (dist >= 12 && currentState == State.Attacking)
            {
                if (currentState != State.Roaming)
                    EnterState(State.Roaming);
            }
            checkCD = 2f + Random.Range(0f, 1f);
        }
    }

    void EnterState(State s)
    {
        if (isRoaming)
        { 
            isRoaming = false;
            if (startMoveCoroutine != null)
            { 
                StopCoroutine(startMoveCoroutine);
            }
            if (moveCoroutine != null)
            { 
                StopCoroutine (moveCoroutine);
            }
        }

        switch (s)
        {
            case State.Idle:
                isAttacking = false;
                marker.gameObject.SetActive(false);
                idleTimer = 0f;
                idleDuration = Random.Range(2f, 5f);
                break;
            case State.Roaming:
                isAttacking = false;
                marker.gameObject.SetActive(false);
                break;
            case State.Attacking:
                attackTimer = 0f;
                targetFound = false;
                marker.gameObject.SetActive(true);
                isAttacking = true;
                break;
            case State.Dead:
                isAttacking = false;
                anim.Stop();
                rb.isKinematic = false;
                marker.gameObject.SetActive(false);
                Destroy(gameObject, 10f);
                break;
        }

        currentState = s;
    }
}
