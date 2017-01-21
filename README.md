**Feature list:**  

- Dialog editor, for normal dialogs or dialogs with user options.
- Dialogs to implement cutscenes or cutscenes with user options
- Option to use different grammar files for each dialog (Sphinx 4)
- Export to Json & Load Json
- Typewriter animations
- Delay before a next dialog shows!
- Out of the box support for 3 speech recognition systems. Google Speech, Wit.ai and Sphinx 4.
- Automatic selection of dialog answers on speech result. Let's calculate that accuracy!
- Audio input analyzer. When did the user start talking and when did he stop? Let's cut that audio out, and recognize it!
- Voice Activation Volume adjuster
- Callbacks for timers, automatic answer selection and current state of audio input (no audio, listening & analyzing speech)
- Includes a working Sphinx 4 Server, and text to language model tool. (out of the box support for English, Dutch and German)
- Automatic grammar files to dictionary to decrease server load
- Docs available in source and here: [https://hespen.net/Portfolio/UnityDialogEditor/annotated.html](https://hespen.net/Portfolio/UnityDialogEditor/annotated.html)

Remember this is **not** a finished product, as there still are some bugs in the dialog editor. And I haven't had the time to make it beautifull yet. I did implement this in a VR game, but as that is part of a company, I can't share that one.

**How to use:**  
Enable Microphone Setup Object and run it and toggle the button for like 5 seconds while being silent. Toggle it off, and speak. When you speak the square should become green. (saved in prefs automatically)  
  
Stop the game, disable the microphone setup object. Select the speech system you'd like to use on the Main Camera object. Press enter to start the dialog.  
  
Remember: Google en Wit.ai are really slow, use Sphinx 4 for the fastest result! I did research on the implemented speech recognition systems and their accuracy and speed. Sphinx is the fastest with an average of 200ms recognition (external server)! Where Google and Wit.ai will need atleast 2-5 seconds. (tested with 3200 audiofiles, 2 languages)  
  
  
**The editor: **  
Windows -&gt; Nodes Editor. Right click to create new nodes or load the json file. Demo Json in Resources folder. Middle click to drag, scroll to zoom. Right click to export to json. You can attach the json to the main camera!  
  

- Keywords are used for speech recognition! They determine the accuracy.
- Delay in Seconds before dialog is shown
- Time until next node is a delay before the next node is shown. This one starts counting after the first delay has passed.

**Video**

Demo

[https://www.youtube.com/watch?v=5kEO3hXBO1A](https://www.youtube.com/watch?v=5kEO3hXBO1A)

Dialog Editor Example

[https://www.youtube.com/watch?v=QjPzp2W6zRU](https://www.youtube.com/watch?v=QjPzp2W6zRU)

Installing the Sphinx 4 Server

[https://youtu.be/tkeS9XtGdjI](https://youtu.be/tkeS9XtGdjI)
