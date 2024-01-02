using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kurushi.StateMachine
{
    public class StateAdvanceCube : BaseState
    {
        protected override bool shouldExit => _StepCount >= _TargetStepCount;

        private float _RotationDelta;
        private int _TargetStepCount;
        private int _StepCount;
        private List<MovingCube> _Cubes;

        public StateAdvanceCube(InstanceWave wave)
        {
            _Cubes = wave.GetAllCubes();
        }

        protected override void InternalFixedUpdate(float dt)
        {
            _TargetStepCount = Mathf.CeilToInt((1f / Time.fixedDeltaTime) * WaveSettings.TurnDuration);
            _RotationDelta = -90f / _TargetStepCount;

            foreach (MovingCube c in _Cubes)
            {
                c.UpdateMove(_RotationDelta);
            }

            _StepCount++;
        }

        protected override void OnEnter()
        {
            foreach (MovingCube cube in _Cubes)
                cube.PrepareToMove();
        }

        protected override void OnExit()
        {
            foreach (MovingCube m in _Cubes)
                m.CompleteMovement();
        }
    }
}