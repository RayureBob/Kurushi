using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseMine
{
    protected bool _Armed;
    protected bool _Detonated;
    protected MineGraphics _Graphics;
    protected Vector3 _MineWorldPosition;

    public bool Armed => _Armed;
    public bool Detonated => _Detonated;
    public MineGraphics Graphics => _Graphics;

    protected BaseMine(MineGraphics graphics)
    {
        _Graphics = graphics;
        _MineWorldPosition = graphics.transform.position;
        _Armed = false;
    }

    public void Arm()
    {
        if (_Armed) return;

        _Armed = true;
        _Graphics.SetArmedColor();
    }

    public virtual MovingCube TryGetTargetCube()
    {
        if (!_Armed) return null;

        Collider[] hits = Physics.OverlapBox(_MineWorldPosition + Vector3.up * .5f, Vector3.one * .25f, Quaternion.identity, LayerMask.GetMask(Layers.MOVING_CUBE));

        if (hits.Length > 0)
        {
            _Detonated = true;
            return hits[0].GetComponentInParent<MovingCube>();
        }
        else return null;
    }

    public void Destroy()
    {
        _Graphics.DisableGraphics();
    }
}
