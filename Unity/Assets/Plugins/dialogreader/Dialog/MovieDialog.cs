using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovieDialog : Dialog {

    private MovieTexture _movieTexture;

    /// <summary>
    /// Movie Dialog Node, Start movie in constructor. Audio should be present in movie texture
    /// </summary>
    new void Start()
    {
        base.Start();
        var renderer = DialogController.MovieScreenForMovieDialog.GetComponent<Renderer>();
        var audioSource = DialogController.MovieScreenForMovieDialog.GetComponent<AudioSource>();
        _movieTexture = Resources.Load<MovieTexture>(CurrentNode.MovieTexture);
        renderer.material.mainTexture = _movieTexture;
        audioSource.clip = _movieTexture.audioClip;
        _movieTexture.Play();
        audioSource.Play();
    }


    protected override void ShowContent()
    {
        if (Animating) return;
        if (DialogController.AnimateText)
        {
            StartCoroutine(AnimateText());
        }
        else
        {
            DialogController.TextAreaForDialog.text = CurrentNode.Text;
        }
    }

    public override void ShowOptions()
    {
        //Not needed
    }

    public override void CheckNextNode()
    {
        if (!_movieTexture.isPlaying)
        {
            DialogController.GetSceneController().SetDialogNode(CurrentNode.NextNode);
        }
        if (CurrentNode.NextNodeAfter <= 0) return;
        if (CurrentNode.NextNodeAfter <= Stopwatch.ElapsedMilliseconds / 1000)
        {
            DialogController.GetSceneController().SetDialogNode(CurrentNode.NextNode);
        }
    }

    public override void SelectAnswer(int answer)
    {
        throw new System.NotImplementedException();
    }
}
