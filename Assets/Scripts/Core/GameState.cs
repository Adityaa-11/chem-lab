using UnityEngine;
using System;
using System.Collections.Generic;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    public event Action<string> OnElementResearched;
    public event Action<int> OnRPChanged;

    [SerializeField] private int startingRP = 5;
    [SerializeField] private ElementData[] allElements;
    [SerializeField] private string[] starterSymbols = { "H", "Fe" };

    private HashSet<string> researched = new HashSet<string>();
    private Dictionary<string, int> atoms = new Dictionary<string, int>();
    private HashSet<string> discoveredCompounds = new HashSet<string>();
    private int rp;

    public int RP => rp;
    public ElementData[] AllElements => allElements;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        rp = startingRP;
        foreach (var s in starterSymbols) researched.Add(s);
    }

    public bool IsResearched(string sym) => researched.Contains(sym);

    public bool CanResearch(string sym)
    {
        if (researched.Contains(sym)) return false;
        var el = GetElement(sym);
        if (el == null) return false;
        if (el.prerequisiteSymbols == null) return true;
        foreach (var p in el.prerequisiteSymbols)
            if (!researched.Contains(p)) return false;
        return true;
    }

    public bool Research(string sym)
    {
        if (!CanResearch(sym)) return false;
        researched.Add(sym);
        OnElementResearched?.Invoke(sym);
        return true;
    }

    public IReadOnlyCollection<string> Researched => researched;

    public ElementData GetElement(string sym)
    {
        if (allElements == null) return null;
        foreach (var e in allElements)
            if (e.symbol == sym) return e;
        return null;
    }

    public int GetAtoms(string sym) => atoms.TryGetValue(sym, out int c) ? c : 0;
    public void AddAtoms(string sym, int n) { if (!atoms.ContainsKey(sym)) atoms[sym] = 0; atoms[sym] += n; }
    public bool RemoveAtoms(string sym, int n) { if (GetAtoms(sym) < n) return false; atoms[sym] -= n; return true; }

    public void AddRP(int n) { rp += n; OnRPChanged?.Invoke(rp); }
    public bool SpendRP(int n) { if (rp < n) return false; rp -= n; OnRPChanged?.Invoke(rp); return true; }

    public bool IsCompoundDiscovered(string formula) => discoveredCompounds.Contains(formula);
    public void DiscoverCompound(string formula) => discoveredCompounds.Add(formula);
}
