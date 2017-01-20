using System;
using System.Linq;
using Assets.Dia;
using Boo.Lang;
using UnityEditor;
using UnityEngine;

public class MovieOptionNode : BaseDialogNode
{
    
    public MovieOptionNode()
    {
        WindowTitle = GUIContentCreator.MovieOptionDialog;
        NodeType = GUIContentCreator.MovieOptionDialog;
    }

    public override void DrawWindow()
    {
        //Define background box
        GUIContentCreator.BoxField(new Rect(25, 0, GUIContentCreator.OptionDialogNodeSize.x - 50, 25), new Color(0.22f, 0.22f, 0.22f));
        GUIContentCreator.BoxField(new Rect(25, 25, GUIContentCreator.OptionDialogNodeSize.x - 50, GUIContentCreator.OptionDialogNodeSize.y - 50 + (extraItems * GUIContentCreator.ExtraBoxSize)), new Color(0.34f, 0.34f, 0.34f));
        //Create Main Group - All editable fields belong in this group
        GUI.backgroundColor = Color.white;
        GUI.BeginGroup(new Rect(25, 10, GUIContentCreator.OptionDialogNodeSize.x, GUIContentCreator.OptionDialogNodeSize.y - 50 + (extraItems * GUIContentCreator.ExtraBoxSize)));
        Event e = Event.current;
        MovieTexture = (MovieTexture)EditorGUILayout.ObjectField("Movie", MovieTexture, typeof(MovieTexture), true, GUILayout.Width(190), GUILayout.Height(35));
        if (MovieTexture != null)
        {
            Attachment = AssetDatabase.GetAssetPath(MovieTexture);
        }
        DrawMainDialogContent(true);


        AddOptionViews();
        if (GUILayout.Button("Add Option", GUILayout.Width(190)))
        {
            Options.Add(new List<string>() { "", "" });
            OutputNodes.Add(null);
            extraItems += 3;
        }
        GUI.EndGroup();

        //Add Connectors
        AddConnectorsMultiple();

        if (e.type == EventType.Repaint)
        {
            OutputNodesWindow = GUILayoutUtility.GetLastRect();
        }
        base.DrawWindow();
    }

    private void AddOptionViews()
    {
        for (int i = 0; i < Options.Count; i++)
        {
            EditorGUILayout.Separator();
            GUIContentCreator.LabelField("Option "+(i+1), Color.white);
            var option = Options[i];
            var newList = new List<string>() { "", "" };
            GUIContentCreator.LabelField("Answer",Color.white);
            newList[0] = EditorGUILayout.TextField(option[0], GUILayout.Width(190));
            GUIContentCreator.LabelField("Keywords", Color.white);
            newList[1] = EditorGUILayout.TextField(option[1], GUILayout.Width(190));
            Options[i] = newList;
        }
    }

    public override Rect GetWindowsRect()
    {
        Rect temp = WindowRect;
        temp.height = GUIContentCreator.MovieOptionNodeSize.y - 50 + (extraItems * GUIContentCreator.ExtraBoxSize);
        return temp;
    }
}
