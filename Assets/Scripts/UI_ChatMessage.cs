using TMPro;
using UnityEngine;
public class UI_ChatMessage : MonoBehaviour
{
    public EChatType ChatType;

    public TextMeshProUGUI NicknameTextUI;
    public TextMeshProUGUI MessageTextUI;
    public TextMeshProUGUI DateTimeTextUI;

    public void Set(Chat chat)
    {
        if (NicknameTextUI != null)
        {
            NicknameTextUI.text = chat.Nickname;
        }
        MessageTextUI.text = chat.Message;
        if (DateTimeTextUI != null)
        {
            DateTimeTextUI.text = "미구현";
        }
    }
}
