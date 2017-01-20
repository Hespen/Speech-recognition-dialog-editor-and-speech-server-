using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Dia;
using UnityEngine;

public class SceneController : MonoBehaviour
{

    private List<Node> _nodes;
    private DialogController _dialogController;
    private bool _running;
    private Node _currentNode;
    private Dialog _currentDialog;
    private SpeechRecognition _speechRecognizer;
    private Recorder _recorder;

    
	void Start () {
		ClearData();
	}
	
	/// <summary>
    /// Update Method checks if dialog is running and if a node has been selected.
    /// Updates the dialog
    /// </summary>
	void Update () {
	    if (!_running)return;
	    if (_currentNode == null) _currentNode=_nodes.Find(x => x.StartingNode);
	    if(_currentDialog==null) CheckNodeType();
	    _currentDialog.Update();
	}

    /// <summary>
    /// Function to check the type of Node.
    /// All Node types used in the JSON, should be present in this function
    /// </summary>
    private void CheckNodeType()
    {
        switch (_currentNode.NodeType)
        {
            case GUIContentCreator.StandardDialog:
                _currentDialog= gameObject.AddComponent<StandardDialog>();
                _currentDialog.SetVariables(_currentNode,_dialogController);
                break;
            case GUIContentCreator.StandardOptionDialog:
                _currentDialog = gameObject.AddComponent<StandardOptionDialog>();
                _currentDialog.SetVariables(_currentNode, _dialogController);
                break;
            case GUIContentCreator.MovieDialog:
                _currentDialog = gameObject.AddComponent<MovieDialog>();
                _currentDialog.SetVariables(_currentNode, _dialogController);
                break;
            case GUIContentCreator.MovieOptionDialog:
                _currentDialog = gameObject.AddComponent<MovieOptionDialog>();
                _currentDialog.SetVariables(_currentNode, _dialogController);
                break;
        }
    }

    /// <summary>
    /// Deserialize Json to nodes
    /// </summary>
    /// <param name="text">Json</param>
    public void DeserializeJson(string text)
    {
        var list=JsonUtility.FromJson<NodeList>(text);
        if (list != null)
        {
            _nodes = list.nodes;
            foreach (var node in _nodes)
            {
                node.MovieTexture = node.MovieTexture.Replace("Assets/Resources/", "").Replace("/(.*)\\.[^.]+$/", "");
                if (node.MovieTexture.LastIndexOf('.') > 0)
                {
                    node.MovieTexture = node.MovieTexture.Substring(0, node.MovieTexture.LastIndexOf('.'));
                }
                node.Avatar = node.Avatar.Replace("Assets/Resources/", "");
                if (node.Avatar.LastIndexOf('.') > 0)
                {
                    node.Avatar = node.Avatar.Substring(0, node.Avatar.LastIndexOf('.'));
                }
                print(node.Avatar);
            }
        }
        else
        {
            Debug.LogError("TextAsset does not contain valid Json");
        }
    }

    /// <summary>
    /// Set Reference from DialogController
    /// </summary>
    /// <param name="dialogController">Current DialogController</param>
    public void SetDialogController(DialogController dialogController)
    {
        _dialogController = dialogController;
    }

    /// <summary>
    /// Call the next dialog node
    /// </summary>
    /// <param name="nodeId">Next dialog id</param>
    public void SetDialogNode(int nodeId)
    {
        ClearData();
        _currentNode = _nodes.Find(x=>x.NodeId==nodeId);
    }

    /// <summary>
    /// Clear all data
    /// Including text and videos from scene
    /// </summary>
    private void ClearData()
    {
        _currentNode = null;
        Destroy(_currentDialog);
        _dialogController.CharacterPortrait.sprite = null;
        _dialogController.TextAreaForDialog.text = "";
        _dialogController.MovieScreenForMovieDialog.GetComponent<AudioSource>().Stop();
        _dialogController.MovieScreenForMovieDialog.GetComponent<Renderer>().material.mainTexture=null;
        Destroy(_recorder);
        Destroy(_speechRecognizer);
        _speechRecognizer = null;
        foreach (var dialogControllerTextAreaForDialogOption in _dialogController.TextAreaForDialogOptions)
        {
            dialogControllerTextAreaForDialogOption.text = "";
            dialogControllerTextAreaForDialogOption.transform.parent.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Start recognition if available
    /// </summary>
    /// <returns>If Speech is enabled and options are available return true, else false</returns>
    public bool StartRecognition()
    {
        if (_dialogController.SpeechRecognitionSystem!=SpeechRecognition.SpeechRecognitionSystem.None && _currentNode.Options.Count>0)
        {
            switch (_dialogController.SpeechRecognitionSystem)
            {
                case SpeechRecognition.SpeechRecognitionSystem.Wit:
                    _speechRecognizer = gameObject.AddComponent<Wit>();
                    _speechRecognizer.SetVariables("TCMS34MGHCMIHUGJED4PGXDIS4CGA4ML", this);
                    break;
                case SpeechRecognition.SpeechRecognitionSystem.Google:
                    _speechRecognizer = gameObject.AddComponent<GoogleSpeech>();
                    _speechRecognizer.SetVariables("AIzaSyAHSIshzaLlMYiV1WMWvBrSyaOaTqNlYGc", this);
                    break;
                case SpeechRecognition.SpeechRecognitionSystem.Sphinx:
                    _speechRecognizer = gameObject.AddComponent<Sphinx>();
                    _speechRecognizer.SetVariables(null, this);
                    ((Sphinx)_speechRecognizer).SetScene(_currentNode.Grammar.ToString());
                    break;
            }
            _recorder=gameObject.AddComponent<Recorder>();
            _recorder.SetSpeechRecognizer(_speechRecognizer);
            _currentDialog.SetSpeechRecognizer(_recorder);
            return true;
        }
        return false;
    }

    public long GetCountdownTimerInMilliseconds()
    {
        return _currentDialog!=null?_currentDialog.GetRemainingTimeUntilNextNodeInMilliseconds():0;
    }

    public int GetCountdownTimerInSeconds()
    {
        return _currentDialog != null ? _currentDialog.GetRemainingTimeUntilNextNodeInSeconds():0;
    }
    /// <summary>
    /// Call to start the dialog
    /// </summary>
    public void StartDialog()
    {
        _running = true;
    }

    /// <summary>
    /// Method to select the correct answer
    /// Input should be: Answer 1 -> 1
    /// </summary>
    /// <param name="answer"></param>
    public void Answer(int answer)
    {
        if (answer<0)
        {
            _currentDialog.CheckNextNode();
            return;
        }
        _currentDialog.SelectAnswer(answer-1);
    }

    public Recorder GetRecorder()
    {
        return _recorder;
    }

    /// <summary>
    /// Callback for speech recognition.
    /// Calculates the accuracy for each option
    /// Option with highest accuracy will be picked.
    /// </summary>
    /// <param name="result"></param>
    public void RecognitionCallback(String result)
    {
        Option node = null;
        float best = 0;
        Debug.Log(result);
        foreach (var currentNodeOption in _currentNode.Options)
        {
            foreach (var resultKeyword in result.Split(null))
            {
                float acc = 0;
                foreach (var optionKeywords in currentNodeOption.Keywords)
                {
                    if (resultKeyword.ToLower().Equals(optionKeywords.ToLower()))
                    {
                        acc++;
                        break;
                    }
                }
                acc = acc/currentNodeOption.Keywords.Length;
                if (acc > best)
                {
                    best = acc;
                    node = currentNodeOption;
                }
            }
        }
        if (best > 0)
        {
            _currentDialog.SelectAnswer(_currentNode.Options.FindIndex(x => x == node));
           _recorder.StopRecording();
        }
        else
        {
            _recorder.StartRecording();
        }
    }
}
