using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("CameraMovement")]
    [SerializeField] private Camera _camera;
    [SerializeField] private float _hSensitivity;
    [SerializeField] private float _vSensitivity;
    private float _xRotation = 0f;
    private float _yRotation = 0f;

    [Header("PlayerMovement")]
    [SerializeField] float _walkSpeed;
    [SerializeField] float _runSpeed;
    [SerializeField] Rigidbody _playerRb;
    float _speed;
    Vector3 _movement;

    [Header("GroundCheck")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _checkRadius;
    [SerializeField] private LayerMask _terrainLayer;
    private int _terrainLayerIndex;
    private RaycastHit _hit;
    private float _gravity = -9.81f;
    private float _gravityStartTime;
    public bool _isGrounded = false;
    public Vector3 playerVelo;

    [Header("Jump")]
    [SerializeField] private float _jumpVelocity;
    private bool _isJumping = false;

    [Header("Equipment")]
    [SerializeField] private Pickaxe _pickaxe;
    [SerializeField] private Flaregun _flaregun;
    private Equipment _currentEquipment;

    [Header("Flare")]
    [SerializeField] private Rigidbody _flare;
    [SerializeField] private Transform _flareSpawnPoint;
    [SerializeField] private Transform _flareThrowPoint;
    [SerializeField] private float _throwPower;
    [SerializeField] private float _flareRegenTime;
    private float _flareCount;
    private float _flareSpawnTime;

    
    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_groundCheck.position, _checkRadius);
    }

    void Start()
    {
        _terrainLayerIndex = Mathf.RoundToInt(Mathf.Log(_terrainLayer.value, 2));
        Cursor.lockState = CursorLockMode.Locked;
        _currentEquipment = _pickaxe;
        _flareCount = 4;
    }

    void Update()
    {
        CameraMovement();
        GetPlayerMovement();
        GetPlayerAction();
        RegenFlare();
    }

    void FixedUpdate()
    {
        PlayerMovement();
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
        _speed = isRunning ? _runSpeed : _walkSpeed;

        _movement = (transform.right * x + transform.forward * z).normalized;

        if (y && _isJumping == false)
        {
            _isJumping = true;
        }
    }

    void PlayerMovement()
    {
        if (_movement.magnitude == 0)
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
            direction += Vector3.ProjectOnPlane(_movement, _hit.normal).normalized * _speed * Time.fixedDeltaTime;
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

        _playerRb.velocity = direction;
    }

    Vector3 ApplyGravity()
    {
        float timeDiff = Time.fixedTime - _gravityStartTime;
        float yVelocity = _gravity * timeDiff;
        return (transform.up * yVelocity);
    }

    float CheckSlope()
    {
        float slopeAngle = 180f;
        if (Physics.Raycast(transform.position - new Vector3(0f, 0.99f, 0f), Vector3.down, out _hit, 1f))
        {
            if (_hit.transform.gameObject.layer == _terrainLayerIndex)
                slopeAngle = Vector3.Angle(_hit.normal, Vector3.up);
        }
        return slopeAngle;
    }

    bool IsGrounded()
    {
        return Physics.CheckSphere(_groundCheck.position, _checkRadius, _terrainLayer);
    }

    void GetPlayerAction()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("current equip" + _currentEquipment);
            _currentEquipment.Use(_camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)));
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (_currentEquipment == _pickaxe) return;
            if (_currentEquipment.isAnimating) return;

            _currentEquipment.Unequip();
            _currentEquipment = _pickaxe;
            _currentEquipment.Equip();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (_currentEquipment == _flaregun) return;
            if (_currentEquipment.isAnimating) return;
            _currentEquipment.Unequip();
            _currentEquipment = _flaregun;
            _currentEquipment.Equip();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            _currentEquipment.Reload();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            SpawnFlare();
        }
    }

    public void Spawn(int x, int y, int z)
    {
        transform.position = new Vector3Int(x, y, z);
    }

    void SpawnFlare()
    {
        if (_flareCount <= 0)
        {
            return;
        }
        if (_flareCount == 4)
        {
            _flareSpawnTime = Time.time;
        }

        Rigidbody flareInstance;
        flareInstance = Instantiate(_flare, _flareSpawnPoint.position, Quaternion.identity) as Rigidbody;

        Vector3 heading = _flareThrowPoint.position - _flareSpawnPoint.position;
        float distance = heading.magnitude;
        Vector3 direction = heading / distance;

        flareInstance.AddForce(direction * _throwPower);

        _flareCount--;
    }

    void RegenFlare()
    {
        if (_flareCount < 4)
        {
            if (Time.time - _flareSpawnTime >= _flareRegenTime)
            {
                _flareCount++;
                _flareSpawnTime = Time.time;
            }
        }
    }
}
