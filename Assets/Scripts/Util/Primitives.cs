using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Primitives
{
    private static Dictionary<PrimitiveType, Mesh> _Meshes;

    public static Mesh Get(PrimitiveType type)
    {
        if(_Meshes.ContainsKey(type))
        {
            return _Meshes[type];
        }

        Mesh res = GameObject.CreatePrimitive(type).GetComponent<MeshFilter>().mesh;
        _Meshes.Add(type, res);

        return res;
    }
}
