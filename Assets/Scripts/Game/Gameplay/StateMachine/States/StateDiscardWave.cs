using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kurushi.StateMachine
{
    public class StateDiscardWave : BaseState
    {
        protected override bool shouldExit => _CurrentWave.IsRoundFinished();

        private int _TurnInStep;
        private float _RotationDelta;

        private InstanceWave _CurrentWave;
        private float _StepCount;

        public StateDiscardWave(InstanceWave wave)
        {
            _CurrentWave = wave;
        }

        protected override void InternalFixedUpdate(float dt)
        {
            if (_StepCount <= _TurnInStep)
            {
                foreach (MovingCube c in _CurrentWave.GetAllCubes())
                {
                    c.UpdateMove(_RotationDelta);
                }

                _StepCount++;
            }
            else
            {
                _StepCount = 0; // Restart movement loop
                _TurnInStep = Mathf.CeilToInt((1f / Time.fixedDeltaTime) * WaveSettings.TurnDuration);
                _RotationDelta = -90f / _TurnInStep;
                _CurrentWave.ProcessDestroyedCubes();
            }
        }

        protected override void OnEnter()
        {
            _TurnInStep = Mathf.CeilToInt((1f / Time.fixedDeltaTime) * WaveSettings.TurnDuration);
            _RotationDelta = -90f / _TurnInStep;
            WaveSettings.FastForward = true;
            MineManager.Reset();
        }

        protected override void OnExit()
        {
            WaveSettings.FastForward = false;
        }
    }
}
