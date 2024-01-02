using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName ="Database", menuName = "Game/Database")]
public class WaveDatabase : ScriptableObject
{
    [SerializeField] private List<Wave> _Waves;
    public List<Wave> Waves => _Waves;

    public Wave GetRandomWave(int width, List<Wave> exclude = null)
    {
        List<Wave> candidates = new List<Wave>();

        foreach (IGrouping<int, Wave> group in _Waves.GroupBy(w => w.Width))
        {
            if (group.Key == width)
            {
                foreach (Wave w in group)
                {
                    candidates.Add(w);
                }
                break;
            }
        }

        if(exclude != null)
        {
            foreach (Wave w in exclude)
            {
                candidates.Remove(w);
            }
        }

        return candidates[Random.Range(0, candidates.Count - 1)];
    }

    [CustomEditor(typeof(WaveDatabase))]
    private class WaveEditor : Editor
    {
        private const string _WavesFolderPath = "C:\\Users\\virgi\\Desktop\\Projets Unity\\Beast\\Kurushi\\Assets\\Data\\Waves";
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button("Fetch waves"))
            {
                string[] paths = Directory.GetFiles(_WavesFolderPath).Where(s => !s.Contains(".meta")).ToArray();

                if (paths.Length < 1) return;

                WaveDatabase t = target as WaveDatabase;
                if (t._Waves == null) t._Waves = new List<Wave>();
                else t._Waves.Clear();

                foreach(string s in paths)
                {
                    Wave asset = (Wave)AssetDatabase.LoadAssetAtPath(s.Split("Kurushi\\")[1], typeof(Wave));
                    if(asset)
                    {
                        t._Waves.Add(asset);
                        EditorUtility.SetDirty(target);
                    }
                }
            }
        }
    }
}
