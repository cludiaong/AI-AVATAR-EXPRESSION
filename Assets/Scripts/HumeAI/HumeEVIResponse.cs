using System;

[Serializable]
public class HumeEVIResponse
{
    public UserMessage user_message;
}

[Serializable]
public class UserMessage
{
    public Message message;
    public Models models;
}

[Serializable]
public class Message
{
    public string text;
}

[Serializable]
public class Models
{
    public Prosody prosody;
}

[Serializable]
public class Prosody
{
    public Prediction[] predictions;
}

[Serializable]
public class Prediction
{
    public string name;
    public float score;
}


