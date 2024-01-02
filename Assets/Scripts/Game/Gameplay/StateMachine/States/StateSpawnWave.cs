using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kurushi.StateMachine
{
    public class StateSpawnWave : BaseState
    {

        protected override bool shouldExit => _LinesSpawned >= _Wave.LineCount;

        private InstanceWave _Wave;
        private List<List<MovingCube>> _CurrentLines = new List<List<MovingCube>>();

        private float _StartingHeight;
        private int _CurrentIndex = 0;
        private float _Duration;
        private float _DelayBetweenLines;
        private float _DeltaHeight;
        private int _LinesSpawned = 0;

        private float _SpawnCountdown;

        protected override void InternalFixedUpdate(float dt)
        {
            if(_SpawnCountdown <= 0 && _CurrentIndex < _Wave.LineCount)
            {
                _CurrentLines.Add(_Wave[_CurrentIndex++]);
                _SpawnCountdown = _DelayBetweenLines;
            }

            List<List<MovingCube>> spawnedLines = new List<List<MovingCube>>();

            foreach(List<MovingCube> line in _CurrentLines)
            {
                ForeachCubeInList(
                    line,
                    cube => cube.transform.position += Vector3.up * _DeltaHeight
                );

                float currentHeight = line[0].transform.position.y;
                if (currentHeight >= _StartingHeight + 1)
                {
                    if(currentHeight > _StartingHeight + 1f)
                    {
                        ForeachCubeInList(
                            line,
                            cube => cube.transform.position = new Vector3
                            {
                                x = cube.transform.position.x,
                                y = _StartingHeight + 1f,
                                z = cube.transform.position.z,
                            }
                        );
                    }


                    spawnedLines.Add(line);
                    _LinesSpawned++;
                }
            }

            foreach (var line in spawnedLines)
                _CurrentLines.Remove(line);

            _SpawnCountdown -= dt;
        }

        public StateSpawnWave(InstanceWave wave)
        {
            _Wave = wave;
            _StartingHeight = _Wave[0][0].transform.position.y;
        }

        private void ForeachCubeInList(List<MovingCube> list, Action<MovingCube> behaviour)
        {
            foreach(MovingCube m in list)
                behaviour(m);
        }

        protected override void OnEnter()
        {
            _Duration = WaveSettings.WaveSpawnDuration;
            _DelayBetweenLines = WaveSettings.WaveSpawnLineDelay;
            _DeltaHeight = 1f / ((1f / Time.fixedDeltaTime) * _Duration);
        }
    }
}