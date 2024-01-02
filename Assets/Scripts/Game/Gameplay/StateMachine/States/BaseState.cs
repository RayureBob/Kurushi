using System;
using System.Collections;
using UnityEngine;

namespace Kurushi.StateMachine
{
    public abstract class BaseState
    {
        public event Action OnStateEntered;
        public event Action OnStateExited;
        public event Action<StateStatusEnum> OnStatusChanged;
        private StateStatusEnum _currentStatus = StateStatusEnum.Starting;
        public StateStatusEnum Status
        {
            get => _currentStatus;
            set
            {
                _currentStatus = value;
                OnStatusChanged?.Invoke(_currentStatus);
            }
        }

        protected abstract bool shouldExit { get; }

        public void EnterState()
        {
            OnStateEntered?.Invoke();
            OnEnter();
            Status = StateStatusEnum.Running;
            OnStatusChanged?.Invoke(Status);
        }

        public void FixedUpdate(float dt)
        {
            if (Status != StateStatusEnum.Running) return;

            InternalFixedUpdate(dt);

            if(shouldExit)
            {
                Status = StateStatusEnum.Exiting;
                ExitState();
            }
        }


        public void ExitState()
        {
            OnExit();
            Status = StateStatusEnum.Dead;
            OnStateExited?.Invoke();
        }

        protected virtual void OnEnter() { }
        protected virtual void OnExit() { }

        protected abstract void InternalFixedUpdate(float dt);
    }

    public class StateTransition
    {
        private float _Duration;
        private Action _Start;
        private Action _End;

        public StateTransition(float duration = 0f, Action start = null, Action end = null)
        {
            _Duration = duration;
            _Start = start;
            _End = end;
        }

        public IEnumerator Execute()
        {
            _Start?.Invoke();
            yield return new WaitForSeconds(_Duration);
            _End?.Invoke();
        }
    }

    public enum StateStatusEnum
    {
        Starting,
        Running,
        Exiting,
        Dead
    }

}