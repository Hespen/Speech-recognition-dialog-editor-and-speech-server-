using System;
using System.Linq;
using Assets.Dia;
using Boo.Lang;
using UnityEditor;
using UnityEngine;

public class StandardDialogNode : BaseDialogNode
{

    public StandardDialogNode()
    {
        WindowTitle = GUIContentCreator.StandardDialog;
        Node = new Node();
        NodeType = GUIContentCreator.StandardDialog;
    }

    public override void DrawWindow()
    { 
        //Define background box
        GUIContentCreator.BoxField(new Rect(25, 0, GUIContentCreator.StandardDialogNodeSize.x - 50, 25), new Color(0.22f, 0.22f, 0.22f));
        GUIContentCreator.BoxField(new Rect(25, 25, GUIContentCreator.StandardDialogNodeSize.x-50, GUIContentCreator.StandardDialogNodeSize.y - 50 + (extraItems * GUIContentCreator.ExtraBoxSize)),new Color(0.34f,0.34f,0.34f));

        //Create Main Group - All editable fields belong in this group
        GUI.backgroundColor = Color.white;
        GUI.BeginGroup(new Rect(25, 10, GUIContentCreator.StandardDialogNodeSize.x, GUIContentCreator.StandardDialogNodeSize.y - 50 + (extraItems* GUIContentCreator.ExtraBoxSize)));
        Event e = Event.current;
        Avatar = (Sprite)EditorGUILayout.ObjectField("Avatar", Avatar, typeof(Sprite), true, GUILayout.Width(190),GUILayout.Height(35));
        if (Avatar != null)
        {
             Attachment = AssetDatabase.GetAssetPath(Avatar);
        }

        DrawMainDialogContent();

        GUI.EndGroup();

       AddConnectorsSingle();
          
        if (e.type == EventType.Repaint)
        {
            OutputNodesWindow = GUILayoutUtility.GetLastRect();
        }
        base.DrawWindow();
    }

    public override Rect GetWindowsRect()
    {
        Rect temp = WindowRect;
        temp.height = GUIContentCreator.StandardDialogNodeSize.y - 50 + (extraItems * GUIContentCreator.ExtraBoxSize);
        return temp;
    }
}
