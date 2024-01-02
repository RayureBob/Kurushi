using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenMine : BaseMine
{
    public GreenMine(MineGraphics graphics) : base(graphics)
    {
        _Graphics.SetGreenMine();
    }

    public new List<MovingCube> TryGetTargetCube()
    {
        MovingCube above = base.TryGetTargetCube();

        if (above == null) return null;
        List<MovingCube> res = new List<MovingCube>();
        res.Add(above);

        float adjacentLength = 1f;
        float diagonalLength = Mathf.Sqrt(2f * (adjacentLength * adjacentLength));
        Vector3 center = above.transform.position;

        var cols = Physics.OverlapBox(center + Vector3.right * adjacentLength, Vector3.one * .25f, Quaternion.identity, LayerMask.GetMask(Layers.MOVING_CUBE));
        AddRangeToList(cols, res);
        cols = Physics.OverlapBox(center + Vector3.left * adjacentLength, Vector3.one * .25f, Quaternion.identity, LayerMask.GetMask(Layers.MOVING_CUBE));
        AddRangeToList(cols, res);
        cols = Physics.OverlapBox(center + Vector3.forward * adjacentLength, Vector3.one * .25f, Quaternion.identity, LayerMask.GetMask(Layers.MOVING_CUBE));
        AddRangeToList(cols, res);
        cols = Physics.OverlapBox(center + Vector3.back * adjacentLength, Vector3.one * .25f, Quaternion.identity, LayerMask.GetMask(Layers.MOVING_CUBE));
        AddRangeToList(cols, res);

        cols = Physics.OverlapBox(center + (Vector3.right + Vector3.forward) * diagonalLength, Vector3.one * .25f, Quaternion.identity, LayerMask.GetMask(Layers.MOVING_CUBE));
        AddRangeToList(cols, res);
        cols = Physics.OverlapBox(center + (Vector3.left + Vector3.forward) * diagonalLength, Vector3.one * .25f, Quaternion.identity, LayerMask.GetMask(Layers.MOVING_CUBE));
        AddRangeToList(cols, res);
        cols = Physics.OverlapBox(center + (Vector3.right + Vector3.back) * diagonalLength, Vector3.one * .25f, Quaternion.identity, LayerMask.GetMask(Layers.MOVING_CUBE));
        AddRangeToList(cols, res);
        cols = Physics.OverlapBox(center + (Vector3.left + Vector3.back) * diagonalLength, Vector3.one * .25f, Quaternion.identity, LayerMask.GetMask(Layers.MOVING_CUBE));
        AddRangeToList(cols, res);

        if (res.Count < 1) return null;
        return res;
    }

    private void AddRangeToList(Collider[] cols, List<MovingCube> res)
    {
        foreach(Collider c in cols)
        {
            res.Add(c.GetComponentInParent<MovingCube>());
        }
    }
}
