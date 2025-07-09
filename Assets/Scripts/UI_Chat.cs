using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Chat : MonoBehaviour
{
    [Header("프리팹")]
    public UI_ChatMessage MinePrefab;
    public UI_ChatMessage OtherPrefab;
    public UI_ChatMessage SystemPrefab;
    
    public TMP_InputField InputField; // 입력 필드
    private List<UI_ChatMessage> _chatMessageUI = new();
    public Transform ContentTransfom;

    private void Start()
    {
        ChatManager.Instance.OnDataChanged += Refresh;
    }

    public void Refresh()
    {
        var chats = ChatManager.Instance.Chats;

        foreach (var ui in _chatMessageUI)
        {
            Destroy(ui.gameObject);
        }
        foreach (var chat in chats)
        {
            UI_ChatMessage chatMessage = null;
            switch (chat.Type)
            {
                case EChatType.Mine:
                    chatMessage = Instantiate(MinePrefab, ContentTransfom);
                    break;
                case EChatType.Other:
                    chatMessage = Instantiate(OtherPrefab, ContentTransfom);
                    break;
                case EChatType.System:
                    chatMessage = Instantiate(SystemPrefab, ContentTransfom);
                    break;
                default:
                    throw new ArgumentException();
            }
            chatMessage.Set(chat);
        }
    }

    public void OnClickSendButon()
    {
        string text =  InputField.text;
        if(string.IsNullOrEmpty(text)) return;
        ChatManager.Instance.SendPublicChatMessage(text);
        InputField.text =  string.Empty;
    }
}
