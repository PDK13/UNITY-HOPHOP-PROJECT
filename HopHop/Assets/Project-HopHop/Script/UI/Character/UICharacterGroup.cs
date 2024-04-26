using System.Collections.Generic;
using UnityEngine;

public class UICharacterGroup : MonoBehaviour
{
    [SerializeField] private Transform m_content;

    private List<UICharacterItem> m_list = new List<UICharacterItem>();

    //

    private void Start()
    {
        CharacterManager.Instance.onCharacter += SetCharacter;
        //
        SetInit();
    }

    private void OnDestroy()
    {
        CharacterManager.Instance.onCharacter -= SetCharacter;
    }

    //

    private void SetInit()
    {
        for (int i = 0; i < m_content.childCount; i++)
        {
            UICharacterItem Item = m_content.GetChild(i).GetComponent<UICharacterItem>();
            Item.SetCharacter(CharacterManager.Instance.CharacterParty[i]);
            Item.SetMana(3);
            Item.SetChoice(CharacterManager.Instance.CharacterCurrent);
            //
            m_list.Add(Item);
        }
    }

    private void SetCharacter()
    {
        for (int i = 0; i < m_list.Count; i++)
            m_list[i].SetChoice(CharacterManager.Instance.CharacterCurrent);
    }
}