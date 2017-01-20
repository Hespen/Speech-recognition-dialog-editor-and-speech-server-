package main;

import edu.cmu.sphinx.api.Configuration;
import edu.cmu.sphinx.api.Context;
import edu.cmu.sphinx.recognizer.Recognizer;

import java.util.HashMap;

/**
 * Created by Peter on 1-11-2016.
 */
public class Base {

    public static final HashMap<String, Grammar> grammars;
    static String newline = System.getProperty("line.separator");
    private static String timedOut;
    static{


    }

    static {
        //Grammar [0] is the main grammar file! This one will be initialized when no specific grammar has been set
        grammars = new HashMap<>();
        grammars.put("0", new Grammar("Standard English", "Scene_1", Language.ENGLISH,"Scene_1"));

        grammars.put("1", new Grammar("Scene 1, 3 en 4", "sc1", Language.DUTCH,"sc1"));
        grammars.put("2", new Grammar("Scene 2", "sc2", Language.DUTCH,"sc2"));
        grammars.put("3", new Grammar("Scene 5", "sc5", Language.DUTCH,"sc5"));
        grammars.put("4", new Grammar("Scene 6", "sc6", Language.DUTCH,"sc6"));
        grammars.put("5", new Grammar("Selectie", "selection", Language.DUTCH,"selection"));
    }

    public static void main(String[] args) {;
        SpeechServer ss = new SpeechServer();
        timedOut="Recognizer is shutdown, please create a new request to a specified grammar"+newline;
        timedOut+= "---------------------"+newline+"Available Grammars"+newline;
        for (String grammarName : grammars.keySet()){
            timedOut+="Identifier: \""+grammarName+"\" for Scene: \""+grammars.get(grammarName).name+"\""+newline;
        }
        timedOut+="Recognizers are automatically shutdown when no requests have been made within 10 minutes";
        ss.Start();
    }

    public static String getTimedOut() {
        return timedOut;
    }
}
