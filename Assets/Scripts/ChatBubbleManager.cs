using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatBubbleManager : MonoBehaviour
{
    [Header("말풍선 프리팹")]
    public GameObject MessageRightPrefab;
    public GameObject MessageLeftPrefab;

    [Header("부모 오브젝트")]
    public Transform ContentParent; // Scroll View > Viewport > Content
    
    [Header("스크롤뷰")]
    public RectTransform ContentRect;
    public ScrollRect _scrollRect;

    public void CreatePlayerBubble(string message)
    {
        CreateBubble(message, MessageRightPrefab);
    }

    public void CreateNpcBUbble(string message)
    {
        CreateBubble(message, MessageLeftPrefab);
    }

    private void CreateBubble(string message, GameObject prefab)
    {
        GameObject bubble = Instantiate(prefab, ContentParent);
        TMP_Text textComponent = bubble.GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = message;
        }

        StartCoroutine(ScrollToBottomNextFrame());
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(ContentRect);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        _scrollRect.verticalNormalizedPosition = 0;
    }
}