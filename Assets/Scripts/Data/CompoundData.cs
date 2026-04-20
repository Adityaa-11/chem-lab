using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewCompound", menuName = "ChemGame/Compound Data")]
public class CompoundData : ScriptableObject
{
    public string formula;
    public string compoundName;
    [TextArea(2, 5)] public string description;
    public CompoundIngredient[] ingredients;
}

[Serializable]
public class CompoundIngredient
{
    public string elementSymbol;
    public int quantity;
}
