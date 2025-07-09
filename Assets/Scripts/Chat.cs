using System;

public enum EChatType
{
    Mine,
    Other,
    System
}
public class Chat
{
    public readonly EChatType Type;
    public readonly string Nickname;
    public readonly string Message;

    public Chat(EChatType type, string nickname, string message)
    {
        if (string.IsNullOrEmpty(nickname)) throw new Exception("");
        if(string.IsNullOrEmpty(message)) throw new Exception("");
        
        Type = type;
        Nickname = nickname;
        Message = message;
    }
}
