using UnityEngine;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem instance;

    public Dictionary<string, int> elements = new Dictionary<string, int>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddElement(string elementName, int amount)
    {
        if (!elements.ContainsKey(elementName))
        {
            elements[elementName] = 0;
        }

        elements[elementName] += amount;

        Debug.Log(elementName + " + " + amount);
    }
    public void RemoveElement(string elementName, int amount)
    {
        if (!elements.ContainsKey(elementName))
        {
            elements[elementName] = 0;
        }
        if (amount > elements[elementName])
        {
            return;
        }
        elements[elementName] -= amount;
    }
    public int GetAmount(string elementName)
    {
        if (elements == null)
            return 0;

        return elements.TryGetValue(elementName, out int value) ? value : 0;
    }
}