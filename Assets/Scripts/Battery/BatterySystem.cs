using UnityEngine;
using UnityEngine.SceneManagement;

public class BatterySystem : MonoBehaviour
{
    public float maxBattery = 500f;
    public float currentBattery;

    public float drainRate = 2f;        // slow passive drain
    public float zapCost = 25f;         // COST per zap
    const float BATTERY_PER_LEVEL = 500f;
    public int batteryLevel = 0;
    void Start()
    {
        batteryLevel = GameState.Instance?.BatteryLevel ?? 0;
        currentBattery = maxBattery + BATTERY_PER_LEVEL * batteryLevel;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // passive drain over time
        currentBattery -= drainRate * Time.deltaTime;

        currentBattery = Mathf.Max(currentBattery, 0);

        if (currentBattery <= 0)
        {
            GameOver();
        }
    }
    public void UseZap()
    {
        currentBattery -= zapCost;

        // allow it to hit 0 but not go below visually
        currentBattery = Mathf.Max(currentBattery, 0f);
    }

    void GameOver()
    {  
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
    }
}