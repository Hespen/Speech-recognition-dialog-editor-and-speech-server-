using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class Sphinx : SpeechRecognition {
    private Job job;
    private bool _sceneStarted;

    void Update()
    {
        if (job != null)
        {
            if (job.Update())
            {
                job = null;
            }
        }
    }

    public override void Recognize(string fileName)
    {
        byte[] audio = File.ReadAllBytes(fileName);
        job=new Job(SceneController,audio,null,SpeechRecognitionSystem.Sphinx);
        job.Start();
    }

    public void SetScene(String sceneIdentifier)
    {
        var job = new Job(SceneController,Encoding.UTF8.GetBytes(sceneIdentifier),null,SpeechRecognitionSystem.Sphinx);
        job.Start();
    }
}
