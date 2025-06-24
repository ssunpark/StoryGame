using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;



using OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;
using OpenAI.Images;
using OpenAI.Models;
using UnityEngine.Android;
using UnityEngine.Networking;

public class ChatGPTTest : MonoBehaviour
{
    public TextMeshProUGUI ResultTextUI; // 결과 텍스트
    public TMP_InputField PromptField; // 입력 필드
    public Button SendButton; // 보내기 버튼
    public AudioSource MyAudioSource; //TTS 재생할 오디오 소스
    public RawImage MyRawImage;
    private string _key;




    private List<Message> _messages = new List<Message>();
    private OpenAIClient _api;


    private void Start()
    {

        // 1. API 클라이언트 초기화 -> ChatGPT 접속
        _key = APIKeys.OPEANAI_API_KEY;
        _api = new OpenAIClient(_key);

        // CHAT-F
        // C: Context   : 문맥, 상황을 많이 알려줘라
        // H: Hint      : 예시 답변을 많이 줘라
        // A: As A role : 역할을 제공해라
        // T: Target    : 답변의 타겟을 알려줘라 
        // F: Format    : 답변 형태를 지정해라

        string systemMessage = "";
        systemMessage += "역할: 너는 로맨스 판타지 세계관의 서브 남자 주인공이다. 귀족 가문 출신으로, 겉보기엔 무심하고 차가워 보이지만 여주인공에게만은 다정하고 진심을 숨기지 않는다. 말투는 중세풍으로 부드럽고 절제되어 있으나, 때때로 감정이 드러난다.\n";
        systemMessage += "목적: 플레이어는 여주인공이며, 너는 그녀와 가벼운 대화를 하거나 감정을 드러내는 대사를 한다. 플레이어가 감정을 묻거나 질문을 던지면 그에 맞는 감정과 분위기의 답변을 한다.\n";
        systemMessage += "표현: 말투는 '~군요.', '~입니다만.', '~하시겠습니까?' 같은 격식을 유지하면서도 간혹 속마음을 드러내는 말(예: '당신만은 특별합니다.')을 사용한다. 답변은 반드시 100자 이내로 유지한다.\n";
        systemMessage += "[json 규칙] ";
        systemMessage += "답변은 'ReplyMessage', ";
        systemMessage += "남주의 표정/분위기 묘사는 'Emotion', ";
        systemMessage += "남주의 외모나 복장 상태 묘사는 'Appearance', ";
        systemMessage += "StoryImageDescription은 반드시 '남주의 외모와 현재 배경이 함께 어우러지는 하나의 묘사 문장'으로 출력한다. 예: '은색 머리칼의 청년이 붉은 장미가 가득한 정원 한가운데에 서 있다.'\n";

        _messages.Add(new Message(Role.System, systemMessage));
    }


    public async void Send()
    {
        // 0. 프롬프트(=AI에게 원하는 명령을 적은 텍스트)를 읽어온다.
        string prompt = PromptField.text;
        if (string.IsNullOrEmpty(prompt))
        {
            return;
        }

        PromptField.text = string.Empty;

        SendButton.interactable = false;

        // 2. 메시지 작성 후 메시지's 리스트에 추가
        Message promptMessage = new Message(Role.User, prompt);
        _messages.Add(promptMessage);

        // 3. 메시지 보내기
        //var chatRequest = new ChatRequest(_messages, Model.GPT4oAudioMini, audioConfig:Voice.Alloy);
        var chatRequest = new ChatRequest(_messages, Model.GPT4o);


        // 4. 답변 받기
        // var response = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
        var (npcResponse, response) = await _api.ChatEndpoint.GetCompletionAsync<NpcResponse>(chatRequest);

        Debug.Log(npcResponse.replyMessage);

        // 5. 답변 선택
        var choice = response.FirstChoice;

        // 6. 답변 출력
        Debug.Log($"[{choice.Index}] {choice.Message.Role}: {choice.Message} | Finish Reason: {choice.FinishReason}");
        ResultTextUI.text = npcResponse.replyMessage;

        // 7. 답변도 message's 추가
        Message resultMessage = new Message(Role.Assistant, prompt);
        _messages.Add(resultMessage);

        // 8. 답변 오디오 재생
        // MyAudioSource.PlayOneShot(response.FirstChoice.Message.AudioOutput.AudioClip);
        PlayTTS(npcResponse.replyMessage);
        GenerateImage(npcResponse.StoryImageDescription);
    }

    private async void PlayTTS(string text)
    {
        // Todo: 입력받은 text를 사운드로 재생...
        var request = new SpeechRequest(text);
        var speechClip = await _api.AudioEndpoint.GetSpeechAsync(request);
        MyAudioSource.PlayOneShot(speechClip);
        Debug.Log(speechClip);
    }

    private async void GenerateImage(string imagePrompt)
    {
        if (string.IsNullOrEmpty(imagePrompt) || MyRawImage == null)
        {
            Debug.LogWarning("이미지 프롬프트가 비어있거나 MyRawImage가 없습니다.");
            return;
        }
        
        var request = new ImageGenerationRequest("A house riding a velociraptor", Model.DallE_3);
        var imageResults = await _api.ImagesEndPoint.GenerateImageAsync(request);

        foreach (var result in imageResults)
        {
            Debug.Log(result.ToString());
            MyRawImage.texture = result.Texture;
        }
        
        SendButton.interactable = true;
    }
}