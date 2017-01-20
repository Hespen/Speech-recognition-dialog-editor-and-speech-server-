package main;

import edu.cmu.sphinx.api.Configuration;
import edu.cmu.sphinx.api.Context;
import edu.cmu.sphinx.api.SpeechResult;
import edu.cmu.sphinx.result.Result;
import edu.cmu.sphinx.util.TimeFrame;

import java.io.*;

/**
 * Created by Peter on 1-11-2016.
 */
public class Recognizer {
    private final Configuration configuration;
    private Grammar Grammar;
    private final edu.cmu.sphinx.recognizer.Recognizer recognizer;
    private Context context;
    private long lastUsed;

    public Recognizer(Grammar grammar){
        Grammar=grammar;
        configuration = new Configuration();
        configuration.setAcousticModelPath("src/main/resources/" + grammar.language + "/model");
        if(grammar.location.length()==0){
            configuration.setLanguageModelPath("src/main/resources/" + grammar.language + "/lm.lm");
            configuration.setDictionaryPath("src/main/resources/" + grammar.language + "/dict.dict");
            configuration.setUseGrammar(false);
        }else{
            configuration.setUseGrammar(true);
            configuration.setGrammarPath("src/main/resources/" + grammar.language + "/grammars/");
            configuration.setGrammarName(grammar.location);
            if(grammar.dictionary.length()==0){
                configuration.setDictionaryPath("src/main/resources/" + grammar.language + "/dict.dict");
            }else{
                configuration.setDictionaryPath("src/main/resources/" + grammar.language + "/grammars/"+grammar.dictionary+".dict");
            }
            }
        try {
            context = new Context(configuration);
        } catch (IOException e1) {
            e1.printStackTrace();
        }
        recognizer = context.getInstance(edu.cmu.sphinx.recognizer.Recognizer.class);
        lastUsed = System.currentTimeMillis();
        recognizer.allocate();
    }

    public edu.cmu.sphinx.recognizer.Recognizer getRecognizer(){
        return recognizer;
    }

    public String recognize(byte[] input) {
        lastUsed = System.currentTimeMillis();
        String str = new String(input);
        try {
            FileOutputStream fileOutputStream = new FileOutputStream("src/main/resources/"+lastUsed+".wav");
            fileOutputStream.write(input,0,input.length);
            fileOutputStream.flush();
            fileOutputStream.close();
           // ByteArrayInputStream bais = new ByteArrayInputStream(input);
            FileInputStream fis = new FileInputStream("src/main/resources/"+lastUsed+".wav");
            context.setSpeechSource(fis, TimeFrame.INFINITE);
            Result result;
            while ((result = recognizer.recognize()) != null) {
                SpeechResult speechResult = new SpeechResult(result);
                System.out.println(speechResult.getHypothesis());
                fis.close();
                return speechResult.getHypothesis();
            }
            fis.close();
        } catch (FileNotFoundException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        } finally {
            File f = new File("src/main/resources/"+lastUsed+".wav");
            f.delete();
        }
        return "No speech was recognized. Please try again.";
    }

    public void Stop(){
        recognizer.deallocate();
        System.out.println(getGrammar().name +" recognizer shutdown");
    }

    public Grammar getGrammar() {
        return Grammar;
    }

    public long getLastUsed() {
        return lastUsed;
    }
}
