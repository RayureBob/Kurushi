using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceWave : IEnumerable
{
    private List<List<MovingCube>> _Content;
    private bool _Failed;
    public int LineCount => _Content.Count;

    public List<MovingCube> this[int index] => _Content[index];

    public InstanceWave(List<List<MovingCube>> content)
    {
        _Content = content;
    }

    public void SetParents(Transform parent)
    {
        foreach(List<MovingCube> line in _Content)
            foreach(MovingCube c in line)
                c.transform.SetParent(parent);
    }

    public Vector3 PositionContentAndReturnLastPosition(Vector3 pos)
    {
        foreach(List<MovingCube> line in _Content)
        {
            Vector3 subPos = pos;
            foreach(MovingCube cube in line)
            {
                cube.transform.position = subPos;
                subPos += Vector3.right;
            }

            pos -= Vector3.forward;
        }

        return pos;
    }

    public void RemoveCube(MovingCube target)
    {
        foreach(List<MovingCube> line in _Content)
            if(line.Contains(target))
            {
                line.Remove(target);
            }
    }

    public void RemoveCubes(IEnumerable<MovingCube> cubes)
    {
        foreach (MovingCube cube in cubes)
            RemoveCube(cube);
    }

    public void ProcessDestroyedCubes()
    {
        foreach(List<MovingCube> line in this)
            line.TrimExcess();
    }

    public bool IsRoundFinished()
    {
        List<MovingCube> allCubes = GetAllCubes();
        if (allCubes.Count == 0) return true;

        foreach(MovingCube cube in allCubes)
        {
            if (cube.Type != CubeTypeEnum.Black)
                return false;
        }

        return true;
    }

    public List<MovingCube> GetAllCubes()
    {
        List<MovingCube> res = new List<MovingCube>();

        foreach (List<MovingCube> line in _Content)
            res.AddRange(line);

        return res;
    }

    public IEnumerator GetEnumerator()
    {
        return new InstanceWaveEnumerator(_Content);
    }

    private class InstanceWaveEnumerator : IEnumerator
    {
        private List<List<MovingCube>> _Cubes;
        private int position = -1;
        public object Current { 
            get {
                try
                {
                    return _Cubes[position];
                }
                catch(System.IndexOutOfRangeException e)
                {
                    throw e;
                }
            }
        }

        public InstanceWaveEnumerator(List<List<MovingCube>> cubes)
        {
            _Cubes = cubes;
        }

        public bool MoveNext()
        {
            position++;
            return position < _Cubes.Count;
        }

        public void Reset()
        {
            position = -1;
        }
    }
}
