using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Chemist : MonoBehaviour
{
    public int rpReward = 1;

    public void Talk()
    {
        var gs = GameState.Instance;
        if (gs == null || gs.AllElements == null)
        {
            DialogUI.Show("The Chemist", "My notes are scattered today — come back later.");
            return;
        }

        string biome = BiomeForCurrentScene();
        var pick = PickElementForBiome(gs.AllElements, biome);

        if (pick == null)
        {
            DialogUI.Show("The Chemist",
                "Nothing new I can teach you in this place right now. Explore a different world.");
            return;
        }

        string title = $"The Chemist on {pick.elementName} ({pick.symbol})";
        string body = $"Atomic number {pick.atomicNumber}. Shells: {pick.shellCount}. Valence: {pick.valenceElectrons}.\n\n{pick.description}";

        if (gs.CanResearch(pick.symbol))
            body += "\n\n<i>You've seen enough of this in the wild to add it to your research tree.</i>";

        gs.AddRP(rpReward);
        DialogUI.Show(title, body);
    }

    static string BiomeForCurrentScene()
    {
        string name = SceneManager.GetActiveScene().name;
        if (name.Contains("Cave")) return "Caverns";
        if (name.Contains("Forest")) return "Forest";
        if (name.Contains("Waste")) return "Wasteland";
        return "Forest";
    }

    static ElementData PickElementForBiome(ElementData[] all, string biome)
    {
        var matches = new List<ElementData>();
        foreach (var e in all)
        {
            if (e == null || e.foundInBiomes == null) continue;
            foreach (var b in e.foundInBiomes)
            {
                if (b == biome) { matches.Add(e); break; }
            }
        }
        if (matches.Count == 0) return null;
        return matches[Random.Range(0, matches.Count)];
    }
}
