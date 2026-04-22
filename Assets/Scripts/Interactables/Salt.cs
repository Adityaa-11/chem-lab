using UnityEngine;

public class Salt : MonoBehaviour, IInteractable
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
            int rolls = Random.Range(3, 6); // 3-5 rolls

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

        if (r < 55f) return "Sodium";
        if (r < 90f) return "Chlorine";
        if (r < 95f) return "Magnesium";
        if (r < 98f) return "Calcium";
        return "Potassium";
    }
}
