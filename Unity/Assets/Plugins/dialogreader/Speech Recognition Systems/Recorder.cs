using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Recorder class, will get input and send it to the speech recognition system
/// </summary>
public class Recorder : MonoBehaviour {
    
    //Speech recognition system, the audio source (output must be Recorder!) and the recorded audioclip   
    private SpeechRecognition _speechRecognizer;
    private AudioSource _audioSource;
    private AudioClip _sendingClip;
    
    //Name of microphone
    private string _recordingDevice;
    
    //Container for spectrumdata, the average input volume, the current time (part of UpdateStep) and the amount of time the user has stopped speaking
    private float[] _clipSampleData;
    private float _inputVolumeAverage;
    private float _currentUpdateTime = 0f;
    private float _stoppedSpeakingTime;

    //Location of where the user started and stopped speaking.
    private int _startedSpeaking;
    private int _stoppedSpeaking;

    //Time to recheck audio input
    private const double UpdateStep = 0.05f;

    //Is audio being recognized, Is audio not being recorded and is the user speaking
    private bool _currentlySending;
    private bool _notRecording = true;
    private bool _speaking;

    //Possible states for the recorder
    public enum RecorderState { Recording, NotRecording, RecognizingAudio, Stopped };

    /// <summary>
    /// Update will constantly check if we can record, if the user is speaking and if audio is ready to be analyzed.
    /// </summary>
    void Update()
    {
        if (_notRecording) return;
        _currentUpdateTime += Time.deltaTime;
        if (_currentUpdateTime >= UpdateStep)
        {
            _currentUpdateTime = 0f;
            _audioSource.GetSpectrumData(_clipSampleData, 0, FFTWindow.Rectangular);
            float currentAverageVolume = _clipSampleData.Average();

            if (currentAverageVolume > _inputVolumeAverage + _inputVolumeAverage*0.7)
            {
                _speaking = true;
                if (_startedSpeaking == -1)
                {
                    _startedSpeaking = _audioSource.timeSamples;
                }
                _stoppedSpeaking = -1;
            }
            else if (_speaking)
            {
                if (_stoppedSpeaking == -1)
                {
                    _stoppedSpeaking = _audioSource.timeSamples;
                    _stoppedSpeakingTime = 0;
                }
                else
                {
                    _stoppedSpeakingTime += Time.deltaTime;
                    //After audio input is silent for 0.2 seconds, stop recording
                    if (_stoppedSpeakingTime >= 0.2f)
                    {
                        _notRecording = true;
                        _stoppedSpeaking = _audioSource.timeSamples;
                        EndRecording();
                    }

                }
            }
        }
    }
    
    /// <summary>
    /// Set the to be used Speech Recognition System
    /// </summary>
    /// <param name="speechRecognizer">SpeechRecognition Class, e.g. Wit(), GoogleSpeech() </param>
    public void SetSpeechRecognizer(SpeechRecognition speechRecognizer)
    {
        _speechRecognizer = speechRecognizer;
    }

    /// <summary>
    /// Get the current state of the recorder. Contains speaking, not speaking, sending and stopped
    /// </summary>
    /// <returns>Sate of the recorder</returns>
    public RecorderState GetRecorderState()
    {
        if (!_notRecording && !_speaking)
        {
            return RecorderState.NotRecording;
        }else if (!_notRecording && _speaking)
        {
            return RecorderState.Recording;
        }else if (_notRecording && !_speaking)
        {
            return RecorderState.RecognizingAudio;
        }
        return RecorderState.Stopped;
    }

    /// <summary>
    /// Start recording for recognition
    /// </summary>
    public void StartRecording()
    {
        if (_clipSampleData == null)
        {
            _recordingDevice = Microphone.devices[0];
            print(_recordingDevice);
            _audioSource = GetComponent<AudioSource>();
            _clipSampleData = new float[1024];
            _inputVolumeAverage = PlayerPrefs.GetFloat("mic", 0.000001586018f);

            _startedSpeaking = -1;
            _stoppedSpeaking = -1;
        }
        BeginRecording();
    }

    /// <summary>
    /// Stop recording
    /// </summary>
    public void StopRecording()
    {
        _notRecording = true;
        EndRecording(true);
    }

    /// <summary>
    /// Private call for the start of a recording
    /// </summary>
    private void BeginRecording() {
        if (_recordingDevice.Length == 0)
        {
            Debug.LogWarning("No Microphone Found");
            return;
        }
        print("Start Recording");
        _notRecording = false;
        _sendingClip = Microphone.Start(_recordingDevice, true, 60, 16000);
        GetComponent<AudioSource>().clip = _sendingClip;
        while (!(Microphone.GetPosition(null) > 0)) { }
        GetComponent<AudioSource>().Play();
    }

    /// <summary>
    /// Private call to stop recording
    /// </summary>
    /// <param name="end"></param>
    private void EndRecording(bool end = false)
    {
        Microphone.GetPosition(_recordingDevice);
        Microphone.End(_recordingDevice);
        try
        {
            if (!end)
            {
                if (_currentlySending) return;
                _currentlySending = true;
                print("End Recording");
                if (_sendingClip == null) return;
                float[] data = new float[_sendingClip.samples - (_sendingClip.samples - _stoppedSpeaking) + 3200];
                _sendingClip.GetData(data, 0);

                var tmpClip = AudioClip.Create("trimmed", data.Length, 1, 16000, false);
                tmpClip.SetData(data, 0);
                String fileName = "audio";
                SavWav.Save(Application.dataPath + "/" + fileName + ".wav", tmpClip);
                _speechRecognizer.Recognize(Application.dataPath + "/" + fileName + ".wav");
                _currentlySending = false;
            }
        }
        finally
        {
            _clipSampleData = new float[1024];
            _startedSpeaking = -1;
            _stoppedSpeaking = -1;
            _currentUpdateTime = 0;
            _speaking = false;
        }
    }

    public void SetScene(int currentNodeGrammar)
    {
        if (!_speechRecognizer) return;
        if (_speechRecognizer.GetType()==typeof(Sphinx))
        {
            ((Sphinx)_speechRecognizer).SetScene(currentNodeGrammar.ToString());
        }
    }
}
