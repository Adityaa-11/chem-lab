using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public GameObject panel;
    public Transform contentParent;
    public GameObject rowPrefab;

    private Dictionary<string, TMP_Text> uiRows = new Dictionary<string, TMP_Text>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            panel.SetActive(!panel.activeSelf);

            Cursor.lockState = panel.activeSelf ?
                CursorLockMode.None :
                CursorLockMode.Locked;

            Cursor.visible = panel.activeSelf;

            if (panel.activeSelf)
                RefreshUI();
        }
    }

    void RefreshUI()
    {
        var inv = InventorySystem.instance;
        foreach (var item in inv.elements)
        {

            if (!uiRows.ContainsKey(item.Key))
            {
                GameObject row = Instantiate(rowPrefab, contentParent);
                var text = row.GetComponent<TMP_Text>();
                uiRows[item.Key] = text;
            }
            uiRows[item.Key].text = item.Key + ": " + item.Value;
        }
    }
}