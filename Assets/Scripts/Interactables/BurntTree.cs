using UnityEngine;

public class BurntTree : MonoBehaviour, IInteractable
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
            int rolls = Random.Range(2, 5); // 2-4 rolls

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

        if (r < 75f) return "Carbon";
        if (r < 85f) return "Oxygen";
        if (r < 92f) return "Calcium";
        if (r < 96f) return "Potassium";
        return "Magnesium";
    }
}