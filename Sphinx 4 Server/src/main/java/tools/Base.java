package tools;

import java.io.BufferedReader;
import java.io.FileOutputStream;
import java.io.FileReader;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashSet;
import java.util.Set;

/**
 * Created by Peter on 2-11-2016.
 */
public class Base {

    //Path to folder. Can be /english/ or /dutch/
    private static String path = "src/main/resources/english/";

    //Location of grammar file inside the language folder (excl. file extension)
    private static String grammarFile = "grammars/Scene_1";

    //Location of dictionary in language folder
    private static String dictionaryFile = "dict.dict";
    private static Set<String> grammarWords = new HashSet<>();

    public static void main(String[] args) throws IOException {
        try (BufferedReader br = new BufferedReader(new FileReader(path + grammarFile + ".gram"))) {
            String line;
            while ((line = br.readLine()) != null) {
                processLine(line);
            }
        }
        FileOutputStream fous = new FileOutputStream(path + grammarFile + ".dict");
        ArrayList<String> foundWords = new ArrayList<>();
        try (BufferedReader br = new BufferedReader(new FileReader(path + dictionaryFile))) {
            String line;
            while ((line = br.readLine()) != null) {
                String[] split = line.split(" ");
                for (String words : grammarWords) {
                    if (split[0].equals(words)) {
                        foundWords.add(split[0]);
                        System.out.println(line);
                        fous.write((line + "\n").getBytes());
                    }
                }
            }
        }
        Set<String> words = new HashSet<String>(grammarWords);
        Set<String> dictionary = new HashSet<String>(foundWords);
        words.removeAll(dictionary);
        for (String str : words) {
            if (str.length() == 0) continue;
            if(str.equals("+"))continue;
            fous.write(("ERROR NO REFERENCE FOUND IN DICTIONARY: " + str + "\n").getBytes());
        }
        fous.flush();
        fous.close();
    }

    private static void processLine(String line) {
        if (line.startsWith("<")) {
            String[] split = line.split("=");
            split[1] = split[1].replaceAll(" ", "").replaceAll(";", "").replaceAll("\\(","").replaceAll("\\)","");
            String[] words = split[1].split("\\|");
            Collections.addAll(grammarWords, words);
        } else if (line.startsWith("public")) {
            String[] split = line.split("=");
            split[1] = split[1].replaceAll("<opt>\\*", "").replaceAll(";", "").replaceAll("[\\s*]", "|").replaceAll("\\|\\|", "|").replaceAll("\\(","").replaceAll("\\)","");
            String[] words = split[1].split("\\|");
            Collections.addAll(grammarWords, words);
        }
    }
}
