using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int money;
    public int hpLevel;
    public int flareRechargeLevel;
    public int flareDurationLevel;
    public int flareIntensityLevel;

    public int pickPowerLevel;
    public int pickSpeedLevel;

    private string saveFilePath;

    public int loadState;

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
                pickSpeedLevel = pickSpeedLevel
            };

            formatter.Serialize(stream, saveData);
        }
    }

    public void LoadGame()
    {
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
            }
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

}
