using UnityEngine;

public class Player : MonoBehaviour
{
    //CameraMovement
    [SerializeField] Camera _camera;
    [SerializeField] float _hSensitivity;
    [SerializeField] float _vSensitivity;
    [SerializeField] Transform _player;

    float xRotation = 0f, yRotation = 0f;

    //PlayerMovement
    float _speed;
    [SerializeField] float _walkspeed;
    [SerializeField] float _runSpeed;
    [SerializeField] Rigidbody _playerRb;

    Vector3 _movement;

    //IsGround
    [SerializeField] float _groundDistance;
    [SerializeField] float _jumpVelocity;
    [SerializeField] float _jumpHeight;
    LayerMask terrainLayer;
    bool _isJumping = false;
    float _airTime;
    float _lastDisplacement;

    float _gravity = -9.81f;

    //Equipment
    Equipment currentEquipment;
    public Pickaxe pickaxe;
    public Flaregun flaregun;

    public Rigidbody flare;
    public Transform flareSpawnPoint;
    public Transform flareThrowPoint;
    public float throwPower;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        currentEquipment = pickaxe;
    }

    // Update is called once per frame
    void Update()
    {
        CameraMovement();
        GetPlayerMovement();
        GetPlayerAction();
    }

    void FixedUpdate()
    {
        PlayerMovement();
    }

    void CameraMovement()
    {
        float x = Input.GetAxis("Mouse X") * _hSensitivity * Time.deltaTime;
        float y = Input.GetAxis("Mouse Y") * _vSensitivity * Time.deltaTime;

        xRotation -= y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        yRotation += x;

        _player.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        _camera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

    }

    void GetPlayerMovement()
    {
        float x = Input.GetAxis("Horizontal");
        bool y = Input.GetKey(KeyCode.Space);
        float z = Input.GetAxis("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        _speed = isRunning ? _runSpeed : _walkspeed;

        if (IsGrounded() && y)
        {
            _airTime = Time.fixedTime;
            _lastDisplacement = 0f;
            _isJumping = true;
        }

        _movement = _player.right * x + _player.forward * z;
    }

    void PlayerMovement()
    {
        float displacement = 0f;
        Vector3 direction = Vector3.zero;
        if (_isJumping)
        {
            float timeDiff = Time.fixedTime - _airTime;
            displacement = _jumpVelocity * timeDiff + 0.5f * _gravity * timeDiff * timeDiff - _lastDisplacement;
            _lastDisplacement = displacement;
            //Jump
            direction += (_player.up * displacement * _jumpHeight * Time.fixedDeltaTime);
        }
        //Move
        direction += (_movement * _speed * Time.fixedDeltaTime);

        _playerRb.MovePosition(_player.position + direction);

        if (_movement.magnitude == 0)
        {
            Vector3 velocity = _playerRb.velocity;
            velocity.x = 0f;
            //velocity.y = 0f;
            velocity.z = 0f;
            _playerRb.velocity = velocity;
        }

        if (CheckSlope() <= 50)
        {
            _playerRb.useGravity = false;
        }
        else
        {
            _playerRb.useGravity = true;
        }
    }

    bool IsGrounded()
    {
        Vector3 raycastOrigin = _player.position - new Vector3(0f,1f,0f) + Vector3.up * 0.1f;

        Vector3 raycastDirection = Vector3.down;

        RaycastHit hit;

        if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, _groundDistance))
        {
            if (hit.transform.gameObject.layer == terrainLayer)
            {
                _isJumping = false;
                return true;
            }
        }

        return false;
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

    float CheckSlope()
    {
        RaycastHit hit;
        float slopeAngle = 0f;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            // Calculate the angle between the ground normal and the up direction
            slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
        }

        return slopeAngle;
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
