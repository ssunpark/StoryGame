using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WebText : MonoBehaviour
{
    public Text MyTextUI;
    
    void Start() {
        StartCoroutine(GetText());
    }
 
    IEnumerator GetText()
    {
        string url = "https://openapi.naver.com/v1/search/news.json?query=컴투스&display=30";
        
        UnityWebRequest www = UnityWebRequest.Get(url);
        //var www = UnityWebRequest.Get(url);
        www.SetRequestHeader("X-Naver-Client-Id", "mpQZ2RDNA4pYwYZaf4s1");
        www.SetRequestHeader("X-Naver-Client-Secret", "Fyw0RZF5IM");
        
        yield return www.SendWebRequest();
 
        if(www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        }
        else {
            // Show results as text
            MyTextUI.text = www.downloadHandler.text;
        }
    }
}
