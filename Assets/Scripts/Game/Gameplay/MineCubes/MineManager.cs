using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/*  TODO
 * 
 *  Since instance subscribes to sceen reload, make sure the singleton lives until
 *  the game process is destroyed
 * 
 */
public class MineManager
{
    private static MineManager _Instance;

    private BlueMine _PendingBlueMine;
    private BlueMine _ArmedBlueMine;
    private List<GreenMine> _GreenMines;

    public static BlueMine ArmedBlueMine => _Instance?._ArmedBlueMine;

    public static bool HasArmedBlueMine 
    {
        get
        {
            if (_Instance == null) return false;
            return _Instance._ArmedBlueMine != null;
        }
    }
    public static bool CanArmBlueMine
    {
        get
        {
            if (_Instance == null) return false;
            return _Instance._PendingBlueMine != null && _Instance._ArmedBlueMine == null;
        }
    }

    public static bool HasPendingBlueMine
    {
        get
        {
            if (_Instance == null) return false;
            return _Instance._PendingBlueMine != null;
        }
    }

    public static void CreateBlueMine(MineGraphics targetGraphics)
    {
        VerifySingleton();
        _Instance._PendingBlueMine = new BlueMine(targetGraphics);
    }

    public static void CreateGreenMine(MineGraphics targetGraphics)
    {
        if (targetGraphics.Used)
            throw new System.InvalidOperationException("Tried to create a mine on a used MineGraphics");

        VerifySingleton();
        _Instance._GreenMines.Add(new GreenMine(targetGraphics));
    }

    public static void ArmBlueMine()
    {
        if (_Instance == null) return;
        _Instance._PendingBlueMine.Arm();
        _Instance._ArmedBlueMine = _Instance._PendingBlueMine;
        _Instance._PendingBlueMine = null;
    }

    public static void ArmGreenMines()
    {
        Debug.Log("Arming Green mines");
        if (_Instance == null) return;

        foreach(GreenMine m in _Instance._GreenMines)
        {
            m.Arm();
        }
    }

    public static GreenMine[] GreenMines => _Instance?._GreenMines?.ToArray();

    public static void RemoveMine(BaseMine mine)
    {
        if(mine is BlueMine && mine == _Instance._ArmedBlueMine)
        {
            _Instance._ArmedBlueMine = null;
            return;
        }

        if(mine is GreenMine gMine && _Instance._GreenMines.Contains(gMine))
        {
            _Instance._GreenMines.Remove(gMine);
            return;
        }

        Debug.LogError("Tried to delete a orphan mine. This should never happen");
    }

    public static void RemoveMines(IEnumerable<BaseMine> mines)
    {
        foreach (BaseMine m in mines)
            RemoveMine(m);
    }

    public static void Reset()
    {
        if (_Instance == null) return;
        _Instance._PendingBlueMine = null;
        _Instance._ArmedBlueMine = null;
        _Instance._GreenMines.Clear();
    }

    private MineManager()
    {
        this._GreenMines = new List<GreenMine>();
        SceneManager.sceneLoaded += ResetInternal;

    }

    private void ResetInternal(Scene s, LoadSceneMode m)
    {
        Reset();
    }

    private static void VerifySingleton()
    {
        if (_Instance == null)
            _Instance = new MineManager();
    }
#if UNITY_EDITOR
    public class MineManagerEditor : EditorWindow
    {
        [MenuItem("SINGLETONS/Mine Manager")]
        private static void Open()
        {
            GetWindow<MineManagerEditor>().Show();
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                bool initialized = _Instance != null;
                string v = initialized ? "INITIALIZED" : "UNDEFINED";
                Color c = initialized ? Color.green : Color.red;

                GUI.color = c;
                GUILayout.Label(v);
                GUI.color = Color.white;

                if(initialized)
                {
                    if(GUILayout.Button("Set To Null"))
                    {
                        Reset();
                        if(_Instance != null)
                        {
                            _Instance = null;
                            SceneManager.sceneLoaded -= _Instance.ResetInternal;
                        }
                    }
                }
                else
                {
                    if(GUILayout.Button("Create"))
                    {
                        _Instance = new MineManager();
                    }
                }
            }
        }
    }
#endif
}
