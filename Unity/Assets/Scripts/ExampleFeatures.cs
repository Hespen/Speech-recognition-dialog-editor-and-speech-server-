using System.Collections;
using System.Collections.Generic;
using Assets.Dia;
using UnityEngine;
using UnityEngine.UI;

public class ExampleFeatures : MonoBehaviour
{

    public Text MicrophoneIndicator,Countdown;
    private DialogController _dialogController;
    private SpeechSetup _speechSetup;
    public Toggle AnalyzeEnvironment;
    public Slider InputSlider;
    public Image Indicator;
    private bool _isSliderUpdated=false;

    // Use this for initialization
    void Start()
    { 
        _dialogController = Camera.main.GetComponent<DialogController>();
        _speechSetup = GetComponent<SpeechSetup>();
        UpdateSlider();
        InputSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
    }
    
    // Update is called once per frame
    void Update()
    {
        UpdateMicrophone();
        HandleInput();
        UpdateCountdown();
    }

    private void UpdateCountdown()
    {
        Countdown.text="Time until next dialog: "+ _dialogController.GetSceneController().GetCountdownTimerInMilliseconds();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.touchCount>0)
        {
            _dialogController.StartDialog();
        }
        var averageVolumeInput = _speechSetup.GetAverageInputVolume();
        if (AnalyzeEnvironment.isOn)
        {
            PlayerPrefs.SetFloat("mic",averageVolumeInput);
            _isSliderUpdated = false;
        }else if (!_isSliderUpdated)
        {
            UpdateSlider();
            _isSliderUpdated = true;
        }
        else
        {
            CheckForAudioInput(averageVolumeInput);
        }
    }

    private void CheckForAudioInput(float averageVolumeInput)
    {
        Indicator.color = averageVolumeInput > InputSlider.value + InputSlider.value*0.7 ? Color.green : Color.red;
    }

    private void UpdateSlider()
    {
        InputSlider.minValue = 0;
        InputSlider.maxValue = PlayerPrefs.GetFloat("mic", 0) * 3f;
        InputSlider.value = PlayerPrefs.GetFloat("mic", 0);
    }

    public void ValueChangeCheck()
    {
        PlayerPrefs.SetFloat("mic", InputSlider.value);
    }

    private void UpdateMicrophone()
    {
        var recorder = _dialogController.GetSceneController().GetRecorder();
        if (recorder == null) { MicrophoneIndicator.text = ""; return;}
        switch (recorder.GetRecorderState())
        {
            case Recorder.RecorderState.NotRecording:
                MicrophoneIndicator.text = "Waiting for speech";
                break;
            case Recorder.RecorderState.Recording:
                MicrophoneIndicator.text = "Listening";
                break;
            case Recorder.RecorderState.RecognizingAudio:
                MicrophoneIndicator.text = "Analyzing";
                break;
            default:
                MicrophoneIndicator.text = "";
                break;
        }
    }
}
