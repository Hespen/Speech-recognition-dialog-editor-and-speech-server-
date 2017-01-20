import java.io.*;
import java.util.ArrayList;
import java.util.HashSet;
import java.util.Set;

/**
 * Created by Peter on 11-1-2017.
 */
public class DictionaryCreator {
    private static Set<String> grammarWords = new HashSet<>();

    public DictionaryCreator(String input) throws IOException {
        try {
            BufferedReader br = new BufferedReader(new InputStreamReader(new FileInputStream("output.tmp.vocab"), "UTF-8"));
            String line;
            int counter = 0;
            while ((line = br.readLine()) != null) {
                //Skip Header
                if (counter < 4) {
                    counter++;
                    continue;
                }
                grammarWords.add(line);
            }
        } catch (FileNotFoundException e) {
            e.printStackTrace();
        }

        FileOutputStream fous = new FileOutputStream("output.dict");
        ArrayList<String> foundWords = new ArrayList<>();
        try {
            BufferedReader br = new BufferedReader(new InputStreamReader(new FileInputStream(input), "UTF-8"));
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
            br.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
        Set<String> words = new HashSet<String>(grammarWords);
        Set<String> dictionary = new HashSet<String>(foundWords);
        words.removeAll(dictionary);
        for (String str : words) {
            if (str.length() == 0) continue;
            System.err.println(("ERROR NO REFERENCE FOUND IN DICTIONARY: " + str));
        }

        fous.flush();
        fous.close();
    }


}
