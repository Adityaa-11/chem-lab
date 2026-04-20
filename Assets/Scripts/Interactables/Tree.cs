using UnityEngine;

public class Tree : MonoBehaviour
{
    public void Zap()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        BatterySystem battery = player.GetComponent<BatterySystem>();

        if (battery == null)
            return;

        battery.UseZap();
        InventorySystem inv = InventorySystem.instance;

        if (inv != null)
        {
            int rolls = Random.Range(5, 8); // 5–7 rolls

            for (int i = 0; i < rolls; i++)
            {
                string element = RollElement();
                inv.AddElement(element, 1);
            }
        }
        Destroy(gameObject);
    }

    string RollElement()
    {
        float r = Random.Range(0f, 100f);

        if (r < 45f) return "Carbon";
        if (r < 90f) return "Oxygen";
        if (r < 96f) return "Hydrogen";
        if (r < 98f) return "Nitrogen";

        return GetRandomTraceElement();
    }

    string GetRandomTraceElement()
    {
        string[] traceElements =
        {"Iron","Zinc","Magnesium","Magnesium","Magnesium","Magnesium","Copper"};

        return traceElements[Random.Range(0, traceElements.Length)];
    }
}