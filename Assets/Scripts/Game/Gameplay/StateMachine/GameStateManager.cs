using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Kurushi.StateMachine;
using System;

public class GameStateManager : MonoBehaviour
{
    [SerializeField] private WaveDatabase _Database;
    [SerializeField] private GridAuthoring _GridAuthor;
    [SerializeField] private MovingCube[] _CubesPrefabs;
    [SerializeField] private CharacterController _Player;
    [SerializeField] private CameraManager _CamManager;

    [Space]
    [SerializeField] private float _SpawnDuration;
    [SerializeField] private float _SpawnDelay;
    [SerializeField] private float _TurnDuration;
    [SerializeField] private float _DelayBetweenTurns;

    [Space]
    [SerializeField] private int _WaveWidth;
    [SerializeField] private int _WaveCount;


    private Transform _WavesParent;
    private List<InstanceWave> _InstanceWaves;
    private InstanceWave _CurrentWave;

    private BaseState _CurrentState;

    // Start is called before the first frame update
    private void OnEnable()
    {
        _WavesParent = new GameObject("WAVES").transform;

        WaveSettings.InitializeValues(
            _SpawnDelay,
            _SpawnDuration,
            _TurnDuration,
            _DelayBetweenTurns
        );
    }

    private void StartLevel()
    {
        if(!_GridAuthor.HasGrid)
        {
            throw new System.NullReferenceException("Grid not initialized");
        }

        MineManager.Reset();
        for (int i = 0; i < _WavesParent.childCount; i++)
        {
            Destroy(_WavesParent.GetChild(0).gameObject);
        }

        List<Wave> waveSelection = new List<Wave>();
        for(int i=0; i<_WaveCount; i++)
        {
            waveSelection.Add(_Database.GetRandomWave(_WaveWidth, waveSelection));
        }


        _InstanceWaves = new List<InstanceWave>();
        int count = 1;
        Vector3 gridStart = _GridAuthor.GetFirstLinePosition();

        foreach(Wave wave in waveSelection)
        {
            var waveParent = new GameObject("wave " + count++);
            waveParent.transform.SetParent(_WavesParent);

            var waveInstance = wave.CreateInstance(_CubesPrefabs);
            waveInstance.PositionContentAndReturnLastPosition(gridStart);
            waveInstance.SetParents(waveParent.transform);
            _InstanceWaves.Add(waveInstance);
        }

        _InstanceWaves.Reverse();
        StartNewWave();
    }

    private void StartNewWave()
    {
        _CurrentWave = _InstanceWaves[0];
        SetState(new StateSpawnWave(_CurrentWave), new StateTransition(1f, () => _CamManager.SetDolly(true)));
    }

    private void SetState(BaseState @new, StateTransition transition = null)
    {
        _CurrentState = @new;
        _CurrentState.OnStatusChanged += HandleStateStatus;

        if(transition == null)
            transition = new StateTransition();

        StartCoroutine(TransitionToNewState(transition));
    }

    IEnumerator TransitionToNewState(StateTransition transition)
    {
        yield return transition.Execute();
        _CurrentState.EnterState();
    }

    private void FixedUpdate()
    {
        _CurrentState?.FixedUpdate(Time.fixedDeltaTime);
    }

    private void HandleStateStatus(StateStatusEnum newStatus)
    {
        if (newStatus != StateStatusEnum.Exiting) return;

        _CurrentState.OnStatusChanged -= HandleStateStatus;

        BaseState nextState = null;
        StateTransition transition = null;

        switch (_CurrentState)
        {
            case StateSpawnWave:
                transition = new StateTransition(1f, end: () => _CamManager.SetDolly(false));
                nextState = new StateAdvanceCube(_CurrentWave);
                break;
            case StateAdvanceCube:
                nextState = _Player.Dead ? new StateDiscardWave(_CurrentWave) : new StateProcessMines(_CurrentWave);
                break;
            case StateProcessMines:
                if(_CurrentWave.IsRoundFinished())
                {
                    StartNewWave();
                    return;
                }
                nextState = new StateAdvanceCube(_CurrentWave);
                break;
            case StateDiscardWave:
                StartNewWave();
                return;
        }

        SetState(nextState, transition);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            MineManager.Reset();
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            return;
        }

        if (Keyboard.current.pKey.wasPressedThisFrame && _CurrentWave == null)
        {
            StartLevel();
        }
    }

    [SerializeField] private bool _ShouldDisplayInfo;
    private void OnGUI()
    {
        if (Keyboard.current.f1Key.wasPressedThisFrame)
            _ShouldDisplayInfo = !_ShouldDisplayInfo;

        if (true)
        {
            Rect r = new Rect(10, -10, 300, 100);
            RectOffset rContent = new RectOffset(10, 10, 10, 10);

            GUI.Box(r, GUIContent.none);

            GUILayout.BeginArea(rContent.Remove(r));
            GUILayout.BeginVertical();

            Line("PendingBlueMine :", MineManager.HasPendingBlueMine ? "Yes" : "None");
            Line("ArmedBlueMine:", MineManager.HasArmedBlueMine ? "Yes" : "None");
            Line("Greenmines:", MineManager.GreenMines != null && MineManager.GreenMines.Length > 0 ? $"Yes, {MineManager.GreenMines.Length} " : "None");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }

    private void Line(string title, string content)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label(title);
        GUILayout.Label(content);

        GUILayout.EndHorizontal();
    }
#endif
}
