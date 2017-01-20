using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Connect this script to your scene. 
/// Required: Audio Listener and Audio Source
/// Audio Source output should be set to 'Recorder' (Assets/Plugins/dialogreader)
/// </summary>
public class DialogController : MonoBehaviour
{

    ///Exported JSON
    public TextAsset JsonDialogStructure;

    //Text where dialog will be shown
    public Text TextAreaForDialog;

    //Array of text where options will be shown. Make sure this array is as big as the most amount of options you defined
    public Text[] TextAreaForDialogOptions;

    //Area where movie will be played on
    public GameObject MovieScreenForMovieDialog;

    //Area where Avatar / Character Portrait will be shown
    public SpriteRenderer CharacterPortrait;

    //Used to select a speech recognition system
    public SpeechRecognition.SpeechRecognitionSystem SpeechRecognitionSystem;

    // Reference to the Scene Controller. Initialized in Start();
    private SceneController _sceneController;

    public bool AnimateText = true;
    
    
	void Start ()
	{
	    _sceneController = gameObject.AddComponent<SceneController>();
        _sceneController.SetDialogController(this);
    }

    /// <summary>
    /// Return the current Scene Controller
    /// </summary>
    /// <returns>Current Scene Controller</returns>
    public SceneController GetSceneController()
    {
        return _sceneController;
    }

    /// <summary>
    /// Call to start the dialog
    /// </summary>
    public void StartDialog()
    {
        if (!_sceneController)
        {
            Debug.LogError("DialogController has not been attached or enabled");
            return;
        }
        _sceneController.DeserializeJson(JsonDialogStructure.text);
        _sceneController.StartDialog();
    }

    /// <summary>
    /// Callback function for option buttons
    /// Button 1: 1
    /// Button 2: 2
    /// Button 3: 3
    /// etc.
    /// </summary>
    /// <param name="answer">Answer number (start from 1)</param>
    public void SelectAnswer(int answer)
    {
        _sceneController.Answer(answer);
    }

    /// <summary>
    /// Select the default answer if no answer has been given.
    /// Automatically called if delay has been set in json
    /// </summary>
    public void SelectDefaultAnswer()
    {
        _sceneController.Answer(-1);
    }
}
