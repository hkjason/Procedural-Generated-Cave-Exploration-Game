using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool isPause = false;

    public int money;
    public int hpLevel;
    public int flareRechargeLevel;
    public int flareDurationLevel;
    public int flareIntensityLevel;

    public int pickPowerLevel;
    public int pickSpeedLevel;


    public float mouseSensitivity;
    public bool inverseY;

    private string saveFilePath;

    public bool tutorial;

    private bool _digWallQuest;
    private bool _digOreQuest;

    public event Action<bool> digWallChanged;
    public event Action<bool> digOreChanged;

    private bool _shootBulletQuest;
    private bool _reloadQuest;

    public event Action<bool> shootBulletChanged;
    public event Action<bool> reloadChanged;

    private bool _throwFlareQuest;
    private bool _shootFlareQuest;

    public event Action<bool> throwFlareChanged;
    public event Action<bool> shootFlareChanged;

    public bool digWallQuest
    {
        get { return _digWallQuest; }
        set
        {
            if (_digWallQuest != value)
            {
                _digWallQuest = value;
                if (value) digWallChanged?.Invoke(value);
            }
        }
    }

    public bool digOreQuest
    {
        get { return _digOreQuest; }
        set
        {
            if (_digOreQuest != value)
            {
                _digOreQuest = value;
                if (value) digOreChanged?.Invoke(value);
            }
        }
    }

    public bool shootBulletQuest
    {
        get { return _shootBulletQuest; }
        set
        {
            if (_shootBulletQuest != value)
            {
                _shootBulletQuest = value;
                if (value) shootBulletChanged?.Invoke(value);
            }
        }
    }

    public bool reloadQuest
    {
        get { return _reloadQuest; }
        set
        {
            if (_reloadQuest != value)
            {
                _reloadQuest = value;
                if (value) reloadChanged?.Invoke(value);
            }
        }
    }

    public bool throwFlareQuest
    {
        get { return _throwFlareQuest; }
        set
        {
            if (_throwFlareQuest != value)
            {
                _throwFlareQuest = value;
                if (value) throwFlareChanged?.Invoke(value);
            }
        }
    }

    public bool shootFlareQuest
    {
        get { return _shootFlareQuest; }
        set
        {
            if (_shootFlareQuest != value)
            {
                _shootFlareQuest = value;
                if (value) shootFlareChanged?.Invoke(value);
            }
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        saveFilePath = Application.persistentDataPath + "/saveData.dat";
        LoadGame();
    }

    public void SaveGame()
    {
        Debug.Log("SaveGame");

        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream stream = new FileStream(saveFilePath, FileMode.Create))
        {
            SaveData saveData = new SaveData
            {
                money = money,
                hpLevel = hpLevel,
                flareRechargeLevel = flareRechargeLevel,
                flareDurationLevel = flareDurationLevel,
                flareIntensityLevel = flareIntensityLevel,
                pickPowerLevel = pickPowerLevel,
                pickSpeedLevel = pickSpeedLevel,

                mouseSensitivity = mouseSensitivity,
                inverseY = inverseY,

                tutorial = tutorial
            };

            formatter.Serialize(stream, saveData);
        }
    }

    public void LoadGame()
    {
        Debug.Log("LoadGame");
        if (File.Exists(saveFilePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(saveFilePath, FileMode.Open))
            {
                SaveData saveData = (SaveData)formatter.Deserialize(stream);

                money = saveData.money;

                hpLevel = saveData.hpLevel;

                flareRechargeLevel = saveData.flareRechargeLevel;
                flareDurationLevel = saveData.flareDurationLevel;
                flareIntensityLevel = saveData.flareIntensityLevel;

                pickPowerLevel = saveData.pickPowerLevel;
                pickSpeedLevel = saveData.pickSpeedLevel;

                mouseSensitivity = saveData.mouseSensitivity;
                inverseY = saveData.inverseY;

                tutorial = saveData.tutorial;
            }
        }
        else
        {
            money = 0;
            hpLevel = 0;
            flareRechargeLevel = 0;
            flareDurationLevel = 0;
            flareIntensityLevel = 0;
            pickPowerLevel = 0;
            pickSpeedLevel = 0;

            mouseSensitivity = 1;
            inverseY = false;

            tutorial = false;

            SaveGame();
        }
    }

    public bool UpgradePickaxePower(int cost)
    {
        if (money >= cost)
        {
            money -= cost;
            pickPowerLevel++;

            SaveGame();
            return true;
        }
        return false;
    }

    public bool UpgradePickaxeSpeed(int cost)
    {
        if (money >= cost)
        {
            money -= cost;
            pickSpeedLevel++;

            SaveGame();
            return true;
        }
        return false;
    }

    public bool UpgradeFlareRecharge(int cost)
    {
        if (money >= cost)
        {
            money -= cost;
            flareRechargeLevel++;

            SaveGame();
            return true;
        }
        return false;
    }

    public bool UpgradeFlareDuration(int cost)
    {
        if (money >= cost)
        {
            money -= cost;
            flareDurationLevel++;

            SaveGame();
            return true;
        }
        return false;
    }

    public bool UpgradeFlareIntensity(int cost)
    {
        if (money >= cost)
        {
            money -= cost;
            flareIntensityLevel++;

            SaveGame();
            return true;
        }
        return false;
    }

    public bool UpgradeHP(int cost)
    {
        if (money >= cost)
        {
            money -= cost;
            hpLevel++;

            SaveGame();
            return true;
        }
        return false;
    }


    public void GameEndVictory()
    { 
        
    }

    public void GameEndDefeat()
    { 
    
    }


    public void ResetQuest()
    {
        _digWallQuest = false;
        _digOreQuest = false;

        _shootBulletQuest = false;
        _reloadQuest = false;

        _throwFlareQuest = false;
        _shootFlareQuest = false;
    }
}
