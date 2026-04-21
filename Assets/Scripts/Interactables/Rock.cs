using UnityEngine;

public class Rock : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Zap();
    }
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

        if (r < 40f) return "Silicon";
        if (r < 60f) return "Calcium";
        if (r < 75f) return "Iron";
        if (r < 85f) return "Aluminum";
        if (r < 93f) return "Sulfur";
        if (r < 98f) return "Copper";

        return GetRandomRareElement();
    }

    string GetRandomRareElement()
    {
        string[] rareElements = { "Gold", "Tin" };
        return rareElements[Random.Range(0, rareElements.Length)];
    }
}
