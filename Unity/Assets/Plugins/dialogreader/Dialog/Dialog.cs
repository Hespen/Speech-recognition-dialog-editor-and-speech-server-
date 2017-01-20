using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public abstract class Dialog : MonoBehaviour
{
    //Current Node
    protected Node CurrentNode;

    //Stopwatch will keep track of passed time. Used for delay tracking
    protected Stopwatch Stopwatch;

    //Reference to Dialog Controller
    protected DialogController DialogController;

    //Value to check if contents has been shown
    private bool _contentShown;

    //Speech recognition system used, if available
    private Recorder _speechRecognizer;
    public bool Animating;


    protected void Start()
    {
        Stopwatch = new Stopwatch();
        Stopwatch.Start();
    }
    
    /// <summary>
    /// Update Value, Check content and nodes
    /// </summary>
    public void Update()
    {
        if (!_contentShown) { CheckDelay(); return;}
        CheckNextNode();
    }

    /// <summary>
    /// Show content of node
    /// </summary>
    protected abstract void ShowContent();

    /// <summary>
    /// Check the current time against the specified delay.
    /// Show content if delay time has passed
    /// </summary>
    private void CheckDelay()
    {
        if (Stopwatch == null) return;
        if (CurrentNode.Delay > Stopwatch.ElapsedMilliseconds/1000) return;
        ShowContent();
        if(DialogController.GetSceneController().StartRecognition()) _speechRecognizer.StartRecording();
        Stopwatch.Reset();
        Stopwatch.Start();
        _contentShown = true;
    }

    /// <summary>
    /// Get the remaining time until the next node is called in seconds
    /// </summary>
    /// <returns>Remaining time in seconds</returns>
    public int GetRemainingTimeUntilNextNodeInSeconds()
    {
        if (!_contentShown) return 0;
        var time = CurrentNode.NextNodeAfter- (int)Mathf.Round(Stopwatch.ElapsedMilliseconds/1000f);
        return time>=0?time:0;
    }

    /// <summary>
    /// Get the remaining time until the next node is called in milliseconds
    /// </summary>
    /// <returns>Remaining time in milliseconds</returns>
    public long GetRemainingTimeUntilNextNodeInMilliseconds()
    {
        if (!_contentShown) return 0;
        var time = (CurrentNode.NextNodeAfter*1000) - Stopwatch.ElapsedMilliseconds;
      return time >= 0 ? time : 0;
    }

    /// <summary>
    /// Setter for the to be used speech recognition system
    /// </summary>
    /// <param name="controller"></param>
    public void SetSpeechRecognizer(Recorder controller)
    {
        _speechRecognizer = controller;
    }

    public IEnumerator AnimateText()
    {
        Animating = true;
        var text = CurrentNode.Text;
        for (int i = 0; i < text.Length + 1; i++)
        {
            DialogController.TextAreaForDialog.text = text.Substring(0, i);
            yield return new WaitForSeconds(.03f);
        }
        if (DialogController.TextAreaForDialog.text.Length == text.Length)
        {
            ShowOptions();
            Animating = false;
        }
    }

    public abstract void ShowOptions();

    /// <summary>
    /// Set initial variables
    /// </summary>
    /// <param name="currentNode">The current node</param>
    /// <param name="dialogController">Current Dialogcontroller</param>
    public void SetVariables(Node currentNode, DialogController dialogController)
    {
        CurrentNode = currentNode;
        DialogController = dialogController;
    }
    /// <summary>
    /// If Next Node Delay has been passed, it will call the next node
    /// </summary>
    public abstract void CheckNextNode();

    /// <summary>
    /// Select the chosen answer
    /// </summary>
    /// <param name="answer">Array position of answer</param>
    public abstract void SelectAnswer(int answer);

   
}
