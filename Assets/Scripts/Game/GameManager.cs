using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

using Random = UnityEngine.Random;
public class GameManager : MonoBehaviour
{
    public event Action<GameStateEnum> OnGameStateChanged;
    [SerializeField] private CharacterController _CharaController;

    [Space]
    [SerializeField] private WaveDatabase _Database;
    [SerializeField] private int _TargetWidth;
    [SerializeField] private GridAuthoring _GridAuthor;
    [SerializeField] private MovingCube[] _CubePrefabs;

    [Space]
    [SerializeField] private float _SpawnDuration;
    [SerializeField] private float _SpawnDelay;
    [SerializeField] private float _AfterSpawnDelay = 1f;

    [Space]
    [SerializeField] private float _LineFallForce;
    [SerializeField] private float _LineFallTorque;
    [SerializeField] private float _LineFallDuration;

    private List<Wave> _SelectedWaves;
    private List<InstanceWave> _InstanceWaves;
    private InstanceWave _CurrentWave;
    private int _LostCubes;
    [Space]
    [SerializeField] private float _TurnDuration;
    [SerializeField] private float _DelayBetweenTurns;

    private bool _Blocked;
    private Coroutine _TurnProcessor;
    private bool _PlayerIsDead;
    private Transform _WavesParent;
    private GameStateEnum _GameState;
    public GameStateEnum GameState
    {
        get => _GameState;
        private set
        {
            Debug.Log("Settings game state to " + value.ToString());
            _GameState = value;
            this.OnGameStateChanged?.Invoke(value);
        }
    }
    private void OnEnable()
    {
        _CharaController.OnPlayerDeathChanged += PlayerIsDead;
        _WavesParent = new GameObject("WAVES").transform;

        WaveSettings.InitializeValues(
            _SpawnDelay,
            _SpawnDuration,
            _TurnDuration,
            _DelayBetweenTurns
        );
    }

    private void OnDisable()
    {
        _CharaController.OnPlayerDeathChanged -= PlayerIsDead;
    }



    private void PlayerIsDead(bool value)
    {
        _PlayerIsDead = value;
    }

    public void StartLevel()
    {
        MineManager.Reset();
        int waveCount = Random.Range(3, 5);
        if (_SelectedWaves == null)
            _SelectedWaves = new List<Wave>();
        else _SelectedWaves.Clear();

        for (int i = 0; i < waveCount; i++)
        {
            _SelectedWaves.Add(_Database.GetRandomWave(_TargetWidth, _SelectedWaves));
        }

        InstanceWave[] waveInstances = new InstanceWave[_SelectedWaves.Count];

        for (int i = 0; i < _WavesParent.childCount; i++)
        {
            Destroy(_WavesParent.GetChild(0).gameObject);
        }

        _InstanceWaves = new List<InstanceWave>();
        Vector3 linePos = _GridAuthor.GetFirstLinePosition();
        int cnt = 1;

        for (int i = 0; i < _SelectedWaves.Count; i++)
        {
            Wave w = _SelectedWaves[i];

            Transform waveParent = new GameObject().transform;
            waveParent.gameObject.name = "wave" + cnt;
            waveParent.SetParent(_WavesParent);

            InstanceWave currentWave = w.CreateInstance(_CubePrefabs);
            waveInstances[i] = currentWave;
            currentWave.PositionContentAndReturnLastPosition(linePos);

            currentWave.SetParents(waveParent);
            cnt++;
        }

        for (int i = 0; i < waveInstances.Length; i++)
        {
            _InstanceWaves.Add(waveInstances[waveInstances.Length - 1 - i]);
        }

        StartRound();
    }

    private IEnumerator AnimateCubeSpawn(InstanceWave wave)
    {
        GameState = GameStateEnum.WaveSpawning;

        List<AsyncState> states = new List<AsyncState>();

        foreach (List<MovingCube> line in wave)
        {
            AsyncState state = new AsyncState();
            states.Add(state);
            StartCoroutine(PushLineUp(line, _SpawnDuration, state));
            yield return new WaitForSeconds(_SpawnDelay);
        }

        bool animationsOnGoing = true;

        yield return new WaitWhile(() =>
        {
            animationsOnGoing = false;

            foreach (AsyncState state in states)
                if (!state.IsCompleted)
                {
                    animationsOnGoing = true;
                    break;
                }

            return animationsOnGoing;
        });

        _Blocked = true;

        while(_Blocked)
        {
            yield return null;
        }

        yield return new WaitForSeconds(_AfterSpawnDelay);
    }

    public void Unblock()
    {
        _Blocked = false;
    }


    public IEnumerator PushLineUp(List<MovingCube> line, float duration, AsyncState state)
    {
        int stepPerSec = Mathf.CeilToInt(1 / Time.fixedDeltaTime);
        int durationInStep = Mathf.CeilToInt(stepPerSec * duration);
        float displacementPerStep = 1f / durationInStep;

        for (int i = 0; i < durationInStep; i++)
        {
            foreach (MovingCube cube in line)
            {
                cube.transform.Translate(Vector3.up * displacementPerStep);
            }

            yield return new WaitForFixedUpdate();
        }

        state.Complete();
    }

    private void StartRound()
    {
        _TurnProcessor = StartCoroutine(LevelLoop());
    }

    IEnumerator LevelLoop()
    {
        while (_InstanceWaves.Count > 0)
        {
            _CurrentWave = _InstanceWaves[0];

            yield return AnimateCubeSpawn(_CurrentWave);

            while (!_CurrentWave.IsRoundFinished())
            {
                yield return MoveRoundCubes();
                yield return ProcessMines();
            }

            _InstanceWaves.Remove(_CurrentWave);
        }
    }

    private IEnumerator MoveRoundCubes()
    {
        GameState = GameStateEnum.AdvancingWaveCubes;
        yield return MoveCubes(_CurrentWave.GetAllCubes());

        ProcessGarbageCubes();

        if (_PlayerIsDead)
        {
            StopCoroutine(_TurnProcessor);
            StartCoroutine(DiscardWave());
        }
    }

    private IEnumerator MoveCubes(List<MovingCube> allCubes)
    {
        int secStep = Mathf.CeilToInt(1f / Time.fixedDeltaTime);
        int stepCount = Mathf.CeilToInt(WaveSettings.TurnDuration * secStep);
        float dAngle = -90f / stepCount;

        foreach (MovingCube cube in allCubes)
            cube.PrepareToMove();

        for (int i = 0; i < stepCount; i++)
        {
            foreach (MovingCube c in allCubes)
            {
                c.UpdateMove(dAngle);
            }
            yield return new WaitForFixedUpdate();
        }

        foreach (MovingCube m in allCubes)
            m.CompleteMovement();
    }

    private IEnumerator DiscardWave()
    {
        MineManager.Reset();

        GameState = GameStateEnum.DiscardingWave;

        WaveSettings.FastForward = true;


        while (!_CurrentWave.IsRoundFinished())
        {
            yield return MoveCubes(_CurrentWave.GetAllCubes());

            ProcessGarbageCubes();

            yield return new WaitForSeconds(WaveSettings.DelayBetweenTurns);
        }

        WaveSettings.FastForward = false;

        _InstanceWaves.Remove(_CurrentWave);

        yield return _CharaController.StandUp();
        yield return new WaitForSeconds(2f);

        StartRound();
    }

    private IEnumerator ProcessGarbageCubes()
    {
        List<MovingCube> fallen = new List<MovingCube>();
        foreach (List<MovingCube> line in _CurrentWave)
        {
            foreach (MovingCube c in line)
            {
                if (!c.IsGrounded)
                {
                    if (c.Type == CubeTypeEnum.Standard)
                    {
                        _LostCubes++;
                    }
                    fallen.Add(c);
                }
            }
        }

        int lineLost = Mathf.FloorToInt(_LostCubes / 3);

        if (lineLost > 0 && false)
        {
            GameStateEnum currentState = GameState;
            GameState = GameStateEnum.RemovingLine;

            List<AsyncState> states = new List<AsyncState>();

            for (int i = 0; i < lineLost; i++)
            {
                AsyncState state = new AsyncState();
                states.Add(state);
                StartCoroutine(DropLine(state));
                yield return new WaitForSeconds(.2f);
            }

            bool looping = true;

            while (looping)
            {
                looping = false;

                foreach (AsyncState s in states)
                {
                    if (!s.IsCompleted)
                        looping = true;
                }
            }
            GameState = currentState;
        }

        _CurrentWave.RemoveCubes(fallen);
        yield break;
    }

    private IEnumerator DropLine(AsyncState state)
    {
        _GridAuthor.GetLastLine().Fall(_LineFallForce, _LineFallTorque);
        yield return new WaitForSeconds(_LineFallDuration);
        state.Complete();
    }

    private IEnumerator ProcessMines()
    {
        GameState = GameStateEnum.ProcessingMines;

        List<IEnumerator> toDestroy = new List<IEnumerator>();
        List<AsyncState> toDestroyStates = new List<AsyncState>();
        List<BaseMine> activatedMines = new List<BaseMine>();
        List<MovingCube> destroyedCubes = new List<MovingCube>();

        float clockwatch = 0f;

        while (clockwatch < WaveSettings.DelayBetweenTurns)
        {
            if (MineManager.HasArmedBlueMine)
            {
                MovingCube target = MineManager.ArmedBlueMine.TryGetTargetCube();

                if (target != null)
                {
                    AsyncState current = new AsyncState();
                    toDestroy.Add(DestroyCube(target, current));
                    activatedMines.Add(MineManager.ArmedBlueMine);
                    toDestroyStates.Add(current);
                    destroyedCubes.Add(target);
                }
            }

            //GreenMines
            var greenMines = MineManager.GreenMines;
            if (greenMines != null && greenMines.Length > 0)
            {
                foreach (GreenMine m in greenMines)
                {
                    if (!m.Armed) break;

                    List<MovingCube> targets = m.TryGetTargetCube();

                    if (targets == null)
                    {
                        continue;
                    }

                    foreach (MovingCube target in targets)
                    {
                        AsyncState current = new AsyncState();
                        toDestroy.Add(DestroyCube(target, current));
                        toDestroyStates.Add(current);
                    }

                    destroyedCubes.AddRange(targets);
                    activatedMines.Add(m);
                }
            }

            if (activatedMines.Count > 0)
            {
                foreach (BaseMine b in activatedMines)
                {
                    MineManager.RemoveMine(b);
                }

                activatedMines.Clear();


                foreach (IEnumerator iterators in toDestroy)
                    StartCoroutine(iterators);

                bool cubesDestroyed = true;

                foreach (AsyncState state in toDestroyStates)
                {
                    if (!state.IsCompleted)
                        cubesDestroyed = false;
                }

                while (!cubesDestroyed)
                {
                    clockwatch = 0f;
                    cubesDestroyed = true;
                    foreach (AsyncState state in toDestroyStates)
                    {
                        if (!state.IsCompleted)
                            cubesDestroyed = false;
                    }
                    yield return null;
                }

                _CurrentWave.RemoveCubes(destroyedCubes);
                toDestroy.Clear();
                destroyedCubes.Clear();
            }

            clockwatch += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator DestroyCube(MovingCube target, AsyncState state)
    {
        if (!target)
        {
            state.Complete();
            yield break;
        }

        MineGraphics graphics = null;
        Physics.Raycast(new Ray(target.transform.position, Vector3.down), out RaycastHit hit, LayerMask.GetMask(Layers.GROUND_CUBE));

        if (!hit.collider)
        {
            Debug.LogError("Didn't find any MineGraphics. Shouldn't happen");
            Debug.Break();
        }

        graphics = hit.collider.GetComponentInParent<MineGraphics>();

        switch (target.Type)
        {
            case CubeTypeEnum.Green:
                if (graphics.Used)
                {
                    Debug.Log("Trying to create green mine on a used tile ?");
                }
                MineManager.CreateGreenMine(graphics);
                break;
        }

        //yield return target.Destroy();
        yield return null;
        state.Complete();
    }


#if DEVELOPMENT_BUILD || UNITY_EDITOR
    [SerializeField] private bool _ShouldDisplayInfo;
    private void OnGUI()
    {
        if (Keyboard.current.f1Key.wasPressedThisFrame)
            _ShouldDisplayInfo = !_ShouldDisplayInfo;

        if (_ShouldDisplayInfo)
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

#if UNITY_EDITOR
    [CustomEditor(typeof(GameManager))]
    private class GameManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isEditor && !Application.isPlaying) return;

            if (GUILayout.Button("Debug Start"))
            {
                ((GameManager)target).StartLevel();
            }

            if (GUILayout.Button("Start Round"))
            {
                ((GameManager)target).StartRound();
            }
        }
    }

#endif
}

public enum GameStateEnum
{
    AdvancingWaveCubes,
    ProcessingMines,
    WaveSpawning,
    DiscardingWave,
    RemovingLine,
    AddingLine
}
