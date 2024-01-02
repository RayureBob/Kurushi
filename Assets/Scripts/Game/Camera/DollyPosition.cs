using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DollyPosition : BaseCinemachineCamera
{
    public event Action OnDollyRepositionned;

    [SerializeField] private GameManager _GameManager;
    [SerializeField] private CinemachineVirtualCamera _Camera;
    [SerializeField] private float _HighHeight;
    [SerializeField] private float _LowHeight;
    [SerializeField] private Transform _DollyPathTransform;

    private Transform _PlayerTransform;
    private CinemachineTrackedDolly _Dolly;
    private bool _ChangingHeight;
    private float _TargetHeight;

    protected override void OnEnable()
    {
        base.OnEnable();
        _PlayerTransform = _Camera.LookAt;
        _Dolly = _Camera.GetCinemachineComponent<CinemachineTrackedDolly>();
    }

    // Update is called once per frame
    void Update()
    {
        float xPos = _PlayerTransform.position.x + 2.5f;
        _Dolly.m_PathPosition = 1f - (xPos / 5f);

        if(_ChangingHeight && Mathf.FloorToInt(Camera.main.transform.position.y) != Mathf.FloorToInt(_TargetHeight))
        {
            _ChangingHeight = true;
            OnDollyRepositionned?.Invoke();
        }
    }

    public void SetHighHeight()
    {
        SetHeightTo(_HighHeight);
    }

    public void SetLowHeight()
    {
        SetHeightTo(_LowHeight);
    }

    private  void SetHeightTo(float height)
    {
        _ChangingHeight = true;
        _Dolly.m_PathOffset = new Vector3(_Dolly.m_PathOffset.x, height, _Dolly.m_PathOffset.z);
        _TargetHeight = _DollyPathTransform.position.y + height;
    }
}
