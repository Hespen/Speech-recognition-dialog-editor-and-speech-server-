package main;

/**
 * Created by Peter on 1-11-2016.
 */
public class Grammar {
    public String dictionary;
    public String name;
    public String location;
    public String language;

    /**
     * Grammar file containing the name, location and language
     * @param name Name used to identify this grammar file
     * @param location
     * @param language
     */
    public Grammar(String name, String location, String language, String customDict) {
        this.name=name;
        this.location=location;
        this.language=language;
        this.dictionary=customDict;
    }
}
