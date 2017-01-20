using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using CUETools.Codecs;
using CUETools.Codecs.FLAKE;
using UnityEngine;

public class GoogleSpeech : SpeechRecognition
{
    private Job job;
    private string _fileName;

   void Update()
    {
        if (job != null)
        {
            if (job.Update())
            {
                File.Delete(_fileName + ".flac");
                job = null;
            }
        }
    }

    public override void Recognize(String fileName)
    {
        var targetpath = fileName + ".flac";
        try
        {
            _fileName = fileName;
            FileStream fileStream = File.OpenRead(Wav2Flac(fileName, targetpath));
            BinaryReader filereader = new BinaryReader(fileStream);
            byte[] bytes = filereader.ReadBytes((Int32) fileStream.Length);
            fileStream.Close();
            filereader.Close();
            job = new Job(SceneController, bytes, ApiKey, SpeechRecognitionSystem.Google);
            job.Start();
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }
    
    public static String Wav2Flac(String wavName, string flacName)
    {
        int sampleRate = 0;

        IAudioSource audioSource = new WAVReader(wavName, null);
        AudioBuffer buff = new AudioBuffer(audioSource, 0x10000);

        FlakeWriter flakewriter = new FlakeWriter(flacName, audioSource.PCM);

        sampleRate = audioSource.PCM.SampleRate;
        FlakeWriter audioDest = flakewriter;
        while (audioSource.Read(buff, -1) != 0)
        {
            audioDest.Write(buff);
        }
        audioDest.Close();
        audioDest.Close();
        audioSource.Close();
        return flacName;
    }
}

[Serializable]
public class Alternative
{
    public string transcript;
    public double confidence;
}

[Serializable]
public class Result
{
    public Alternative[] alternatives;
}

[Serializable]
public class GoogleResponse
{
    public Result[] results;
}
