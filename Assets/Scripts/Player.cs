using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Player : MonoBehaviour
{
    //CameraMovement
    [SerializeField] Camera _camera;
    [SerializeField] float _hSensitivity;
    [SerializeField] float _vSensitivity;
    float _xRotation = 0f, _yRotation = 0f;

    //PlayerMovement
    float _speed;
    [SerializeField] float _walkspeed;
    [SerializeField] float _runSpeed;
    [SerializeField] Rigidbody _playerRb;
    Vector3 _movement;

    //IsGround
    float _groundDistance = 0.3f;
    [SerializeField]
    LayerMask terrainLayer;
    int terrainLayerIndex;
    [SerializeField]
    LayerMask defaultLayer;
    int defaultLayerIndex;

    RaycastHit hit;

    //Jump
    float _gravity = -9.81f;
    bool _isJumping = false;
    bool _jumpCooldown = false;

    float _gravityStartTime;

    [SerializeField]
    float _jumpVelocity;

    //IsGrounded
    bool _isGrounded = false;
    [SerializeField]
    Transform groundCheck;
    [SerializeField]
    float checkRadius;


    //Equipment
    Equipment currentEquipment;
    public Pickaxe pickaxe;
    public Flaregun flaregun;

    public Rigidbody flare;
    public Transform flareSpawnPoint;
    public Transform flareThrowPoint;
    public float throwPower;


    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red; // Set the color for the Gizmos
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius); // Draw a wireframe circle
        }
    }


    void Start()
    {
        terrainLayerIndex = Mathf.RoundToInt(Mathf.Log(terrainLayer.value, 2));
        defaultLayerIndex = Mathf.RoundToInt(Mathf.Log(defaultLayer.value, 2));
        Cursor.lockState = CursorLockMode.Locked;
        currentEquipment = pickaxe;
    }

    void Update()
    {
        CameraMovement();
        GetPlayerMovement();
        GetPlayerAction();
    }

    void FixedUpdate()
    {
        PlayerMovement();
        _playerRb.AddForce(Vector3.down * 4f, ForceMode.Force);
    }

    void CameraMovement()
    {
        float x = Input.GetAxis("Mouse X") * _hSensitivity * Time.deltaTime;
        float y = Input.GetAxis("Mouse Y") * _vSensitivity * Time.deltaTime;

        _xRotation -= y;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        _yRotation += x;

        transform.localRotation = Quaternion.Euler(0f, _yRotation, 0f);
        _camera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }

    void GetPlayerMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        bool y = Input.GetKey(KeyCode.Space);
        float z = Input.GetAxisRaw("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        _speed = isRunning ? _runSpeed : _walkspeed;

        _movement = (transform.right * x + transform.forward * z).normalized;

        if (y && _isJumping == false)
        {
            _isJumping = true;
        }
    }

    void PlayerMovement()
    {
        if (_isGrounded && _movement.magnitude == 0)
        {
            _playerRb.velocity = Vector3.zero;
        }

        Vector3 direction = Vector3.zero;
        if (_isGrounded)
        {
            _isGrounded = IsGrounded();
            if (!_isGrounded)
            {
                _gravityStartTime = Time.fixedTime;
                direction += ApplyGravity();
            }
        }
        else
        {
            _isGrounded = IsGrounded();
            direction += ApplyGravity();
        }


        //Move
        if (CheckSlope() <= 50)
        {
            direction += Vector3.ProjectOnPlane(_movement, hit.normal).normalized * _speed * Time.fixedDeltaTime;
        }
        else
        {
            direction += (_movement * _speed * Time.fixedDeltaTime);
        }

        if (_isJumping)
        {
            direction += transform.up * _jumpVelocity * Time.fixedDeltaTime;
        }

        if (_isGrounded)
        {
            _isJumping = false;
        }
        Debug.Log(direction);

        //Vector3 currVelocity = _playerRb.velocity;
        //currVelocity = new Vector3(direction.x, direction.y, direction.z);
        _playerRb.velocity = direction;
    }

    Vector3 ApplyGravity()
    {
        float timeDiff = Time.fixedTime - _gravityStartTime;
        float yVelocity = _gravity * timeDiff;
        return (transform.up * yVelocity); //* Time.fixedDeltaTime);
    }

    void GetPlayerAction()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("current equip" + currentEquipment);
            currentEquipment.Use(_camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)));
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (currentEquipment == pickaxe) return;
            if (currentEquipment.isAnimating) return;

            currentEquipment.Unequip();
            currentEquipment = pickaxe;
            currentEquipment.Equip();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (currentEquipment == flaregun) return;
            if (currentEquipment.isAnimating) return;
            currentEquipment.Unequip();
            currentEquipment = flaregun;
            currentEquipment.Equip();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            currentEquipment.Reload();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            SpawnFlare();
        }
    }

    //This is to checkslope or isground
    float CheckSlope()
    {
        float slopeAngle = 180f;
        //if (Physics.Raycast(transform.position - new Vector3(0f, 0.99f, 0f), Vector3.down, out hit, _groundDistance))
        if (Physics.Raycast(transform.position - new Vector3(0f, 0.99f, 0f), Vector3.down, out hit, 1f))
        {
            if (hit.transform.gameObject.layer == terrainLayerIndex)
                slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
        }
        return slopeAngle;
    }

    bool IsGrounded()
    {
        Debug.Log(Physics.CheckSphere(groundCheck.position, checkRadius, terrainLayer));
        return Physics.CheckSphere(groundCheck.position, checkRadius, terrainLayer);
    }

    public void Spawn(int x, int y, int z)
    {
        transform.position = new Vector3Int(x, y, z);
    }

    void SpawnFlare()
    {
        Rigidbody flareInstance;
        flareInstance = Instantiate(flare, flareSpawnPoint.position, Quaternion.identity) as Rigidbody;

        Vector3 heading = flareThrowPoint.position - flareSpawnPoint.position;
        float distance = heading.magnitude;
        Vector3 direction = heading / distance;

        flareInstance.AddForce(direction * throwPower);
    }
}
