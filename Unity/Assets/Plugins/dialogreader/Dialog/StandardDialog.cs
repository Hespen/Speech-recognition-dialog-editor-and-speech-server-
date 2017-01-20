using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardDialog : Dialog
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
      print(CurrentNode.Avatar.Replace("Assets/Resources/", ""));
        DialogController.CharacterPortrait.sprite = Resources.Load<Sprite>(CurrentNode.Avatar);
    }


    public override void CheckNextNode()
    {
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

    public override void ShowOptions()
    {
        //Not needed
    }
}
