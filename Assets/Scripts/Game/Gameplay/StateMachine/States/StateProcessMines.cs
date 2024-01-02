using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kurushi.StateMachine
{
    public class StateProcessMines : BaseState
    {
        private float _ClockWatch;
        protected override bool shouldExit => _ClockWatch <= 0f;

        private InstanceWave _Wave;
        List<ValueTuple<BaseMine, MovingCube[], AsyncState[]>> _MineProcessors = new List<ValueTuple<BaseMine, MovingCube[], AsyncState[]>>();
        //private Dictionary<BaseMine, AsyncState[]> _MineProcessors = new Dictionary<BaseMine, AsyncState[]>();

        public StateProcessMines(InstanceWave wave)
        {
            _Wave = wave;
            _ClockWatch = WaveSettings.DelayBetweenTurns;
        }

        protected override void InternalFixedUpdate(float dt)
        {
            List<BaseMine> triggeredMines = new List<BaseMine>();
            List<MovingCube> destroyedCubes = new List<MovingCube>();

            BlueMine blueMine = MineManager.ArmedBlueMine;
            MovingCube blueMineTarget = blueMine?.TryGetTargetCube();

            if(blueMineTarget)
            {
                AsyncState state = new AsyncState();
                _MineProcessors.Add((blueMine, new MovingCube[] { blueMineTarget }, new AsyncState[] { state }));
                blueMineTarget.DestroySelf(state);
                destroyedCubes.Add(blueMineTarget);
                triggeredMines.Add(MineManager.ArmedBlueMine);
                _ClockWatch = WaveSettings.DelayBetweenTurns;
            }

            var greenMines = MineManager.GreenMines;
            if(greenMines != null)
            {
                foreach(GreenMine m in greenMines)
                {
                    if (!m.Armed) break;

                    var greenTargets = m.TryGetTargetCube();
                    if (greenTargets == null) continue;

                    AsyncState[] states = new AsyncState[greenTargets.Count];

                    MovingCube[] cubes = new MovingCube[greenTargets.Count];
                    triggeredMines.Add(m);
                    _ClockWatch = WaveSettings.DelayBetweenTurns;

                    for(int i=0; i<greenTargets.Count; i++)
                    {
                        MovingCube current = greenTargets[i];
                        cubes[i] = current;
                        AsyncState state = new AsyncState();
                        states[i] = state;
                        current.DestroySelf(state);
                        destroyedCubes.Add(current);
                    }

                    _MineProcessors.Add((m, cubes, states));
                }
            }

            _Wave.RemoveCubes(destroyedCubes);
            MineManager.RemoveMines(triggeredMines);

            if (OnGoingDestructions()) // Bypass clock watch if cubes are being destroyed
                return;

            _ClockWatch -= dt;
        }

        private bool OnGoingDestructions()
        {
            List<BaseMine> toDestroy = new List<BaseMine>();

            foreach(var valueTuple in _MineProcessors)
            {
                bool isFinished = true;
                foreach(AsyncState state in valueTuple.Item3)
                {
                    if(!state.IsCompleted)
                    {
                        isFinished = false;
                        break;
                    }
                }

                if(isFinished)
                {
                    toDestroy.Add(valueTuple.Item1);
                }
            }

            List<ValueTuple<BaseMine, MovingCube[], AsyncState[]>> tupleBuffer = new List<(BaseMine, MovingCube[], AsyncState[])>(_MineProcessors);

            foreach(var valueTuple in _MineProcessors)
            {
                foreach(BaseMine currentMine in toDestroy)
                {
                    if(valueTuple.Item1 == currentMine)
                    {
                        List<MineGraphics> greenMines = new List<MineGraphics>();
                        foreach(MovingCube cube in valueTuple.Item2)
                        {
                            if (cube.Type == CubeTypeEnum.Green)
                                greenMines.Add(currentMine.Graphics);
                        }

                        tupleBuffer.Add(valueTuple);
                        currentMine.Destroy();

                        foreach(MineGraphics gfx in greenMines)
                        {
                            MineManager.CreateGreenMine(gfx);
                        }
                    }
                }
            }

            _MineProcessors = tupleBuffer;
            return _MineProcessors.Count > 0;
        }
    } 
}
