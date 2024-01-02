using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GridAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject _ColumnPrefab;
    [SerializeField, HideInInspector] private Grid _Grid;

    [SerializeField] private int _Depth;
    [SerializeField] private int _Width;


    public bool HasGrid => _Grid != null;


    public GroundLine GetLastLine() => _Grid.GetLastLine();
    public void AddLineToGrid()
    {
        Vector3 linePosition = _Grid.GetLastLine().Position - Vector3.forward;
        Vector3 columnPosition = linePosition;
        GameObject[] currentLine = new GameObject[_Grid.Width];

        for(int i=0; i< _Grid.Width; i++)
        {
            GameObject column = (GameObject)PrefabUtility.InstantiatePrefab(_ColumnPrefab, transform);
            column.transform.position = columnPosition;
            currentLine[i] = column;
            columnPosition += Vector3.right;
        }

        _Grid.AddLine(new GroundLine(currentLine, linePosition));
        _Depth++;
    }

    public void DestroyLastLine()
    {
        GameObject[] lastLine = _Grid.RemoveAndReturnLastLine().Columns;

        foreach (GameObject g in lastLine)
        {
            Rigidbody rb = g.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.AddForce(Vector3.down, ForceMode.VelocityChange);
            rb.angularVelocity = new Vector3
            {
                x = Random.Range(-15f, 15f),
                y = Random.Range(-15f, 15f),
                z = Random.Range(-15f, 15f)
            };
        }
        _Depth--;
    }

    public Vector3 GetFirstLinePosition() => _Grid.GetFirstLine()[0].transform.position;

#if UNITY_EDITOR
    public void EDITOR_DestroyLastLine()
    {
        GameObject[] lastLine = _Grid.RemoveAndReturnLastLine().Columns;
        foreach (GameObject g in lastLine)
            DestroyImmediate(g);
    }

    public void EDITOR_DestroyGrid()
    {
        if (transform.childCount > 0)
        {
            int childCount = transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }
    }

    public void CreateGrid()
    {
        if (transform.childCount > 0)
        {
            int childCount = transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        List<GroundLine> lines = new List<GroundLine>();

        Vector3 linePosition = transform.position;
        GameObject[] currentLine;

        for (int i = 0; i < _Depth; i++)
        {
            Vector3 columnPosition = linePosition;
            currentLine = new GameObject[_Width];

            GameObject column;
            for (int j = 0; j < _Width; j++)
            {
                column = (GameObject)PrefabUtility.InstantiatePrefab(_ColumnPrefab, transform);
                column.transform.position = columnPosition;
                currentLine[j] = column;
                columnPosition += Vector3.right;
            }

            lines.Add(new GroundLine(currentLine, linePosition));
            linePosition -= Vector3.forward;
        }

        _Grid = new Grid(lines);
    }
#endif

    [CustomEditor(typeof(GridAuthoring))]
    private class GroundGeneratorEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GridAuthoring grid = target as GridAuthoring;
            if(grid._Grid == null)
            {
                EditorGUILayout.HelpBox("No grid has been found ! Generate one below", MessageType.Error);
            }

            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Create Grid", EditorStyles.miniButtonLeft))
            {
                ((GridAuthoring)target).CreateGrid();
            }

            if (GUILayout.Button("Add Column To Grid", EditorStyles.miniButtonMid))
            {
                ((GridAuthoring)target).AddLineToGrid();
            }

            if (GUILayout.Button("Destroy last line", EditorStyles.miniButtonMid))
            {
                ((GridAuthoring)target).EDITOR_DestroyLastLine();
            }

            if (GUILayout.Button("Destroy grid", EditorStyles.miniButtonRight))
            {
                ((GridAuthoring)target).EDITOR_DestroyGrid();
            }

            GUILayout.EndHorizontal();
        }
    }
}
