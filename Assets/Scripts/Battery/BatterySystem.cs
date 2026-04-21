using UnityEngine;
using UnityEngine.SceneManagement;

public class BatterySystem : MonoBehaviour
{
    public float maxBattery = 10000f;
    public float currentBattery;

    public float drainRate = 2f;        // slow passive drain
    public float zapCost = 25f;         // COST per zap

    void Start()
    {
        currentBattery = maxBattery;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // passive drain over time
        currentBattery -= drainRate * Time.deltaTime;

        currentBattery = Mathf.Clamp(currentBattery, 0, maxBattery);

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