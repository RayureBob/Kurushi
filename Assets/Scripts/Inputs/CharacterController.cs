using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController : MonoBehaviour
{
    public event Action<bool> OnPlayerDeathChanged;

    [SerializeField] private PlayerInput _Inputs;
    [SerializeField] private Animator _Animator;
    [SerializeField] private Transform _Gfx;

    [Space]
    [SerializeField] private float _Speed;
    [SerializeField] private float _RotationLerpSpeed;
    [SerializeField, Min(0)] private float _CharCenterHeight;
    [SerializeField] private float _CharMovementRadius;
    [SerializeField] private float _CharKillRadius;

    private Vector3 _CharCenterWorld => transform.position + Vector3.up * _CharCenterHeight;
    private Collider[] _OverlapColliders;
    private Vector3 _Direction;
    private bool _Dead;
    private Vector3 _Displacement;

    public bool Dead => _Dead;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_CharCenterWorld, _CharMovementRadius);
        Gizmos.color = Color.white;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_CharCenterWorld, _CharKillRadius);
        Gizmos.color = Color.white;

        Physics.Raycast(new Ray(_CharCenterWorld, Vector3.down), out RaycastHit hit, LayerMask.GetMask(Layers.GROUND_CUBE));

        if(hit.collider)
        {
            bool groundContains = GroundCheck();
            Vector3 walkTestPos = _CharCenterWorld + _Displacement * _CharMovementRadius;
            Gizmos.color = groundContains ? Color.green : Color.red;
            Gizmos.DrawSphere(walkTestPos, .05f);
            Gizmos.color = Color.white;
        }
    }
#endif

    private void OnEnable()
    {
        _Inputs.onActionTriggered += HandleInput;
        _OverlapColliders = new Collider[1];
        _Dead = false;
    }

    private void OnDisable()
    {
        _Inputs.onActionTriggered -= HandleInput;
    }

    private void HandleInput(InputAction.CallbackContext context)
    {
        if (_Dead) return;
        switch(context.action.name)
        {
            case "Horizontal":
                _Direction.x = context.performed ? context.ReadValue<float>() : 0f;
                break;
            case "Vertical":
                _Direction.z = context.performed ? context.ReadValue<float>() : 0f;
                break;
            case "Cross":
                if(context.performed)
                {
                    if (MineManager.CanArmBlueMine)
                    {
                        MineManager.ArmBlueMine();
                        break;
                    }
                    else if(!MineManager.HasPendingBlueMine)
                    {
                        Physics.Raycast(_CharCenterWorld, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask(Layers.GROUND_CUBE));
                        if (!hit.collider) return;
                        var graphics = hit.collider.gameObject.GetComponentInParent<MineGraphics>();
                        if (!graphics) return;
                        MineManager.CreateBlueMine(graphics);
                    }
                }
                break;
            case "Circle":
                break;
            case "Triangle":
                if(context.performed)
                {
                    MineManager.ArmGreenMines();
                }
                break;
            case "Square":
                float value = context.ReadValue<float>();
                if(value > .5f)
                {
                    if (!WaveSettings.FastForward) WaveSettings.FastForward = true;
                }
                else
                {
                    if (WaveSettings.FastForward) WaveSettings.FastForward = false;
                }
                break;
        }
    }

    private bool GroundCheck()
    {
        bool res = false;
        Physics.Raycast(new Ray(_CharCenterWorld, Vector3.down), out RaycastHit hit, LayerMask.GetMask(Layers.GROUND_CUBE));

        if (hit.collider)
        {
            Vector3 walkTestPos = hit.point + _Displacement.normalized * _CharMovementRadius;
            res = hit.collider.bounds.Contains(walkTestPos);

            if (!res)
            {
                Collider[] c = Physics.OverlapSphere(walkTestPos, .01f, LayerMask.GetMask(Layers.GROUND_CUBE));
                res = c.Length > 0;
            }
        }

        return res;
    }

    private void FixedUpdate()
    {
        if (_Dead) return;

        _Displacement = _Direction * _Speed * Time.fixedDeltaTime;
        bool canMove = GroundCheck() && !WallCheck();

        _Animator.SetBool("Running", _Displacement != Vector3.zero && canMove);

        if(canMove)
        {
            Vector3 targetRotation = _Direction != Vector3.zero ? _Direction : Vector3.forward;
            _Gfx.forward = Vector3.Lerp(_Gfx.forward, targetRotation, _RotationLerpSpeed * Time.fixedDeltaTime);
            transform.Translate(_Displacement);
        }

        int count = Physics.OverlapSphereNonAlloc(_CharCenterWorld, _CharKillRadius, _OverlapColliders, LayerMask.GetMask(Layers.MOVING_CUBE));

        if(count > 0)
        {
            foreach(Collider collider in _OverlapColliders)
            {
                MovingCube cube = collider.GetComponentInParent<MovingCube>();

                if(cube.IsMoving)
                {
                    _Animator.SetTrigger("Dying");
                    _Dead = true;
                    OnPlayerDeathChanged?.Invoke(true);
                    break;
                }
            }
        }
    }

    public IEnumerator StandUp()
    {
        _Animator.SetTrigger("StandUp");
        yield return new WaitForSeconds(4.1f);
        _Dead = false;
        OnPlayerDeathChanged?.Invoke(_Dead);
    }

    private bool WallCheck()
    {
        return Physics.Raycast(new Ray(_CharCenterWorld, _Displacement.normalized), out RaycastHit hit, _CharMovementRadius, LayerMask.GetMask(Layers.MOVING_CUBE));
    }

    private void OnGUI()
    {
        return;
        Rect s = new Rect(0, 0, Screen.width, Screen.height);
        GUI.Label(s, _Direction.ToString());
    }
}
