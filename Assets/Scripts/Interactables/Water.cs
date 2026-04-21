using UnityEngine;

public class Water : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Zap();
    }

    private Vector3 startPos;
    void Start()
    {
        startPos = transform.position; 
        transform.position = startPos; // ensures it starts exactly there
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
            inv.AddElement("Hydrogen", 1);
            inv.AddElement("Oxygen", 2);
        }
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y - 1f,
            transform.position.z
        );
    }
}