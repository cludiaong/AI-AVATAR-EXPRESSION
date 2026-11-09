using System;

[Serializable]
public class HumeTTSResponse
{
    public string request_id;
    public Generation[] generations;
}

[Serializable]
public class Generation
{
    public string generation_id;
    public float duration;
    public int file_size;
    public string encoding;
    public string format;
    public int sample_rate;
    public string audio;
    public Snippet[] snippets;
}

[Serializable]
public class Snippet
{
    public string snippet_id;
    public string text;
    public float start;
    public float end;
}