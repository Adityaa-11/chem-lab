using UnityEngine;

[CreateAssetMenu(fileName = "NewElement", menuName = "ChemGame/Element Data")]
public class ElementData : ScriptableObject
{
    public string symbol;
    public string elementName;
    public int atomicNumber;
    public int shellCount;
    public int valenceElectrons;
    [TextArea(3, 8)] public string description;
    public Color elementColor = Color.green;
    public string[] foundInBiomes;
    public string[] prerequisiteSymbols;
}
