﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "event-config-single", menuName = "HopHop/Event Config Single", order = 0)]
public class EventConfigSingle : EventConfigOptional
{
    public List<EventConfigSingleData> Data = new List<EventConfigSingleData>();

    //

    public override OptionalType Type => OptionalType.Event;

#if UNITY_EDITOR

    public int EditorDataListCount
    {
        get => Data.Count;
        set
        {
            while (Data.Count > value)
                Data.RemoveAt(Data.Count - 1);
            while (Data.Count < value)
                Data.Add(new EventConfigSingleData());
        }
    }

    public bool EditorDataListCommand { get; set; } = false;

    public override string EditorName => $"{Type.ToString()} : {Option} : {this.name}";

#endif
}

[Serializable]
public class EventConfigSingleData
{
    public bool Wait;
    public DialogueConfigSingle Dialogue;
    public List<string> Command = new List<string>();
    public List<EventConfigSingleDataChoice> Choice = new List<EventConfigSingleDataChoice>();

    public bool WaitForce
    {
        get
        {
            if (Wait)
                return true;

            if (Dialogue != null)
                return true;

            if (Choice != null ? Choice.Count > 0 : false)
                return true;

            return false;
        }
    }

#if UNITY_EDITOR

    public int EditorCommandListCount
    {
        get => Command.Count;
        set
        {
            while (Command.Count > value)
                Command.RemoveAt(Command.Count - 1);
            while (Command.Count < value)
                Command.Add("");
        }
    }

    public int EditorChoiceListCount
    {
        get => Choice.Count;
        set
        {
            while (Choice.Count > value)
                Choice.RemoveAt(Choice.Count - 1);
            while (Choice.Count < value)
                Choice.Add(new EventConfigSingleDataChoice());
        }
    }

    //

    public bool EditorCommandListCommand { get; set; } = false;

    public bool EditorChoiceListCommand { get; set; } = false;

    //

    public bool EditorCommandListShow { get; set; } = false;

    public bool EditorChoiceListShow { get; set; } = false;

#endif
}

[Serializable]
public class EventConfigSingleDataChoice
{
    public EventConfigOptional Event;

#if UNITY_EDITOR

    public string EditorName => $"{(Event != null ? Event.EditorName : "...")}";

    public bool EditorFull { get; set; } = false;

#endif
}

#if UNITY_EDITOR

[CustomEditor(typeof(EventConfigSingle))]
public class EventConfigSingleEditor : Editor
{
    private const float POPUP_HEIGHT = 300f;
    private const float LABEL_WIDTH = 65f;

    private EventConfigSingle m_target;

    private EventConfig m_eventConfig;
    private string m_debugError = "";

    private Vector2 m_scrollData;

    private void OnEnable()
    {
        m_target = target as EventConfigSingle;

        SetConfigFind();
    }

    public override void OnInspectorGUI()
    {
        if (m_debugError != "")
        {
            QUnityEditor.SetLabel(m_debugError, QUnityEditor.GetGUIStyleLabel());
            return;
        }

        SetGUIGroupData();

        QUnityEditor.SetDirty(m_target);
    }

    //

    private void SetConfigFind()
    {
        if (m_eventConfig != null)
            return;

        var AuthorConfigFound = QUnityAssets.GetScriptableObject<EventConfig>("", false);

        if (AuthorConfigFound == null)
        {
            m_debugError = "Config not found, please create one";
            Debug.Log("[Event] " + m_debugError);
            return;
        }

        if (AuthorConfigFound.Count == 0)
        {
            m_debugError = "Config not found, please create one";
            Debug.Log("[Event] " + m_debugError);
            return;
        }

        if (AuthorConfigFound.Count > 1)
            Debug.Log("[Event] Config found more than one, get the first one found");

        m_eventConfig = AuthorConfigFound[0];

        //CONTINUE:

        m_debugError = "";
    }

    //

    private void SetGUIGroupData()
    {
        QUnityEditor.SetLabel("EVENT", QUnityEditor.GetGUIStyleLabel(FontStyle.Bold));

        //COUNT:
        m_target.EditorDataListCount = QUnityEditor.SetGroupNumberChangeLimitMin("Data", m_target.EditorDataListCount, 0);
        //LIST
        m_scrollData = QUnityEditor.SetScrollViewBegin(m_scrollData);
        for (int i = 0; i < m_target.Data.Count; i++)
        {
            #region ITEM
            QUnityEditor.SetHorizontalBegin();
            if (QUnityEditor.SetButton(i.ToString(), QUnityEditor.GetGUIStyleLabel(), QUnityEditor.GetGUILayoutWidth(25)))
                m_target.EditorDataListCommand = !m_target.EditorDataListCommand;

            #region ITEM - MAIN
            QUnityEditor.SetVerticalBegin();

            #region ITEM - MAIN - WAIT
            QUnityEditor.SetHorizontalBegin();
            QUnityEditor.SetLabel("Wait", null, QUnityEditor.GetGUILayoutWidth(LABEL_WIDTH));
            m_target.Data[i].Wait = QUnityEditor.SetField(m_target.Data[i].Wait, QUnityEditor.GetGUILayoutSizeToggle());
            if (m_target.Data[i].WaitForce)
                QUnityEditor.SetLabel("Wait force activated");
            QUnityEditor.SetHorizontalEnd();
            #endregion

            #region ITEM - MAIN - DIALOGUE
            QUnityEditor.SetHorizontalBegin();
            QUnityEditor.SetLabel("Dialogue", null, QUnityEditor.GetGUILayoutWidth(LABEL_WIDTH));
            m_target.Data[i].Dialogue = QUnityEditor.SetFieldScriptableObject(m_target.Data[i].Dialogue);
            QUnityEditor.SetHorizontalEnd();
            #endregion

            #region ITEM - MAIN - COMMAND
            //COUNT:
            var CommandEditorShow = QUnityEditor.SetGroupNumberChangeLimitMin("Command", m_target.Data[i].EditorCommandListShow, m_target.Data[i].EditorCommandListCount, 0);
            m_target.Data[i].EditorCommandListCount = CommandEditorShow.Value;
            m_target.Data[i].EditorCommandListShow = CommandEditorShow.Switch;
            //LIST:
            if (m_target.Data[i].EditorCommandListShow)
            {
                for (int j = 0; j < m_target.Data[i].Command.Count; j++)
                {
                    #region ITEM - MAIN - COMMAND - ITEM
                    QUnityEditor.SetHorizontalBegin();
                    if (QUnityEditor.SetButton(j.ToString(), QUnityEditor.GetGUIStyleLabel(), QUnityEditor.GetGUILayoutWidth(25)))
                        m_target.Data[i].EditorCommandListCommand = !m_target.Data[i].EditorCommandListCommand;
                    m_target.Data[i].Command[j] = QUnityEditor.SetField(m_target.Data[i].Command[j]);
                    QUnityEditor.SetHorizontalEnd();
                    #endregion

                    #region ITEM - MAIN - COMMAND - ARRAY
                    if (m_target.Data[i].EditorCommandListCommand)
                    {
                        QUnityEditor.SetHorizontalBegin();
                        QUnityEditor.SetLabel("", QUnityEditor.GetGUIStyleLabel(), QUnityEditor.GetGUILayoutWidth(25));
                        if (QUnityEditor.SetButton("↑", QUnityEditor.GetGUIStyleButton(), QUnityEditor.GetGUILayoutWidth(25)))
                            QList.SetSwap(m_target.Data[i].Command, j, j - 1);
                        if (QUnityEditor.SetButton("↓", QUnityEditor.GetGUIStyleButton(), QUnityEditor.GetGUILayoutWidth(25)))
                            QList.SetSwap(m_target.Data[i].Command, j, j + 1);
                        if (QUnityEditor.SetButton("X", QUnityEditor.GetGUIStyleButton(), QUnityEditor.GetGUILayoutWidth(25)))
                            m_target.Data[i].Command.RemoveAt(j);
                        QUnityEditor.SetHorizontalEnd();
                    }
                    #endregion
                }
            }
            #endregion

            #region ITEM - MAIN - CHOICE
            //COUNT:
            var SwitchEditorShow = QUnityEditor.SetGroupNumberChangeLimitMin("Choice", m_target.Data[i].EditorChoiceListShow, m_target.Data[i].EditorChoiceListCount, 0);
            m_target.Data[i].EditorChoiceListCount = SwitchEditorShow.Value;
            m_target.Data[i].EditorChoiceListShow = SwitchEditorShow.Switch;
            //LIST:
            if (m_target.Data[i].EditorChoiceListShow)
            {
                for (int j = 0; j < m_target.Data[i].Choice.Count; j++)
                {
                    #region ITEM - MAIN - CHOICE - ITEM
                    QUnityEditor.SetHorizontalBegin();
                    if (QUnityEditor.SetButton(j.ToString(), QUnityEditor.GetGUIStyleLabel(), QUnityEditor.GetGUILayoutWidth(25)))
                        m_target.Data[i].EditorChoiceListCommand = !m_target.Data[i].EditorChoiceListCommand;

                    #region ITEM - MAIN - CHOICE - ITEM - MAIN
                    QUnityEditor.SetVerticalBegin();

                    #region ITEM - MAIN - CHOICE - ITEM - MAIN - NAME (FULL)
                    QUnityEditor.SetHorizontalBegin();
                    if (QUnityEditor.SetButton(m_target.Data[i].Choice[j].EditorName, QUnityEditor.GetGUIStyleLabel(m_target.Data[i].Choice[j].EditorFull ? FontStyle.Bold : FontStyle.Normal, TextAnchor.MiddleLeft)))
                        m_target.Data[i].Choice[j].EditorFull = !m_target.Data[i].Choice[j].EditorFull;
                    QUnityEditor.SetHorizontalEnd();
                    #endregion

                    if (m_target.Data[i].Choice[j].EditorFull)
                    {
                        #region ITEM - MAIN - CHOICE - ITEM - MAIN - EVENT
                        QUnityEditor.SetHorizontalBegin();
                        QUnityEditor.SetLabel("Event", null, QUnityEditor.GetGUILayoutWidth(LABEL_WIDTH));
                        m_target.Data[i].Choice[j].Event = QUnityEditor.SetFieldScriptableObject(m_target.Data[i].Choice[j].Event);
                        QUnityEditor.SetHorizontalEnd();
                        #endregion
                    }

                    QUnityEditor.SetVerticalEnd();
                    #endregion

                    QUnityEditor.SetHorizontalEnd();
                    #endregion

                    #region ITEM - MAIN - CHOICE - ARRAY
                    if (m_target.Data[i].EditorChoiceListCommand)
                    {
                        QUnityEditor.SetHorizontalBegin();
                        QUnityEditor.SetLabel("", QUnityEditor.GetGUIStyleLabel(), QUnityEditor.GetGUILayoutWidth(25));
                        if (QUnityEditor.SetButton("↑", QUnityEditor.GetGUIStyleButton(), QUnityEditor.GetGUILayoutWidth(25)))
                            QList.SetSwap(m_target.Data[i].Choice, j, j - 1);
                        if (QUnityEditor.SetButton("↓", QUnityEditor.GetGUIStyleButton(), QUnityEditor.GetGUILayoutWidth(25)))
                            QList.SetSwap(m_target.Data[i].Choice, j, j + 1);
                        if (QUnityEditor.SetButton("X", QUnityEditor.GetGUIStyleButton(), QUnityEditor.GetGUILayoutWidth(25)))
                        {
                            m_target.Data[i].Choice.RemoveAt(j);
                            m_target.Data[i].EditorChoiceListCount--;
                        }
                        QUnityEditor.SetHorizontalEnd();
                    }
                    #endregion

                    if (m_target.Data[i].Choice[j].EditorFull)
                        QUnityEditor.SetSpace();
                }
            }
            #endregion

            QUnityEditor.SetVerticalEnd();
            #endregion

            QUnityEditor.SetHorizontalEnd();
            #endregion

            #region ARRAY
            if (m_target.EditorDataListCommand)
            {
                QUnityEditor.SetHorizontalBegin();
                QUnityEditor.SetLabel("", QUnityEditor.GetGUIStyleLabel(), QUnityEditor.GetGUILayoutWidth(25));
                if (QUnityEditor.SetButton("↑", QUnityEditor.GetGUIStyleButton(), QUnityEditor.GetGUILayoutWidth(25)))
                    QList.SetSwap(m_target.Data, i, i - 1);
                if (QUnityEditor.SetButton("↓", QUnityEditor.GetGUIStyleButton(), QUnityEditor.GetGUILayoutWidth(25)))
                    QList.SetSwap(m_target.Data, i, i + 1);
                if (QUnityEditor.SetButton("X", QUnityEditor.GetGUIStyleButton(), QUnityEditor.GetGUILayoutWidth(25)))
                    m_target.Data.RemoveAt(i);
                QUnityEditor.SetHorizontalEnd();
            }
            #endregion

            QUnityEditor.SetSpace();
        }
        QUnityEditor.SetScrollViewEnd();
    }
}

#endif