using System;
using System.Linq;
using Assets.Dia;
using Boo.Lang;
using UnityEditor;
using UnityEngine;

public class TemplateDialogNode : BaseDialogNode
{

    //Todo: Add node to NodeEditor Menu and ContextCallback;
    public TemplateDialogNode()
    {
        WindowTitle = GUIContentCreator.TemplateDialog;
        Node = new Node();
        NodeType = GUIContentCreator.TemplateDialog;
    }

    public override void DrawWindow()
    { 
        //Define background box, you can change its color here
        //Refer Rect values to corresponding NodeSize variable in GUIContentCreator
        GUIContentCreator.BoxField(new Rect(25, 0, GUIContentCreator.TemplateDialogNodeSize.x - 50, 25), new Color(0.22f, 0.22f, 0.22f));
        GUIContentCreator.BoxField(new Rect(25, 25, GUIContentCreator.TemplateDialogNodeSize.x-50, GUIContentCreator.TemplateDialogNodeSize.y - 50 + (extraItems * GUIContentCreator.ExtraBoxSize)),new Color(0.34f,0.34f,0.34f));

        //Create Main Group - All editable fields belong in this group
        //Group is centered in window
        GUI.backgroundColor = Color.white;
        GUI.BeginGroup(new Rect(25, 10, GUIContentCreator.TemplateDialogNodeSize.x, GUIContentCreator.TemplateDialogNodeSize.y - 50 + (extraItems* GUIContentCreator.ExtraBoxSize)));
        Event e = Event.current;
        
        //Optional: Draw Main Dialog Content
        DrawMainDialogContent();

        //Todo: Place all custom GUI Elements here
        //Todo: Add custom GUI WitValues to BaseDialogNode() and Node()
        //Todo: Add all custom GUI WitValues to BaseDialogNode.GetNode() and NodeEditor.InitializeSavedData();

        //End centered group
        GUI.EndGroup();

        //Todo: choose one of the following
        //Add Connectors for no options available:
        AddConnectorsSingle();
        //Add Connectors for options available:
        AddConnectorsMultiple();
          
        if (e.type == EventType.Repaint)
        {
            OutputNodesWindow = GUILayoutUtility.GetLastRect();
        }
        base.DrawWindow();
    }

    public override Rect GetWindowsRect()
    {
        Rect temp = WindowRect;
        temp.height = GUIContentCreator.TemplateDialogNodeSize.y - 50 + (extraItems * GUIContentCreator.ExtraBoxSize);
        return temp;
    }
}
