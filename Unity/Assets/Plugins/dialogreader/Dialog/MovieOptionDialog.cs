using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovieOptionDialog : Dialog {
    private MovieTexture _movieTexture;

    new void Start() {

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
        DialogController.CharacterPortrait.sprite = Resources.Load<Sprite>(CurrentNode.Avatar);
    }

    public override void ShowOptions()
    {
        for (int i = 0; i < CurrentNode.Options.Count; i++)
        {
            var currentNodeOption = CurrentNode.Options[i];
            DialogController.TextAreaForDialogOptions[i].text = currentNodeOption.text;
            DialogController.TextAreaForDialogOptions[i].transform.parent.gameObject.SetActive(true);
        }
    }

    public override void CheckNextNode()
    {
        if (!_movieTexture.isPlaying)
        {
            if (CurrentNode.Options[CurrentNode.DefaultOption - 1].NextNode >= 0)
            {
                DialogController.GetSceneController().SetDialogNode(CurrentNode.Options[CurrentNode.DefaultOption - 1].NextNode);
            }
        }
        if (CurrentNode.NextNodeAfter <= 0) return;
        if (CurrentNode.NextNodeAfter > Stopwatch.ElapsedMilliseconds / 1000) return;
        if (CurrentNode.Options[CurrentNode.DefaultOption - 1].NextNode >= 0)
        {
            DialogController.GetSceneController().SetDialogNode(CurrentNode.Options[CurrentNode.DefaultOption - 1].NextNode);
        }
    }

    public override void SelectAnswer(int answer)
    {
        DialogController.GetSceneController().SetDialogNode(CurrentNode.Options[answer].NextNode);
    }
}
