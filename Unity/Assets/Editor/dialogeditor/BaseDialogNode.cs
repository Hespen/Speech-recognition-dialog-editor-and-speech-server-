using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Dia;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Parent class for all node types
/// This class contains all variables which are editable in his child nodes.
/// Exporting and saving nodes will be done by using these variables
/// </summary>
public abstract class BaseDialogNode : ScriptableObject
{
    
    public List<BaseDialogNode> OutputNodes = new List<BaseDialogNode>();
    public List<Boo.Lang.List<string>> Options = new List<Boo.Lang.List<string>>();
    public Rect OutputNodesWindow;
    public int extraItems = 0;
    public string DialogText = "";
    public int DelayText;
    public int NextNodeDelay;
    public bool StartingNode;
    public int DefaultOption;
    public int Grammar;
    public String Attachment;
    public int DefaultAfter;
    public Node Node;
    public string NodeType;
    public Sprite Avatar;
    public MovieTexture MovieTexture;
    public Rect WindowRect;
    public string WindowTitle = "";
    public Rect BaseRect;
    public int Id;

    /// <summary>
    /// Draw a curve between this node and it's outputnodes
    /// </summary>
    public virtual void DrawCurves()
    {
        for (int index = 0; index < OutputNodes.Count; index++)
        {
            var _input = OutputNodes[index];
            if (_input)
            {
                NodeEditor.DrawNodeCurve(new Rect(WindowRect.x, WindowRect.y + 25 + (index * 30), WindowRect.width, 25), RecalcRect(_input.WindowRect));
            }
        }
    }

    /// <summary>
    /// Calculation for determing the input location for the outputnode
    /// </summary>
    /// <param name="rec">Output Node Rect</param>
    /// <returns>Connector location</returns>
    private Rect RecalcRect(Rect rec)
    {
        var rect = rec;
        rect.y += 37.5f;
        rect.width = 1;
        rect.height = 1;
        return rect;
    }

    /// <summary>
    /// Export function
    /// Creation of new Node which will be placed in a NodeList
    /// All variables in the Node class are serializable.
    /// 
    /// Saving the state of the screen or exporting will call this function.
    /// Make sure every important variable is set in the Node class! 
    /// </summary>
    /// <returns>Node</returns>
    public virtual Node GetNode()
    {
        Node = new Node();
        Node.NodeId = Id;
        Node.Delay = DelayText;
        Node.NodeType = NodeType;
        Node.Text = DialogText;
        Node.StartingNode = StartingNode;
        Node.Grammar = Grammar;
        if (Avatar!=null)
        {
            Node.Avatar = Attachment;
        }
        if (MovieTexture != null)
        {
            Node.MovieTexture = Attachment;
        }
        Node.position = new[] { WindowRect.position.x, WindowRect.position.y };
        Node.NextNodeAfter = NextNodeDelay;
        Node.DefaultOption = DefaultOption;  
        Node.NextNode = Options.Count > 1 ? -1 : OutputNodes.Count > 0 ? OutputNodes[0] != null ? OutputNodes[0].Id : -1 : -1;
        if (Options.Count > 0)
        {
            for (int index = 0; index < Options.Count; index++)
            {
                var opt = Options[index];
                var option = new Option();
                option.text = opt[0];
                option.Keywords = opt[1].Split(null);
                if (OutputNodes[index] != null) option.NextNode = OutputNodes[index].Id;
                Node.Options.Add(option);
            }
        }
        return Node;
    }

    /// <summary>
    /// Base DrawWindow function
    /// Place all generic gui elements here
    /// </summary>
    public virtual void DrawWindow()
    {
        if (GUI.Button(new Rect(WindowRect.width - 50, 0, 25f, 25f),GUIContentCreator.GetTextureFromFile("trash.png",new Vector2(25,25)),GUIStyle.none))
        {
            ActionStorage.Windows.Remove((BaseDialogNode)this);

            //Prefs will not be updated before the next OnGUI(). Therefor manually clear the prefs, if all nodes are manually deleted
            if(ActionStorage.Windows.Count==0)EditorPrefs.SetString("json","");
            foreach (var baseInputNode in ActionStorage.Windows)
            {
                baseInputNode.DeleteNode(this);
            }
        }

        EditorStyles.toggle.normal.textColor = Color.white;
        StartingNode = GUI.Toggle(new Rect(25, 3.5f, 100, 25), StartingNode, "Starting point");
        EditorStyles.toggle.normal.textColor = Color.black;
        if (!StartingNode) return;
        foreach (var baseDialogNode in ActionStorage.Windows)
        {
            if (baseDialogNode != this)
            {
                baseDialogNode.StartingNode = false;
            }
        }
    }

    /// <summary>
    /// Draw Dialog Text Area, Keywords Area, Delay Area.
    /// </summary>
    /// <param name="options">Can node take options, default = false.True gives extra fields for selecting options after x seconds</param>
    protected void DrawMainDialogContent(bool options=false)
    {
        GUIContentCreator.LabelField("Dialog", Color.white);
        DialogText = EditorGUILayout.TextArea(DialogText, GUILayout.Height(50), GUILayout.Width(190));


        EditorGUILayout.Separator();

        DelayText = GUIContentCreator.IntField(DelayText, "Delay (in seconds)", Color.white);

        EditorGUILayout.Separator();
        if (options)
        {
            Grammar = GUIContentCreator.IntField(Grammar, "Grammar id", Color.white);
            DefaultOption = GUIContentCreator.IntField(DefaultOption, "Select answer:", Color.white);
            NextNodeDelay = GUIContentCreator.IntField(NextNodeDelay, "After seconds:", Color.white);
        }
        else
        {
            NextNodeDelay = GUIContentCreator.IntField(NextNodeDelay, "Go to next node after sec", Color.white);
        }
    }

    /// <summary>
    /// Delete node and connections
    /// </summary>
    /// <param name="node"></param>
  public virtual void DeleteNode(BaseDialogNode node)
    {
        for (int index = 0; index < OutputNodes.Count; index++)
        {
            var _input = OutputNodes[index];
            if (node.Equals(_input))
            {
                OutputNodes[index] = null;
            }
        }
    }

    /// <summary>
    /// Set Output Node.
    /// ActionStorage.ClickedIndex should be set!
    /// </summary>
    /// <param name="dialog">Output node</param>
    public void SetOutput(BaseDialogNode dialog)
    { 
        SetOutput(dialog, 0);
    }

    /// <summary>
    /// Set Output Node.
    /// ActionStorage.ClickedIndex should be set!
    /// </summary>
    /// <param name="dialog">Output node</param>
    /// <param name="index">Index to add node</param>
    public void SetOutput(BaseDialogNode dialog, int index)
    { 
        if (ActionStorage.ClickedIndex < OutputNodes.Count)
        {
            OutputNodes[ActionStorage.ClickedIndex] = dialog;
            return;
        } 
        OutputNodes.Insert(index, dialog);
    }

    /// <summary>
    /// Add Connectors.
    /// Use this function when options are available
    /// </summary>
    public void AddConnectorsMultiple()
    {
        for (int i = 0; i < Options.Count; i++)
        {
            if (GUI.Button(new Rect(WindowRect.width - 25, 25 + (i * 30), 25, 25), (i + 1).ToString(), GUIContentCreator.MakeStyle(GUIContentCreator.OutputColor)))
            {
                if (ActionStorage.ClickedIndex==i && ActionStorage.ClickedNode== ActionStorage.Windows.Find(x => x.WindowRect == WindowRect))
                {
                    Options.RemoveAt(i);
                    ActionStorage.ClickedNode = null;
                    ActionStorage.ClickedIndex = -1;
                    if (OutputNodes.Count>=i)
                    {
                        OutputNodes.RemoveAt(i);
                    }
                    extraItems -= 3;
                    return;
                }
                ActionStorage.ClickedNode = ActionStorage.Windows.Find(x => x.WindowRect == WindowRect);
                ActionStorage.ClickLocation = new Rect(WindowRect.x, WindowRect.y + 25 + (i * 30), WindowRect.width, 25);
                ActionStorage.ClickedIndex = i;
            }
        }
        if (GUI.Button(new Rect(0, 25, 25, 25), ">", GUIContentCreator.MakeStyle(GUIContentCreator.InputColor)))
        {
            if (ActionStorage.ClickedNode)
            {
                if (ActionStorage.ClickedNode != ActionStorage.Windows.Find(x => x.WindowRect == WindowRect))
                {
                    ActionStorage.ClickedNode.SetOutput(ActionStorage.Windows.Find(x => x.WindowRect == WindowRect));
                    ActionStorage.ClickedNode = null;
                    ActionStorage.ClickedIndex = -1;
                }

            }
        }
    }

    /// <summary>
    /// Draw connectors for nodes without options
    /// </summary>
    public void AddConnectorsSingle()
    {
        if (GUI.Button(new Rect(WindowRect.width - 25, 25, 25, 25), ">", GUIContentCreator.MakeStyle(GUIContentCreator.OutputColor)))
        {
            ActionStorage.ClickedNode = ActionStorage.Windows.Find(x => x.WindowRect == WindowRect);
            ActionStorage.ClickLocation = new Rect(WindowRect.x, WindowRect.y + 25, WindowRect.width, 25);
            ActionStorage.ClickedIndex = 0;
        }

        if (GUI.Button(new Rect(0, 25, 25, 25), ">", GUIContentCreator.MakeStyle(GUIContentCreator.InputColor)))
        {
            if (ActionStorage.ClickedNode)
            {
                if (ActionStorage.ClickedNode != ActionStorage.Windows.Find(x => x.WindowRect == WindowRect))
                {
                    ActionStorage.ClickedNode.SetOutput(ActionStorage.Windows.Find(x => x.WindowRect == WindowRect));
                    ActionStorage.ClickedNode = null;
                    ActionStorage.ClickedIndex = -1;
                }

            }
        }
    }

    public abstract Rect GetWindowsRect();

}
