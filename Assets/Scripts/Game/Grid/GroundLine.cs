using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GroundLine
{
    [SerializeField] private GameObject[] _Columns;
    [SerializeField] private Vector3 _Position;

    public GameObject[] Columns => _Columns;
    public Vector3 Position => _Position;
    public int Width => _Columns.Length;

    public GameObject this[int i] => _Columns[i];

    public void Fall(float force, float torqueForce)
    {
        Rigidbody b;

        foreach(GameObject g in _Columns)
        {
            b = g.GetComponent<Rigidbody>();
            b.isKinematic = false;
            b.AddForce(Vector3.down * force, ForceMode.VelocityChange);
            b.AddTorque(
                Random.Range(-torqueForce, torqueForce),
                Random.Range(-torqueForce, torqueForce),
                Random.Range(-torqueForce, torqueForce),
                ForceMode.Force
            );
        }
    }

    public GroundLine(GameObject[] columns, Vector3 position)
    {
        this._Columns = columns;
        this._Position = position;
    }
}
