using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class TypecastTTS : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    private string _apiKey;
    private const string _actorId = "63aaec04428dd87af3757d72";
    private const string _ttsUrl = "https://typecast.ai/api/speak";

    private void Start()
    {
        _apiKey = APIKeys.TYPECAST_API_KEY;
        _audioSource = GetComponent<AudioSource>();
    }
    
    public async void PlayTypecastTTS(string text)
    {
        await RequestAndPlayAudio(text);
    }

    private async Task<string> SendURLRequest(string text)
    {
        JObject payload = new JObject
        {
            { "text", text },
            { "lang", "auto" },
            { "actor_id", _actorId },
            { "xapi_hd", true },
            { "model_version", "latest" }
        };

        UnityWebRequest request = new UnityWebRequest(_ttsUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(payload.ToString());
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {_apiKey}");

        await request.SendWebRequest();
        
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("TTS 요청 실패: " + request.error);
            return null;
        } 
        
        string jsonText = request.downloadHandler.text;
        JObject responseJson = JObject.Parse(jsonText);
        
        string speakUrl = responseJson["result"]?["speak_v2_url"]?.ToString();
        
        return speakUrl;
    }

    private async Task<string> SendPollingRequest(string speakUrl)
    {
        JObject responseAudioJson = null;
        const int maxRetries = 20;
        const int delayMs = 500;
        for (int i = 0; i < maxRetries; i++)
        {
            UnityWebRequest audioRequest = UnityWebRequest.Get(speakUrl);
            audioRequest.downloadHandler = new DownloadHandlerBuffer();
            audioRequest.SetRequestHeader("Authorization", $"Bearer {_apiKey}");

            await audioRequest.SendWebRequest();

            if (audioRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("오디오 상태 확인 실패: " + audioRequest.error);
                return null;
            }
            
            string jsonAudioText = audioRequest.downloadHandler.text;
            
            Debug.Log(jsonAudioText);
            responseAudioJson = JObject.Parse(jsonAudioText);
            string status = responseAudioJson["result"]?["status"]?.ToString();

            Debug.Log($"TTS 상태: {status}");

            if (status == "done")
            {
                break;
            }

            await Task.Delay(delayMs);
        }

        string audioUrl = responseAudioJson?["result"]?["audio_download_url"]?.ToString();

        
        return audioUrl;
    }

    private async Task<AudioClip> DownloadAudioClip(string audioUrl)
    {
        UnityWebRequest audioDownloadRequest = new UnityWebRequest(audioUrl, UnityWebRequest.kHttpVerbGET);
        audioDownloadRequest.downloadHandler = new DownloadHandlerAudioClip(audioUrl, AudioType.WAV);
        
        await audioDownloadRequest.SendWebRequest();

        if (audioDownloadRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("오디오 다운로드 실패: " + audioDownloadRequest.error);
            return null;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(audioDownloadRequest);

        return clip;
    }

    private async Task RequestAndPlayAudio(string text)
    {
        string speakUrl = await SendURLRequest(text);
        if (string.IsNullOrEmpty(speakUrl))
        {
            Debug.LogError("speak_url 없음");
            return;
        }

        string audioUrl = await SendPollingRequest(speakUrl);
        if (string.IsNullOrEmpty(audioUrl))
        {
            Debug.LogError("오디오 다운로드 URL 없음");
            return;
        }
        
        _audioSource.clip = await DownloadAudioClip(audioUrl);
        if (_audioSource.clip == null)
        {
            Debug.LogError("오디오 클립 다운로드 실패");
            return;
        }
        
        _audioSource.Play();
    }
}