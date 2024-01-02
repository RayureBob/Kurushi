using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Wave", menuName = "Game/Wave")]
public class Wave : ScriptableObject
{
    [SerializeField] private int _Par;
    [SerializeField] private int _RowWidth;
    [SerializeField] private int _RowCount;

    [SerializeField] private List<Row> _Content;

    [SerializeField] private Row _Row;
    public Row this[int i] => _Content[i];
    public int RowCount => _Content.Count;
    public int TotalCubeCount => _Content.Count * _RowWidth;
    public int Width => _RowWidth;

    private void AddRows(int cnt)
    {
        if (_Content == null)
        {
            _Content = new List<Row>();
        }
        for (int i = 0; i < cnt; i++)
            _Content.Add(new Row(new CubeTypeEnum[_RowWidth]));
    }

    private void RemoveRows(int cnt)
    {
        if(_Content == null && cnt > 0)
        {
            _Content = new List<Row>();
        }

        if (cnt >= _Content.Count)
        {
            _Content.Clear();
            return;
        }

        for(int i=0; i<cnt; i++)
        {
            _Content.RemoveAt(_Content.Count - 1);
        }
    }

    private void ClearGrid()
    {
        foreach (Row r in _Content) r.ClearRow();
    }

    private void UpdateContent()
    {
        if(_Content.Count < 1)
        {
            _Content = new List<Row>();

            for (int i = 0; i < _RowCount; i++)
                _Content.Add(new Row(new CubeTypeEnum[_RowWidth]));

            return;
        }

        int depthDiff = _RowCount - _Content.Count;
        int widthDiff = _RowWidth - _Content[0].Content.Length;

        if (widthDiff < 0)
        {
            foreach (Row r in _Content)
                r.RemoveContent(-widthDiff);
        }
        else if (widthDiff > 0)
        {
            foreach (Row r in _Content)
                r.AppendContent(widthDiff);
        }

        if (depthDiff > 0) AddRows(depthDiff);
        else if (depthDiff < 0) RemoveRows(-depthDiff);
    }

    public InstanceWave CreateInstance(MovingCube[] prefabs)
    {
        List<List<MovingCube>> cubes = new List<List<MovingCube>>();

        int cubeIndex = 0;
        List<MovingCube> currentRow;
        foreach(Row row in _Content)
        {
            currentRow = new List<MovingCube>();

            for(int i=0; i<_RowWidth; i++)
            {
                foreach(MovingCube prefab in prefabs)
                {
                    if(prefab.Type == row[i])
                    {
                        currentRow.Add(Instantiate(prefab));
                    }
                }
            }

            cubes.Add(currentRow);
        }

        return new InstanceWave(cubes);
    }

    [Serializable]
    public class Row
    {
        public CubeTypeEnum[] Content;

        public CubeTypeEnum this[int index] => Content[index];

        public Row(CubeTypeEnum[] content)
        {
            this.Content = content;
        }

        public void AppendContent(int count)
        {
            List<CubeTypeEnum> content = new List<CubeTypeEnum>(Content);

            for(int i=0; i<count; i++)
            {
                content.Add(default);
            }

            Content = content.ToArray();
        }

        public void RemoveContent(int count)
        {
            List<CubeTypeEnum> content = new List<CubeTypeEnum>(Content);

            for (int i = 0; i < count; i++)
            {
                if (content.Count == 0) break;
                content.RemoveAt(content.Count - 1);
            }

            Content = content.ToArray();
        }

        public void ClearRow()
        {
            for(int i=0; i<Content.Length; i++)
                Content[i] = default;
        }
    }

    [CustomEditor(typeof(Wave))]
    private class WaveAuthoring : Editor
    {
        private static Texture2D[] _Textures;

        private void OnEnable()
        {
            if (_Textures != null) return;

            _Textures = new Texture2D[3];

            _Textures[0] = new Texture2D(64, 64);
            SetTexturePixels(_Textures[0], Color.grey);

            _Textures[1] = new Texture2D(64, 64);
            SetTexturePixels(_Textures[1], Color.black);

            _Textures[2] = new Texture2D(64, 64);
            SetTexturePixels(_Textures[2], Color.green);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Clear grid"))
                ((Wave)target).ClearGrid();

            SerializedProperty rowWidth = serializedObject.FindProperty("_RowWidth");
            SerializedProperty rowCount = serializedObject.FindProperty("_RowCount");

            DrawIntFieldWithButtons(rowWidth);
            DrawIntFieldWithButtons(rowCount);

            DrawRowButtons();

            if(serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                (target as Wave).UpdateContent();
            }
        }

        private void DrawIntFieldWithButtons(SerializedProperty targetField)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("-", GUILayout.MinWidth(50f)))
            {
                if (targetField.intValue > 0)
                    SetIntValue(targetField.intValue - 1, targetField);
            }

            GUIStyle s = EditorStyles.boldLabel;
            s.alignment = TextAnchor.MiddleCenter;
            s.normal.textColor = s.normal.textColor + Color.white * .1f;

            int v = EditorGUILayout.IntField(targetField.intValue, s, GUILayout.MaxWidth(100));
            if (v >= 0) SetIntValue(v, targetField);

            Rect r = GUILayoutUtility.GetLastRect();
            GUI.Box(r, GUIContent.none);

            if (GUILayout.Button("+", GUILayout.MinWidth(50f)))
            {
                SetIntValue(targetField.intValue + 1, targetField);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void SetIntValue(int newValue, SerializedProperty targetField)
        {
            targetField.intValue = newValue;
        }

        private void DrawRowButtons()
        {
            SerializedProperty waveContent = serializedObject.FindProperty("_Content");
            SerializedProperty currentRow;
            if (waveContent.arraySize < 1) return;

            GUILayout.FlexibleSpace();

            try
            {
                EditorGUILayout.BeginVertical();
                for (int i = 0; i < waveContent.arraySize; i++)
                {
                    currentRow = waveContent.GetArrayElementAtIndex(i);
                    if (currentRow == null) break;

                    SerializedProperty array = currentRow.FindPropertyRelative("Content");
                    if (array == null) break;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    for (int j=0; j< array.arraySize; j++)
                    {
                        int value = array.GetArrayElementAtIndex(j).intValue;
                        if(GUILayout.Button(_Textures[value], GUILayout.MaxWidth(64)))
                        {
                            array.GetArrayElementAtIndex(j).intValue = (value + 1) % Enum.GetValues(typeof(CubeTypeEnum)).Length;
                        }
                    }

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            catch(Exception e)
            {
                Debug.Log(e.Message);
            }
            GUILayout.FlexibleSpace();
        }

        private void SetTexturePixels(Texture2D target, Color c)
        {
            for(int i=0; i<target.height; i++)
            {
                for(int j=0; j<target.width; j++)
                {
                    target.SetPixel(i, j, c);
                }
            }

            target.Apply();
        }
    }
}
