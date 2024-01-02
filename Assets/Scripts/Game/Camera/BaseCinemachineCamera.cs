using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCinemachineCamera : MonoBehaviour
{
    private static List<BaseCinemachineCamera> _AllCameras;

    protected virtual void OnEnable()
    {
        if(_AllCameras == null)
        {
            _AllCameras = new List<BaseCinemachineCamera>();
        }

        _AllCameras.Add(this);
    }

    protected virtual void OnDisable()
    {
        _AllCameras.Remove(this);

        if (_AllCameras.Count == 0)
            _AllCameras = null;
    }

    public void SetAsCurrent()
    {
        foreach(BaseCinemachineCamera camera in _AllCameras)
        {
            if(camera != this)
            {
                camera.gameObject.SetActive(false);
            }
        }

        gameObject.SetActive(true);
    }
}
