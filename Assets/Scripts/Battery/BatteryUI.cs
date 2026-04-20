using UnityEngine;
using UnityEngine.UI;

public class BatteryUI : MonoBehaviour
{
    public BatterySystem battery;
    public Slider slider;

    void Start()
    {
        slider.maxValue = battery.maxBattery;
    }

    void Update()
    {
        slider.value = battery.currentBattery;
    }
}