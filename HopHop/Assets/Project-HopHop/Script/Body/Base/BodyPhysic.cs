using DG.Tweening;
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BodyPhysic : MonoBehaviour, ITurnManager
{
    //NOTE: Base Physic controller for all Character(s) and Object(s), included Gravity and Collide

    #region Action

    public Action<bool, IsometricVector> onMove;                    //State, Dir
    public Action<bool, IsometricVector> onMoveForce;               //State, Dir
    public Action<bool> onGravity;                                  //State, Block
    public Action<bool, IsometricVector, IsometricVector> onForce;  //State, Dir, From
    public Action<bool, IsometricVector, IsometricVector> onPush;   //State, Dir, From

    #endregion

    #region Turn & Move

    [SerializeField] private bool m_gravity = true;
    [SerializeField] private bool m_dynamic = true;
    [SerializeField] private bool m_static = false;

    private IsometricVector m_moveLastXY;
    private IsometricVector? m_moveForceXY;

    #endregion

    #region Get

    public bool Gravity => m_gravity;

    public bool Dynamic => m_dynamic;

    public IsometricVector MoveLastXY => m_moveLastXY;

    #endregion

    private IsometricBlock m_block;

    //

    private void Awake()
    {
        m_block = GetComponent<IsometricBlock>();
    }

    #region ITurnManager

    public void ISetTurn(int Turn)
    {
        if (!GetBodyStatic(IsometricVector.Bot))
            SetGravityControl();
    }

    public void ISetStepStart(string Step)
    {
        if (Step == StepType.Gravity.ToString())
            SetGravity();
    }

    public void ISetStepEnd(string Step) { }

    #endregion

    #region Move

    public void SetMoveControlReset()
    {
        m_moveLastXY = IsometricVector.None;
    }

    public bool SetMoveControl(IsometricVector Dir, bool Gravity = true)
    {
        if (Dir == IsometricVector.None)
            return true;

        if (GetBodyStatic(Dir))
            return false;

        if (Gravity)
            SetGravityControl(Dir);

        Vector3 MoveDir = IsometricVector.GetDirVector(Dir);
        Vector3 MoveStart = IsometricVector.GetDirVector(m_block.Pos);
        Vector3 MoveEnd = IsometricVector.GetDirVector(m_block.Pos) + MoveDir;
        DOTween.To(() => MoveStart, x => MoveEnd = x, MoveEnd, GameManager.Instance.TimeMove)
            .SetEase(Ease.Linear)
            .OnStart(() =>
            {
                onMove?.Invoke(true, Dir);
            })
            .OnUpdate(() =>
            {
                m_block.Pos = new IsometricVector(MoveEnd);
            })
            .OnComplete(() =>
            {
                onMove?.Invoke(false, Dir);
            });

        m_moveLastXY = Dir;

        return true;
    }

    public bool SetMoveControlForce(bool Gravity = true)
    {
        if (!m_moveForceXY.HasValue)
            //Fine to continue own control!!
            return false;

        if (GetBodyStatic(m_moveForceXY.Value))
            return false;

        if (Gravity)
            SetGravityControl(m_moveForceXY.Value);

        Vector3 MoveDir = IsometricVector.GetDirVector(m_moveForceXY.Value);
        Vector3 MoveStart = IsometricVector.GetDirVector(m_block.Pos);
        Vector3 MoveEnd = IsometricVector.GetDirVector(m_block.Pos) + MoveDir;
        DOTween.To(() => MoveStart, x => MoveEnd = x, MoveEnd, GameManager.Instance.TimeMove)
            .SetEase(Ease.Linear)
            .OnStart(() =>
            {
                onMoveForce?.Invoke(true, m_moveForceXY.Value);
            })
            .OnUpdate(() =>
            {
                m_block.Pos = new IsometricVector(MoveEnd);
            })
            .OnComplete(() =>
            {
                onMoveForce?.Invoke(false, m_moveForceXY.Value);
                m_moveForceXY = null;
            });

        return true;
    }

    #endregion

    #region Gravity

    public bool SetGravityControl()
    {
        if (!m_gravity)
            return false;

        if (GetBodyStatic(IsometricVector.Bot))
            return false;

        TurnManager.Instance.SetAdd(StepType.Gravity, this);
        TurnManager.Instance.onTurn += ISetTurn;
        TurnManager.Instance.onStepStart += ISetStepStart;
        TurnManager.Instance.onStepEnd += ISetStepEnd;

        return true;
    }

    private bool SetGravityControl(IsometricVector Dir)
    {
        if (!m_gravity)
            return false;

        if (GetBodyStatic(Dir + IsometricVector.Bot))
            return false;

        TurnManager.Instance.SetAdd(StepType.Gravity, this);
        TurnManager.Instance.onTurn += ISetTurn;
        TurnManager.Instance.onStepStart += ISetStepStart;
        TurnManager.Instance.onStepEnd += ISetStepEnd;

        return true;
    }

    private void SetGravity()
    {
        if (GetBodyStatic(IsometricVector.Bot))
        {
            onGravity?.Invoke(false); //NOTE: Check if this Body can fall thought another Body?

            TurnManager.Instance.SetEndStep(StepType.Gravity, this);
            TurnManager.Instance.onTurn -= ISetTurn;
            TurnManager.Instance.onStepStart -= ISetStepStart;
            TurnManager.Instance.onStepEnd -= ISetStepEnd;

            return;
        }

        Vector3 MoveDir = IsometricVector.GetDirVector(IsometricVector.Bot);
        Vector3 MoveStart = IsometricVector.GetDirVector(m_block.Pos.Fixed);
        Vector3 MoveEnd = IsometricVector.GetDirVector(m_block.Pos.Fixed) + MoveDir;
        DOTween.To(() => MoveStart, x => MoveEnd = x, MoveEnd, GameManager.Instance.TimeMove)
            .SetEase(Ease.Linear)
            .OnStart(() =>
            {
                onGravity?.Invoke(true);
            })
            .OnUpdate(() =>
            {
                m_block.Pos = new IsometricVector(MoveEnd);
            })
            .OnComplete(() =>
            {
                SetGravity();
            });
    }

    #endregion

    #region Push

    public bool SetPushControl(IsometricVector Dir, IsometricVector From)
    {
        if (!m_dynamic)
            return false;

        if (Dir == IsometricVector.None)
            return true;

        if (GetBodyStatic(Dir))
            return false;

        if (Dir != IsometricVector.Top && From != IsometricVector.Bot)
            m_moveLastXY = Dir;

        Vector3 MoveDir = IsometricVector.GetDirVector(Dir);
        Vector3 MoveStart = IsometricVector.GetDirVector(m_block.Pos.Fixed);
        Vector3 MoveEnd = IsometricVector.GetDirVector(m_block.Pos.Fixed) + MoveDir;
        DOTween.To(() => MoveStart, x => MoveEnd = x, MoveEnd, GameManager.Instance.TimeMove)
            .SetEase(Ease.Linear)
            .OnStart(() =>
            {
                onPush?.Invoke(true, Dir, From);
            })
            .OnUpdate(() =>
            {
                m_block.Pos = new IsometricVector(MoveEnd);
            })
            .OnComplete(() =>
            {
                onPush?.Invoke(false, Dir, From);
            });

        return true;
    }

    #endregion

    #region Push

    public bool SetForceControl(IsometricVector Dir, IsometricVector From, bool Check = true)
    {
        if (Dir == IsometricVector.None)
            return true;

        if (Check)
            return SetPushControl(Dir, From);

        Vector3 MoveDir = IsometricVector.GetDirVector(Dir);
        Vector3 MoveStart = IsometricVector.GetDirVector(m_block.Pos.Fixed);
        Vector3 MoveEnd = IsometricVector.GetDirVector(m_block.Pos.Fixed) + MoveDir;
        DOTween.To(() => MoveStart, x => MoveEnd = x, MoveEnd, GameManager.Instance.TimeMove)
            .SetEase(Ease.Linear)
            .OnStart(() =>
            {
                onForce?.Invoke(true, Dir, From);
            })
            .OnUpdate(() =>
            {
                m_block.Pos = new IsometricVector(MoveEnd);
            })
            .OnComplete(() =>
            {
                onForce?.Invoke(false, Dir, From);
            });

        return true;
    }

    #endregion

    #region Bottom

    public bool SetBottomControl()
    {
        if (!m_dynamic)
            return false;

        var Block = m_block.GetBlockAll(IsometricVector.Bot);
        foreach (var BlockCheck in Block)
        {
            if (BlockCheck == null)
                continue;

            if (BlockCheck.GetTag(KeyTag.Slow))
            {
                m_moveForceXY = IsometricVector.None;
                return true;
            }
            else
            if (BlockCheck.GetTag(KeyTag.Slip))
            {
                m_moveForceXY = m_moveLastXY;
                return true;
            }

            m_moveForceXY = null;

            return false;
        }

        return false;
    }

    #endregion

    #region Static

    private bool GetBodyStatic(IsometricVector Dir)
    {
        var Block = m_block.GetBlockAll(Dir);
        foreach (var BlockCheck in Block)
        {
            if (BlockCheck == null)
                continue;

            BodyStatic BodyStatic = BlockCheck.GetComponent<BodyStatic>();
            if (BodyStatic != null)
                return true;

            BodyPhysic BodyPhysic = BlockCheck.GetComponent<BodyPhysic>();
            if (BodyPhysic != null)
            {
                if (BodyPhysic.m_static)
                    return true;
            }
        }
        return false;
    }

    #endregion
}

#if UNITY_EDITOR

[CustomEditor(typeof(BodyPhysic))]
[CanEditMultipleObjects]
public class BaseBodyEditor : Editor
{
    private BodyPhysic m_target;

    private SerializedProperty m_gravity;
    private SerializedProperty m_dynamic;
    private SerializedProperty m_static;

    private void OnEnable()
    {
        m_target = target as BodyPhysic;

        m_gravity = QUnityEditorCustom.GetField(this, "m_gravity");
        m_dynamic = QUnityEditorCustom.GetField(this, "m_dynamic");
        m_static = QUnityEditorCustom.GetField(this, "m_static");
    }

    public override void OnInspectorGUI()
    {
        QUnityEditorCustom.SetUpdate(this);

        QUnityEditorCustom.SetField(m_gravity);
        QUnityEditorCustom.SetField(m_dynamic);
        QUnityEditorCustom.SetField(m_static);

        QUnityEditorCustom.SetApply(this);
    }
}

#endif