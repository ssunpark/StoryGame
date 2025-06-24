using Newtonsoft.Json;

public class NpcResponse
{
    [JsonProperty("replyMessage")]
    public string replyMessage { get; set; }
    
    [JsonProperty("Appearance")]
    public string Appearance { get; set; }
    
    [JsonProperty("Emotion")]
    public string Emotion { get; set; }
    
    [JsonProperty("StoryImageDescription")]
    public string StoryImageDescription { get; set; }
    
    
}