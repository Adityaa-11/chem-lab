using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewItem", menuName = "ChemGame/Discoverable Item")]
public class DiscoverableItem : ScriptableObject
{
    public string itemName;
    public string biome;
    [TextArea(2, 4)] public string description;
    public ElementYield[] elementYields;
}

[Serializable]
public class ElementYield
{
    public string elementSymbol;
    public int amount;
}