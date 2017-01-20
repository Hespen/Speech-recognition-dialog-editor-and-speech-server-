using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Policy;
using UnityEngine;

public class Wit : SpeechRecognition {
    private Job job;


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

    public override void Recognize(String path)
    {
        FileStream filestream = new FileStream(path, FileMode.Open, FileAccess.Read);
        BinaryReader filereader = new BinaryReader(filestream);
        byte[] bytes = filereader.ReadBytes((Int32)filestream.Length);

        filestream.Close();
        filereader.Close();
        job = new Job(SceneController, bytes,ApiKey,SpeechRecognitionSystem.Wit);
        job.Start();
    }
}

[System.Serializable]
public class Entities
{
}

[System.Serializable]
public class WitValues
{
    public string msg_id;
    public string _text;
    public Entities entities;
}
