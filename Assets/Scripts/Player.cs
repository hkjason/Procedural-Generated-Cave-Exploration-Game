using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [Header("CameraMovement")]
    [SerializeField] private Camera _camera;
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
    [SerializeField] public Pickaxe pickaxe;
    [SerializeField] public Gun gun;
    [SerializeField] public Flaregun flaregun;
    [SerializeField] public PlatformLauncher platformLauncher;
    public Equipment currentEquipment;

    [Header("Flare")]
    [SerializeField] private Rigidbody _flare;
    [SerializeField] private Transform _flareSpawnPoint;
    [SerializeField] private Transform _flareThrowPoint;
    [SerializeField] private float _throwPower;
    [SerializeField] private float holdThreshold = 0.5f;
    [SerializeField] private Light _headLight;
    [SerializeField] private Light _headLight2;
    private bool isHoldingF = false;
    private float keyHoldTimeF = 0f;
    private bool isHoldingE = false;
    private float keyHoldTimeE = 0f;
    private int _flareCount;

    private float _flareSpawnTime;

    private int[] flareRechargeArr = { 14, 13, 12, 11 };
    private int[] flareDurationArr = { 25, 30, 35 };

    private GameManager gameManager;

    private bool alive = false;

    private float _oreCount = 0;
    public float oreCount
    {
        get { return _oreCount; }
        set
        {
            _oreCount = value;
            oreCountChanged?.Invoke(value);
        }
    }
    public int flareCount
    {
        get { return _flareCount; }
        set
        {
            _flareCount = value;
            flareCountChanged?.Invoke(value);
        }
    }

    public event Action<float> oreCountChanged;
    public event Action<int> flareCountChanged;

    private int _maxHp = 100;
    private int _hp;
    public int hp
    {
        get { return _hp; }
        set
        {
            _hp = value;
            hpChanged?.Invoke(value);
        }
    }

    public event Action<int> hpChanged;
    public event Action<Vector3, Vector3> OnDamageTaken;

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_groundCheck.position, _checkRadius);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        _terrainLayerIndex = Mathf.RoundToInt(Mathf.Log(_terrainLayer.value, 2));
        currentEquipment = pickaxe;
        flareCount = 4;
        hp = _maxHp;
        gameManager = GameManager.Instance;
    }

    void Update()
    {
        if (alive && !gameManager.isPause)
        {
            CameraMovement();
            GetPlayerMovement();
            GetPlayerAction();
            RegenFlare();
        }
    }

    void FixedUpdate()
    {
        if (alive)
        { 
            PlayerMovement();
        }
    }

    void CameraMovement()
    {
        float x = Input.GetAxis("Mouse X") * gameManager.mouseSensitivity * 100 * Time.deltaTime;
        float y = Input.GetAxis("Mouse Y") * gameManager.mouseSensitivity * 100 * Time.deltaTime;

        if (gameManager.inverseY)
        {
            y = -y;
        }

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
        if (Physics.Raycast(transform.position - new Vector3(0f, 0.99f, 0f), Vector3.down, out _hit, 10f))
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
            currentEquipment.Use(_camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)));
        }
        else if (Input.GetMouseButton(0))
        {
            if (currentEquipment == gun)
            {
                currentEquipment.Use(_camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)));
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (currentEquipment.isAnimating) return;
            if (currentEquipment == pickaxe)
            {
                currentEquipment.Use(_camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)));
            }
            else
            {
                Equipment lastEquipment = currentEquipment;
                currentEquipment.Unequip();
                currentEquipment = pickaxe;
                pickaxe.TemporaryDig(lastEquipment);
            }
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
            if (currentEquipment == gun) return;
            if (currentEquipment.isAnimating) return;
            currentEquipment.Unequip();
            currentEquipment = gun;
            currentEquipment.Equip();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (currentEquipment == flaregun) return;
            if (currentEquipment.isAnimating) return;
            currentEquipment.Unequip();
            currentEquipment = flaregun;
            currentEquipment.Equip();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (currentEquipment == platformLauncher) return;
            if (currentEquipment.isAnimating) return;
            currentEquipment.Unequip();
            currentEquipment = platformLauncher;
            currentEquipment.Equip();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            currentEquipment.Reload();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            isHoldingF = true;
            keyHoldTimeF = 0f;
        }

        if (Input.GetKey(KeyCode.F) && isHoldingF)
        {
            keyHoldTimeF += Time.deltaTime;
            if (keyHoldTimeF > holdThreshold)
            {
                _headLight.enabled = !_headLight.enabled;
                _headLight2.enabled = !_headLight2.enabled;
                isHoldingF = false;
            }
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            if (isHoldingF && keyHoldTimeF <= holdThreshold)
            {
                SpawnFlare();
            }
            isHoldingF = false;
        }


        if (Input.GetKeyDown(KeyCode.E))
        {
            isHoldingE = true;
            keyHoldTimeE = 0f;
        }

        if (Input.GetKey(KeyCode.E) && isHoldingE)
        {
            keyHoldTimeE += Time.deltaTime;
            if (keyHoldTimeE > holdThreshold)
            {
                if (oreCount >= 200)
                {
                    alive = false;
                    gameManager.GameEndVictory();
                }
                isHoldingE = false;
            }
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            isHoldingE = false;
        }
    }

    public void Spawn(Vector3 spawnOri)
    {
        _playerRb.velocity = new Vector3(0, 0, 0);

        Vector3 raycastDirection = Vector3.down;

        RaycastHit hit;

        if (Physics.Raycast(spawnOri, raycastDirection, out hit, 10f, _terrainLayer))
        {
            transform.position = new Vector3(hit.point.x, hit.point.y + 0.7f, hit.point.z);
        }

        gameObject.SetActive(true);
        alive = true;
    }

    void SpawnFlare()
    {
        if (flareCount <= 0)
        {
            return;
        }
        if (flareCount == 4)
        {
            _flareSpawnTime = Time.time;
        }

        gameManager.throwFlareQuest = true;

        Rigidbody flareInstance;
        flareInstance = Instantiate(_flare, _flareSpawnPoint.position, Quaternion.identity) as Rigidbody;

        Light light = flareInstance.GetComponent<Light>();
        Destroy(light, flareDurationArr[gameManager.flareDurationLevel]);

        Vector3 heading = _flareThrowPoint.position - _flareSpawnPoint.position;
        float distance = heading.magnitude;
        Vector3 direction = heading / distance;

        flareInstance.AddForce(direction * _throwPower);

        flareCount--;
    }

    void RegenFlare()
    {
        if (flareCount < 4)
        {
            if (Time.time - _flareSpawnTime >= flareRechargeArr[gameManager.flareRechargeLevel])
            {
                flareCount++;
                _flareSpawnTime = Time.time;
            }
        }
    }

    public Vector3Int GetCurrentGridPos()
    {
        Vector3 posInGrid = _hit.point * 4;
        return new Vector3Int(Mathf.FloorToInt(posInGrid.x), Mathf.FloorToInt(posInGrid.y), Mathf.FloorToInt(posInGrid.z));
    }


    public void PlayerHpChange(int delta, Vector3 source)
    {
        OnDamageTaken?.Invoke(source, transform.position);

        int val = hp + delta;
        if (val > _maxHp)
        {
            hp = _maxHp;
        }
        else if (val <= 0)
        {
            hp = 0;
            alive = false;
            gameManager.GameEndDefeat();
        }
        else
        {
            hp = val;
        }
    }
}
