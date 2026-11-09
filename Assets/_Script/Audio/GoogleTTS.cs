using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoogleTTS : ElevenlabsAPI
{
    public override string ttsName => "google";
    public GoogleTTS(string apiKey, string voiceId) : base(apiKey, voiceId)
    {
    }
    public GCTextToSpeech _gcTextToSpeech;


    public VoiceConfig voiceConfig;
    public Enumerators.SsmlVoiceGender voiceGender = Enumerators.SsmlVoiceGender.FEMALE;
    public Enumerators.LanguageCode voiceLanguageCode = Enumerators.LanguageCode.en_US;
    public string voiceName = "en-US-Neural2-E";
    public bool ssml = false;
    public double pitch = 10.0;
    public double speakingRate = 1.1;
    public double sampleRateHertz = Constants.DEFAULT_SAMPLE_RATE;
    Enumerators.EffectsProfileId[] effectsProfileId = null;
    Enumerators.TimepointType[] timepoints = null;
    CustomVoiceParams customVoice = null;



    public override void Setup()
    {
        _gcTextToSpeech.SynthesizeSuccessEvent += onSynthesizeSuccess;

        _gcTextToSpeech.GetVoicesFailedEvent += _gcTextToSpeech_GetVoicesFailedEvent;
        _gcTextToSpeech.SynthesizeFailedEvent += _gcTextToSpeech_SynthesizeFailedEvent;
    }
    public override void GetAudio(string content)
    {
        if (string.IsNullOrEmpty(content))
            return;
        this.voiceConfig = voiceConfig == null ? new VoiceConfig()
        {
            gender = voiceGender,
            languageCode = _gcTextToSpeech.PrepareLanguage(voiceLanguageCode),
            name = voiceName
        } : voiceConfig;
        if (GeneralConfig.Config.betaAPI)
        {
            _gcTextToSpeech.Synthesize(content, voiceConfig,
            ssml,
            pitch,
            speakingRate,
            Constants.DEFAULT_SAMPLE_RATE, new Enumerators.EffectsProfileId[] { Enumerators.EffectsProfileId.SMALL_HOME_SPEAKER },
            new Enumerators.TimepointType[] { Enumerators.TimepointType.TIMEPOINT_TYPE_UNSPECIFIED }
            //,
            //new CustomVoiceParams()
            //{
            //    reportedUsage = ReportedUsage.REALTIME,
            //    model = "projects/{project_id}/locations/us-central1/models/{model_id}"
            //}
            );
        }
        else
        {
            _gcTextToSpeech.Synthesize(content, voiceConfig,
            ssml,
            pitch,
            speakingRate,
            Constants.DEFAULT_SAMPLE_RATE, new Enumerators.EffectsProfileId[] { Enumerators.EffectsProfileId.SMALL_HOME_SPEAKER });
        }
    }

    private void onSynthesizeSuccess(PostSynthesizeResponse response, long requestId)
    {
        var audioClip = _gcTextToSpeech.GetAudioClipFromBase64(response.audioContent, Constants.DEFAULT_AUDIO_ENCODING);
        _audioClip = audioClip;
        AudioReceived.Invoke(audioClip);
        Debug.Log("Synthesize Success");
    }
    private void _gcTextToSpeech_SynthesizeFailedEvent(string error, long requestId)
    {
        Debug.LogError(error);
    }

    private void _gcTextToSpeech_GetVoicesFailedEvent(string error, long requestId)
    {
        Debug.LogError(error);
    }
}
