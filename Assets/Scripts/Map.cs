using System;
using UnityEngine;

public class Map : MonoBehaviour
{
    public Transform player;
    public Transform mainCamera;

    public Camera mapCam;

    private Vector3 offset = new Vector3(400, 0, 0);

    public float tempOff;

    private GameManager gameManager;

    public Transform playerMarker;

    public event Action<bool> mapChanged;
    private bool _mapOn;
    public bool mapOn
    {
        get { return _mapOn; }
        set
        {
            if (_mapOn != value)
            {
                _mapOn = value;
                mapCam.enabled = value;
                mapChanged?.Invoke(value);
            }
        }
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        mapOn = false;
    }

    private void Update()
    {
        if (!gameManager.isPause)
        {
            if (Input.GetKey(KeyCode.Tab) || Input.GetKey(KeyCode.M))
            {
                mapOn = true;
            }
            else
            {
                mapOn = false;
            }
        }
    }

    void LateUpdate()
    {
        transform.localRotation = Quaternion.Euler(mainCamera.localEulerAngles.x, player.localEulerAngles.y, 0f);
        transform.localPosition = mainCamera.position + offset + transform.forward * tempOff;

        playerMarker.localPosition = player.position + offset;
    }
}
