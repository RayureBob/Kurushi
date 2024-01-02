using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private GameManager _GameManager;

    [Space]
    [SerializeField] private DollyPosition _PlayerDollyCamera;
    [SerializeField] private BaseCinemachineCamera _LineCamera;

    [Space]
    [SerializeField] private float _PlayerDollyHighHeight;
    [SerializeField] private float _PlayerDollyLowHeight;

    private BaseCinemachineCamera _ActiveCamera;

    private void OnEnable()
    {
        _PlayerDollyCamera.SetAsCurrent();
    }

    private void OnDisable()
    {

    }

    public void OnGameStateChanged(GameStateEnum state)
    {
        switch(state)
        {
            case GameStateEnum.AdvancingWaveCubes:

                if (_ActiveCamera != _PlayerDollyCamera)
                {
                    _PlayerDollyCamera.SetAsCurrent();
                    _ActiveCamera = _PlayerDollyCamera;
                }

                _PlayerDollyCamera.SetHighHeight();
                break;

            case GameStateEnum.WaveSpawning:
            case GameStateEnum.DiscardingWave:
                _PlayerDollyCamera.SetLowHeight();
                break;
            default: break;
        }
    }

    public void SetDolly(bool low)
    {
        if(_ActiveCamera != _PlayerDollyCamera)
        {
            _PlayerDollyCamera.SetAsCurrent();
            _ActiveCamera = _PlayerDollyCamera;
        }

        if(low) _PlayerDollyCamera.SetLowHeight();
        else    _PlayerDollyCamera.SetHighHeight();
    }
}
