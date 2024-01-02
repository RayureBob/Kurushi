using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueMine : BaseMine
{
    public BlueMine(MineGraphics graphics) : base(graphics)
    {
        _Graphics.SetBlueMine();
    }
}
