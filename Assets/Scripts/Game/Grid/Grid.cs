using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Grid
{
    [SerializeField] private List<GroundLine> _Lines;

    public int Depth => _Lines.Count;
    public int Width => _Lines[0].Width;

    public Grid(List<GroundLine> lines)
    {
        _Lines = lines;
    }

    public void AddLine(GroundLine l)
    {
        _Lines.Add(l);
    }

    private GroundLine GetLineAt(int index)
    {
        return _Lines[index];
    }

    public GroundLine GetFirstLine() => GetLineAt(0);

    public GroundLine GetLastLine() => GetLineAt(_Lines.Count - 1);

    public GroundLine RemoveAndReturnLastLine()
    {
         GroundLine last = GetLastLine();
        _Lines.Remove(last);
        return last;
    }
}
