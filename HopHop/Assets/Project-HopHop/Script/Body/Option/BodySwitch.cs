using System;
using UnityEditor;
using UnityEngine;

public class BodySwitch : MonoBehaviour, IBodySwitch
{
    [SerializeField] private bool m_state = true;
    //
    public Action<bool> onState;
    //
    private string m_switchIdentity;
    private string m_switchIdentityCheck;
    //
    private IsometricBlock m_block;
    //
    private void Awake()
    {
        m_block = GetComponent<IsometricBlock>();
    }

    private void Start()
    {
        m_switchIdentity = GameConfigInit.GetData(GetComponent<IsometricDataInit>(), GameConfigInit.Key.SwitchIdentity, false);
        m_switchIdentityCheck = GameConfigInit.GetData(GetComponent<IsometricDataInit>(), GameConfigInit.Key.SwitchIdentityCheck, false);
        //
        GameEvent.onSwitch += ISwitch;
    }

    private void OnDestroy()
    {
        GameEvent.onSwitch -= ISwitch;
    }

    //

    public void ISwitch(string Identity, bool State)
    {
        if (Identity != m_switchIdentityCheck)
            return;
        m_state = State;
        onState?.Invoke(State);
    }

    public void ISwitch(bool State)
    {
        m_state = State;
        onState?.Invoke(State);
        GameEvent.SetSwitch(m_switchIdentity, State);
    }

    //

    public bool State => m_state;

    public void SetSwitch(bool State)
    {
        ISwitch(State);
    }

    public void SetSwitch()
    {
        ISwitch(!m_state);
    }

#if UNITY_EDITOR

    public void SetEditorSwitchIdentity()
    {
        SetEditorMoveCheckAheadRemove();
        //
        m_block = QComponent.GetComponent<IsometricBlock>(this);
        IsometricDataInit BlockInit = QComponent.GetComponent<IsometricDataInit>(this);
        BlockInit.SetValue(GameConfigInit.GetKey(GameConfigInit.Key.SwitchIdentity) + "-" + m_block.Pos.ToString());
    }

    public void SetEditorSwitchIdentityCheck(IsometricBlock BlockFollow)
    {
        SetEditorMoveCheckAheadRemove();
        //
        IsometricDataInit BlockInit = QComponent.GetComponent<IsometricDataInit>(this);
        BlockInit.SetValue(GameConfigInit.GetKey(GameConfigInit.Key.SwitchIdentityCheck) + "-" + BlockFollow.Pos.ToString());
    }

    public void SetEditorMoveCheckAheadRemove()
    {
        IsometricDataInit BlockInit = QComponent.GetComponent<IsometricDataInit>(this);
        BlockInit.Data.RemoveAll(t => t.Contains(GameConfigInit.GetKey(GameConfigInit.Key.SwitchIdentity)));
    }

    public bool GetEditorMoveCheckAhead()
    {
        IsometricDataInit BlockInit = QComponent.GetComponent<IsometricDataInit>(this);
        return BlockInit.Data.Contains(GameConfigInit.GetKey(GameConfigInit.Key.SwitchIdentity));
    }

#endif
}

#if UNITY_EDITOR

[CustomEditor(typeof(BodySwitch))]
[CanEditMultipleObjects]
public class BodySwitchEditor : Editor
{
    private BodySwitch m_target;

    private void OnEnable()
    {
        m_target = target as BodySwitch;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //QUnityEditorCustom.SetUpdate(this);
        //
        //...
        //
        //QUnityEditorCustom.SetApply(this);
    }
}

#endif