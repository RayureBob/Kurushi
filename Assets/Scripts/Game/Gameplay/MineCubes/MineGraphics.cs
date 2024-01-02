using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineGraphics : MonoBehaviour
{
    [SerializeField] private Renderer _MineRenderer;
    [SerializeField] private Color _BlueMineColor;
    [SerializeField] private Color _GreenMineColor;
    [SerializeField] private float _AnimationDuration;
    [SerializeField] private AnimationCurve _AnimationCurve;

    [Space]
    [SerializeField] private Transform _DestructionColumn;
    [SerializeField] private Renderer _ColumnRenderer;
    [SerializeField] private AnimationCurve _ColumnAnimationCurve;

    private MaterialPropertyBlock _MinePBlock;
    private MaterialPropertyBlock _ColumnPBlock;
    private bool _Used;
    public bool Used => _Used;

    private void OnEnable()
    {
        _MinePBlock = new MaterialPropertyBlock();
        _ColumnPBlock = new MaterialPropertyBlock();
    }

    public void SetBlueMine()
    {
        _Used = true;
        _MinePBlock.SetColor("_MineColor", _BlueMineColor);
        _MineRenderer.SetPropertyBlock(_MinePBlock);
        StartCoroutine(SetFloatValue("_ColorAmount", 1f));
    }

    public void SetGreenMine()
    {
        _Used = true;
        _MinePBlock.SetColor("_MineColor", _GreenMineColor);
        _MineRenderer.SetPropertyBlock(_MinePBlock);
        StartCoroutine(SetFloatValue("_ColorAmount", 1f));
    }

    public void SetArmedColor()
    {
        StartCoroutine(SetFloatValue("_ArmedColorAmount", 1f));
    }

    private IEnumerator SetFloatValue(string id, float targetValue)
    {
        int stepsInSec = Mathf.CeilToInt(1f / Time.fixedDeltaTime);
        int maxSteps = Mathf.CeilToInt(_AnimationDuration * stepsInSec);

        for (int i = 0; i < maxSteps; i++)
        {
            float value = _AnimationCurve.Evaluate((float)i / maxSteps);
            _MinePBlock.SetFloat(id, value * targetValue);
            _MineRenderer.SetPropertyBlock(_MinePBlock);
            yield return new WaitForFixedUpdate();
        }

        _MinePBlock.SetFloat(id, targetValue);
        _MineRenderer.SetPropertyBlock(_MinePBlock);
    }

    public void SetDestructionGraphics(float dt, float targetHeight)
    {
        _DestructionColumn.localScale = new Vector3
        {
            x = 1f,
            y = _ColumnAnimationCurve.Evaluate(dt) * targetHeight,
            z = 1f
        };
    }

    public void DisableGraphics()
    {
        StopAllCoroutines();

        _ColumnRenderer.SetPropertyBlock(_ColumnPBlock);
        _DestructionColumn.localScale = new Vector3
        {
            x = 1f,
            y = 0f,
            z = 1f
        };

        _MinePBlock.SetFloat("_ColorAmount", 0f);
        _MinePBlock.SetFloat("_ArmedColorAmount", 0f);
        _MineRenderer.SetPropertyBlock(_MinePBlock);

        _Used = false;
    }
}
