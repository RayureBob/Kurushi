using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEditor;

public class MovingCube : MonoBehaviour
{
    [SerializeField] private CubeTypeEnum _CubeType;
    [SerializeField] private Transform _CubeTransform;

    [Space]
    [SerializeField] private float _DestructionDuration;
    [SerializeField] private float _MaxDestructionColumnHeight = 1.5f;
    [SerializeField] private AnimationCurve _DestructionAnimationCurve;

    public bool IsMoving => _IsMoving;
    private bool _IsMoving;
    private bool _IsGrounded = true;
    private bool _IsDetroyed = true;
    public bool IsGrounded => _IsGrounded;
    float _counter;
    Vector3 _RotationPoint;

    public CubeTypeEnum Type => _CubeType;
    // Start is called before the first frame update

    public void PrepareToMove()
    {
        _RotationPoint = transform.position + Vector3.back * .5f + Vector3.down * .5f;
        _IsMoving = true;
    }

    public void UpdateMove(float dt)
    {
        transform.RotateAround(_RotationPoint, Vector3.right, dt);
    }

    public void CompleteMovement()
    {
        _IsMoving = false;
        _IsGrounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit h, LayerMask.GetMask(Layers.GROUND_CUBE));
#if UNITY_EDITOR
        Color target = Color.red;
        if (h.collider) target = Color.green;
        Debug.DrawRay(transform.position, Vector3.down, target, 1f);
#endif
    }

    private void FixedUpdate()
    {
        if (_IsGrounded) return;
        if (_counter >= 2f)
        {
            gameObject.SetActive(false);
            return;
        }

        transform.Translate(Vector3.down * -Physics.gravity.y * Time.fixedDeltaTime, Space.World);
        _counter += Time.fixedDeltaTime;
    }

    public void DestroySelf(AsyncState state)
    {
        StartCoroutine(DestroyInternal(state));
    }

    private IEnumerator DestroyInternal(AsyncState state)
    {
        Physics.Raycast(new Ray(transform.position, Vector3.down), out RaycastHit hit, LayerMask.GetMask(Layers.GROUND_CUBE));
        MineGraphics graphics = hit.collider?.GetComponent<MineGraphics>();

        if(!graphics)
        {
            throw new System.NullReferenceException("MineGraphics not found on destroy");
        }

        GetComponentInChildren<Collider>().enabled = false;
        int loopCount = CoroutineUtility.GetDurationInFixedStep(_DestructionDuration, true);

        for(int i=0; i<=loopCount; i++)
        {
            float dt = (float)i / loopCount;
            //transform.localScale = Vector3.one * (1f - _DestructionAnimationCurve.Evaluate(dt));
            transform.Translate(Vector3.down / loopCount, Space.World);
            graphics.SetDestructionGraphics(dt, _MaxDestructionColumnHeight);
            yield return new WaitForFixedUpdate();
        }

        Destroy(gameObject);
        state.Complete();
    }

    [CustomEditor(typeof(MovingCube))]
    private class MovingCubeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            bool grounded = ((MovingCube)target)._IsGrounded;

            GUI.color = grounded ? Color.green : Color.red;
            GUILayout.Label(grounded ? "GROUNDED" : "NOT GROUNDED");
            GUI.color = Color.white;
        }
    }
}
