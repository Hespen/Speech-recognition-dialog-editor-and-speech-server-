using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Dia;
using UnityEditor;
using UnityEngine;

public class NodeEditor : EditorWindow
{

    private Vector2 _mousePos;
    private Vector2 _previousPosition;
    private Vector2 _offset;
    public Rect ZoomArea;
    private float _zoom = 1.0f;
    private int _nodeCounter;


    [MenuItem("Window/Nodes Editor")]
    static void ShowEditor()
    {
        GetWindow<NodeEditor>();
    }

    void Update()
    {
        if (EditorApplication.isPlaying)
        {
            if (SceneView.sceneViews.Count > 0)
            {
                SceneView sceneView = (SceneView)SceneView.sceneViews[0];
                GUIContentCreator.LoadedTextures.Clear();
                sceneView.Focus();
            }
        }
    }
    private const float KZoomMin = 0.1f;
    private const float KZoomMax = 10.0f;


    private Vector2 _zoomCoordsOrigin = Vector2.zero;

    /// <summary>
    /// Draw zoomable area.
    /// This area contains all windows and curves.
    /// Extra GUI Items that also need to be zoomed, should be placed between BeginZoomArea() and EndZoomArea()
    /// </summary>
    private void DrawZoomArea()
    {
        Event e = Event.current;
        BeginZoomArea();

        foreach (var baseNode in ActionStorage.Windows)
        {
            baseNode.DrawCurves();
        }
        BeginWindows();
        for (int i = 0; i < ActionStorage.Windows.Count; i++)
        {
            Rect r = ActionStorage.Windows[i].GetWindowsRect();
            GUI.backgroundColor = Color.clear;
            ActionStorage.Windows[i].WindowRect = GUI.Window(i, r, DrawNodeWindow, ActionStorage.Windows[i].WindowTitle);

        }

        EndWindows();
        if (ActionStorage.ClickedNode)
        {
            Rect mouseRect = new Rect(e.mousePosition.x, e.mousePosition.y, 10, 10);
            DrawNodeCurve(ActionStorage.ClickLocation, mouseRect);
            Repaint();
        }
        EndZoomArea();

    }

    /// <summary>
    /// Draw area which is unzoomable.
    /// Currently contains background texture
    /// </summary>
    private void DrawNonZoomArea()
    {
        GUIContentCreator.TileTexture("grid-01.png",new Vector2(50, 50), new Rect(0, 0, 50, 50),  new Rect(0, 0, Screen.width, Screen.height), ScaleMode.ScaleAndCrop);
    }


    /// <summary>
    /// Called with every new event
    /// wantsMouseMove enables events on every move of the mouse.
    /// Disabling wantMouseMoves will call this function on mouse clicks only.
    /// </summary>
    void OnGUI()
    {
        
        InitializeSavedData();
        wantsMouseMove = true;
        StartDraw();

    }

    /// <summary>
    /// Main function for handeling events.
    /// Currently focused on mouse clicks
    /// </summary>
    private void HandleEvents()
    {
        Event e = Event.current;
        _mousePos = e.mousePosition;
        if (e.type == EventType.ScrollWheel)
        {
            Debug.Log("scroll");
            Vector2 screenCoordsMousePos = _mousePos;
            Vector2 delta = e.delta;
            Vector2 zoomCoordsMousePos = ConvertScreenCoordsToZoomCoords(screenCoordsMousePos);
            float zoomDelta = -delta.y / 150.0f;
            float oldZoom = _zoom;
            _zoom += zoomDelta;
            _zoom = Mathf.Clamp(_zoom, KZoomMin, KZoomMax);
            _zoomCoordsOrigin += (zoomCoordsMousePos - _zoomCoordsOrigin) - (oldZoom / _zoom) * (zoomCoordsMousePos - _zoomCoordsOrigin);
            
                e.Use();
            
        }
        else if (Event.current.button == 2)
        {
            _offset = _previousPosition - _mousePos;
            for (int index = 0; index < ActionStorage.Windows.Count; index++)
            {
                var dialogNode = ActionStorage.Windows[index];
                var rect = dialogNode.WindowRect;
                rect.position -= _offset;
                ActionStorage.Windows[index].WindowRect = rect;
            }
                e.Use();
            
        }
        else if (e.button == 1)
        {
            if (e.type == EventType.mouseDown)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Add Dialog Node"), false, ContextCallback, "StandardDialogNode");
                menu.AddItem(new GUIContent("Add Option Dialog Node"), false, ContextCallback, "OptionDialogNode");
                menu.AddItem(new GUIContent("Add Movie Dialog Node"), false, ContextCallback, "MovieDialogNode");
                menu.AddItem(new GUIContent("Add Movie Option Dialog Node"), false, ContextCallback, "MovieOptionDialogNode");
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Load from file"), false, ContextCallback, "Load");
                menu.AddItem(new GUIContent("Export"), false, ContextCallback, "Export");
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Clear Canvas"), false, ContextCallback, "Clear");
                menu.ShowAsContext();
               
                    e.Use();
                
            }
        }
        else if (e.button == 0)
        {
            if (e.type == EventType.mouseDown)
            {
                if (ActionStorage.ClickedNode)
                {
                    ActionStorage.ClickedIndex = -1;
                    ActionStorage.ClickedNode = null;
                }
                    e.Use();
                
            }
        }
        _previousPosition = _mousePos;
    }

    /// <summary>
    /// Converts Screen Coordinates to ZoomArea Coordinates
    /// </summary>
    /// <param name="screenCoords">Unzoomed coordinates</param>
    /// <returns>Zoomed coordinates</returns>
    private Vector2 ConvertScreenCoordsToZoomCoords(Vector2 screenCoords)
    {
        return (screenCoords - ZoomArea.TopLeft()) / _zoom + _zoomCoordsOrigin;
    }

    /// <summary>
    /// Main drawing function
    /// Containing the drawing of the zoomable area's and non-zoomable areas
    /// </summary>
    void StartDraw()
    {
        EditorGUI.BeginChangeCheck();
        DrawNonZoomArea();
        DrawZoomArea();
        HandleEvents();

        if (EditorGUI.EndChangeCheck())
            OnDisable();

        EditorStyles.label.normal.textColor = Color.black;
    }

    /// <summary>
    /// Used to identify the beginning of zoomable area
    /// Needs to be closed by EndZoomArea()
    /// </summary>
    public void BeginZoomArea()
    {
        ZoomArea = new Rect(0.0f, 0, Screen.width, Screen.height);
        EditorZoomArea.Begin(_zoom, ZoomArea);
    }

    /// <summary>
    /// Closing function for BeginZoomArea()
    /// </summary>
    public void EndZoomArea()
    {
        EditorZoomArea.End();
    }

    /// <summary>
    /// Callback function for each window.
    /// </summary>
    /// <param name="id">Window identification number</param>
    void DrawNodeWindow(int id)
    {
        ActionStorage.Windows[id].DrawWindow();
        GUI.DragWindow();
    }

    /// <summary>
    /// Callback function which will be called on menu clicks
    /// </summary>
    /// <param name="userData">Clicked Menu Item</param>
    void ContextCallback(object userData)
    {
        switch (userData.ToString())
        {
            case "StandardDialogNode":
                StandardDialogNode standardDialogNode = ScriptableObject.CreateInstance("StandardDialogNode") as StandardDialogNode;
                standardDialogNode.WindowRect = new Rect(_mousePos.x, _mousePos.y, GUIContentCreator.StandardDialogNodeSize.x, GUIContentCreator.StandardDialogNodeSize.y);
                standardDialogNode.Id = _nodeCounter;
                _nodeCounter++;
                ActionStorage.Windows.Add(standardDialogNode);
                break;
            case "OptionDialogNode":
                OptionDialogNode optionDialogNode = ScriptableObject.CreateInstance("OptionDialogNode") as OptionDialogNode;
                optionDialogNode.WindowRect = new Rect(_mousePos.x, _mousePos.y, GUIContentCreator.OptionDialogNodeSize.x, GUIContentCreator.OptionDialogNodeSize.y);
                optionDialogNode.Id = _nodeCounter;
                _nodeCounter++;
                Debug.Log(optionDialogNode.WindowRect);
                ActionStorage.Windows.Add(optionDialogNode);
                break;
            case "MovieDialogNode":
                MovieNode MovieDialogNode = ScriptableObject.CreateInstance("MovieNode") as MovieNode;
                MovieDialogNode.WindowRect = new Rect(_mousePos.x, _mousePos.y, GUIContentCreator.StandardDialogNodeSize.x, GUIContentCreator.StandardDialogNodeSize.y);
                MovieDialogNode.Id = _nodeCounter;
                _nodeCounter++;
                ActionStorage.Windows.Add(MovieDialogNode);
                break;
            case "MovieOptionDialogNode":
                MovieOptionNode MovieOptionNode = ScriptableObject.CreateInstance("MovieOptionNode") as MovieOptionNode;
                MovieOptionNode.WindowRect = new Rect(_mousePos.x, _mousePos.y, GUIContentCreator.OptionDialogNodeSize.x, GUIContentCreator.OptionDialogNodeSize.y);
                MovieOptionNode.Id = _nodeCounter;
                _nodeCounter++;
                Debug.Log(MovieOptionNode.WindowRect);
                ActionStorage.Windows.Add(MovieOptionNode);
                break;
            case "Export":
                ExportJson();
                Debug.Log(CreateJSON());
                break;
            case "Clear":
                ClearData();
                break;
            case "Load":
                var path = EditorUtility.OpenFilePanel(
                        "Load Json file",
                        "",
                        "txt,json");
                if (path.Length != 0)
                {
                    ClearData();
                    InitializeSavedData(File.ReadAllText(path));
                }
                break;
        }
    }

    private void ExportJson()
    {
        var path = EditorUtility.SaveFilePanel(
                 "Export to Json",
                 "",
                 "Node Editor Output.json",
                 "json");

        if (path.Length != 0)
        {
            var outputData = CreateJSON();
            File.WriteAllText(path, outputData);
        }
    }

    /// <summary>
    /// Remove all saved Data. Clearing will output Json first, in case of accident clicking
    /// </summary>
    private void ClearData()
    {
        Debug.Log(CreateJSON());
        EditorPrefs.SetString("json", "");
        for (int index = 0; index < ActionStorage.Windows.Count; index++)
        {
            var baseDialogNode = ActionStorage.Windows[index];
            baseDialogNode.DeleteNode(baseDialogNode);
        }
        ActionStorage.Windows.Clear();
        _nodeCounter = 0;
    }

    /// <summary>
    /// Generate a JSON file of all nodes currently on screen
    /// </summary>
    /// <returns>JSON string</returns>
    private String CreateJSON()
    {
        if (ActionStorage.Windows.Count == 0)
        {
            Debug.Log("Nothing to export");
            return "";
        }
        var nodelist = new NodeList();
        foreach (var baseNode in ActionStorage.Windows)
        {
            nodelist.nodes.Add(baseNode.GetNode());
        }
        return JsonUtility.ToJson(nodelist);
    }

    /// <summary>
    /// Draw a curve between two points.
    /// </summary>
    /// <param name="start">The starting point of the curve. Alignment: Middle Right</param>
    /// <param name="end">The ending point of the curve. Alignment: Middle Left</param>
    public static void DrawNodeCurve(Rect start, Rect end)
    {
        Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2);
        Vector3 endPos = new Vector3(end.x + end.width, end.y + end.height / 2);
        Vector3 startTan = startPos + Vector3.right * 50;
        Vector3 endTan = endPos + Vector3.left * 50;
        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 3);
    }

    void OnDisable()
    {
        OnLostFocus();
    }
    void OnLostFocus()
    {
        
        if (ActionStorage.Windows.Count == 0) return;
        EditorPrefs.SetString("json", CreateJSON());
    }

    void OnFocus()
    {
        var prefs = EditorPrefs.GetString("json");
        //ClearData();
        InitializeSavedData(prefs);
        Repaint();
    }

    /// <summary>
    /// Function to reload saved data.
    /// </summary>
    private void InitializeSavedData(String data = "")
    {
        if (ActionStorage.Windows.Count == 0)
        {
            NodeList nodes = JsonUtility.FromJson<NodeList>(data.Length > 0 ? data : EditorPrefs.GetString("json"));
            _nodeCounter = 0;
            if (nodes == null) return;
            foreach (var node in nodes.nodes)
            {
                _nodeCounter = node.NodeId > _nodeCounter ? node.NodeId : _nodeCounter;
                _nodeCounter++;
                BaseDialogNode newNode = null;
                switch (node.NodeType)
                {
                    case GUIContentCreator.MovieDialog:
                        newNode = ScriptableObject.CreateInstance("MovieNode") as MovieNode;
                        newNode.WindowRect = new Rect(node.position[0], node.position[1],
                            GUIContentCreator.MovieNodeSize.x, GUIContentCreator.MovieNodeSize.y);
                        break;
                    case GUIContentCreator.MovieOptionDialog:
                        newNode = ScriptableObject.CreateInstance("MovieOptionNode") as MovieOptionNode;
                        newNode.WindowRect = new Rect(node.position[0], node.position[1],
                            GUIContentCreator.MovieOptionNodeSize.x, GUIContentCreator.MovieOptionNodeSize.y);

                        break;
                    case GUIContentCreator.StandardDialog:
                        newNode = ScriptableObject.CreateInstance("StandardDialogNode") as StandardDialogNode;
                        newNode.WindowRect = new Rect(node.position[0], node.position[1],
                            GUIContentCreator.StandardDialogNodeSize.x, GUIContentCreator.StandardDialogNodeSize.y);
                        break;
                    case GUIContentCreator.StandardOptionDialog:
                        newNode = ScriptableObject.CreateInstance("OptionDialogNode") as OptionDialogNode;
                        newNode.WindowRect = new Rect(node.position[0], node.position[1],
                            GUIContentCreator.OptionDialogNodeSize.x, GUIContentCreator.OptionDialogNodeSize.y);
                        break;
                }
                if (!newNode) continue;
                newNode.Id = node.NodeId;
                newNode.DialogText = node.Text;
                if (node.Avatar.Length > 0)
                {
                    newNode.Attachment = node.Avatar;
                    newNode.Avatar = AssetDatabase.LoadAssetAtPath(node.Avatar, typeof(Sprite)) as Sprite;
                }
                if (node.MovieTexture.Length > 0)
                {
                    newNode.Attachment = node.MovieTexture;
                    newNode.MovieTexture = AssetDatabase.LoadAssetAtPath(node.MovieTexture, typeof(MovieTexture)) as MovieTexture;
                }
                newNode.DelayText = node.Delay;
                newNode.StartingNode = node.StartingNode;
                newNode.DefaultOption = node.DefaultOption;
                newNode.NextNodeDelay = node.NextNodeAfter;
                newNode.Grammar = node.Grammar;
                foreach (var nodeOption in node.Options)
                {
                    newNode.Options.Add(new Boo.Lang.List<string>()
                    {
                        nodeOption.text,
                        String.Join(" ", nodeOption.Keywords)
                    });
                    newNode.extraItems += 3;
                    newNode.OutputNodes.Add(null);
                }
                ActionStorage.Windows.Add(newNode);
            }
            foreach (var node in nodes.nodes)
            {
                ActionStorage.ClickedNode = ActionStorage.Windows.Find(x => x.Id == node.NodeId);
                if (node.Options.Count == 0 && node.NextNode > -1)
                {
                    ActionStorage.ClickedIndex = 0;
                    ActionStorage.ClickedNode.OutputNodes.Add(null);
                    var n = ActionStorage.Windows.Find(x => x.Id == node.NextNode);
                    ActionStorage.ClickedNode.SetOutput(n);
                }
                else
                {
                    for (int i = 0; i < node.Options.Count; i++)
                    {
                        var nodeOption = node.Options[i];
                        ActionStorage.ClickedIndex = i;
                        ActionStorage.ClickedNode.OutputNodes.Add(null);
                        ActionStorage.ClickedNode.SetOutput(
                            ActionStorage.Windows.Find(x => x.Id == nodeOption.NextNode), i);
                    }
                }
                ActionStorage.ClickedIndex = -1;
                ActionStorage.ClickedNode = null;
            }
            OnLostFocus();
        }
    }
}
