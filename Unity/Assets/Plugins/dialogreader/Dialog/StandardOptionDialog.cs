using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardOptionDialog : Dialog
{
    new void Start()
    {
        base.Start();
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
        if (CurrentNode.NextNodeAfter <= 0) return;
        if (CurrentNode.NextNodeAfter > Stopwatch.ElapsedMilliseconds/1000) return;
        if (CurrentNode.Options[CurrentNode.DefaultOption-1].NextNode >= 0)
        { 
            DialogController.GetSceneController().SetDialogNode(CurrentNode.Options[CurrentNode.DefaultOption-1].NextNode);
        }
    }

    public override void SelectAnswer(int answer)
    {
        DialogController.GetSceneController().SetDialogNode(CurrentNode.Options[answer].NextNode);
    }
}
