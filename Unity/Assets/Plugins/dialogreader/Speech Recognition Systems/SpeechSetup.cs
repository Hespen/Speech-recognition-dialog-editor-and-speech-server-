using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class SpeechSetup : MonoBehaviour {

    //Time to recheck audio input
    private AudioSource _audioSource;
    private float[] _clipSampleData = new float[1024];
    private bool _recording=false;
    private AudioClip _sendingClip;
    private float _previousAverage=0;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public float GetAverageInputVolume()
    {
        if (!_recording)
        {
            _recording = true;
            _sendingClip = Microphone.Start(Microphone.devices[0], true, 120, 16000);
            _audioSource.clip = _sendingClip;
            while (!(Microphone.GetPosition(null) > 0)) { }
            _audioSource.Play();
            return 0;
        }
        _audioSource.GetSpectrumData(_clipSampleData, 0, FFTWindow.Rectangular);
        _previousAverage = _previousAverage==0?_clipSampleData.Average():(_previousAverage+_clipSampleData.Average())/2;
        return _previousAverage;
    }
}
