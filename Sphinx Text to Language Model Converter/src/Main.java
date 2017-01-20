import java.io.*;
import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

public class Main {
    static String prjPath;
    static String[] languages = {"en", "nl", "de"};
    static List<String> names = new ArrayList<String>() {{
        add("idngram2lm.exe");
        add("text2wfreq.exe");
        add("text2idngram.exe");
        add("wfreq2vocab.exe");
        add("output.idngram");
        add("output.tmp.vocab");
        add("output.txt");
    }};

    public static void main(String[] args){
        String path = args[1];
        String result = "";
        prjPath = System.getProperty("user.dir");


        try{
            //Parse text
            result = removePunctuation(readFile(path, StandardCharsets.UTF_8));
            result = addSilentTags(result);
            StartConversion(result);

            //Check if Dictionary is required
            if (args.length > 2) {
                File f = new File(args[3]);
                if (f.exists()) {
                    new DictionaryCreator(args[3]);
                } else if (Arrays.asList(languages).contains(args[3])) {
                    new DictionaryCreator(ExportResource("/" + args[3] + ".dic"));
                    names.add(args[3]+".dic");
                } else {
                    System.out.println("Dictionary not found");
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
        }

        //Remove all temp files
        for (String name : names) {
            new File(name).delete();
        }
    }

    private static void StartConversion(String result) throws Exception {

        //write string to text file
        File f = new File(prjPath + "/output.txt");
        BufferedWriter bw = new BufferedWriter(new FileWriter(f));
        bw.write(result);
        bw.close();

        runProcess(ExportResource("/text2wfreq.exe") + " < output.txt | " + ExportResource("/wfreq2vocab.exe") + " > output.tmp.vocab");
        runProcess(ExportResource("/text2idngram.exe") + " -vocab output.tmp.vocab -idngram output.idngram < output.txt");
        runProcess(ExportResource("/idngram2lm.exe") + " -vocab_type 0 -idngram output.idngram -vocab output.tmp.vocab -arpa output.lm");

    }

    private static void runProcess(String command) throws IOException, InterruptedException {
        ProcessBuilder pb = new ProcessBuilder("cmd.exe", "/C", command);
        pb.redirectOutput(ProcessBuilder.Redirect.INHERIT);
        pb.redirectError(ProcessBuilder.Redirect.INHERIT);
        Process p = pb.start();
        p.waitFor();
        p.destroyForcibly();
    }

    private static String addSilentTags(String text) {
        String silentTagged = "";
        for (String line : text.split("\r\n")) {
            silentTagged += "<s> " + line.toLowerCase() + " </s>\r\n";
        }
        return silentTagged;
    }

    public static String removePunctuation(String text) {
        text = text.replace(".", "\r\n");
        text = text.replaceAll("[\\\"Ó,;:%¿?¡!()\\[\\]{}<>_\\./@#$€^*']", "");
        text = text.replaceAll("[éè]", "e");
        text = text.replaceAll("-", " ");
        text = text.replaceAll("[0-9]", "");
        text = text.replace("\r\n\r\n", "\r\n");
        text = text.replace("\r\n ", "\r\n");
        return text;
    }

    static String readFile(String path, Charset encoding)
            throws IOException {
        byte[] encoded = Files.readAllBytes(Paths.get(path));
        return new String(encoded, encoding);
    }

    static public String ExportResource(String resourceName) throws Exception {
        InputStream stream = null;
        OutputStream resStreamOut = null;
        String jarFolder;
        try {
            stream = Main.class.getResourceAsStream(resourceName);//note that each / is a directory down in the "jar tree" been the jar the root of the tree
            if (stream == null) {
                throw new Exception("Cannot get resource \"" + resourceName + "\" from Jar file.");
            }

            int readBytes;
            byte[] buffer = new byte[4096];
            jarFolder = new File(Main.class.getProtectionDomain().getCodeSource().getLocation().toURI().getPath()).getParentFile().getPath().replace('\\', '/');
            resStreamOut = new FileOutputStream(jarFolder + resourceName);
            while ((readBytes = stream.read(buffer)) > 0) {
                resStreamOut.write(buffer, 0, readBytes);
            }
        } catch (Exception ex) {
            throw ex;
        } finally {
            stream.close();
            resStreamOut.close();
        }

        return resourceName.substring(1);
    }

}
