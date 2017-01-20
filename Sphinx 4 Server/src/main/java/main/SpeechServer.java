package main;


import org.apache.commons.io.IOUtils;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.io.PrintWriter;
import java.net.ServerSocket;
import java.net.Socket;
import java.util.*;

/**
 * Created by Peter on 1-11-2016.
 */
public class SpeechServer {

    private ServerSocket serverSocket;

    //Contains al running recognizers
    private static HashMap<Recognizer, ArrayList<String>> runningRecognizers;


    public SpeechServer() {
        runningRecognizers = new HashMap<>();
    }

    /**
     * Start listening to the socket
     */
    public void Start() {
        Timer timer = new Timer();
        timer.schedule(new Cleaner(), 0, 10000);
        try {
            serverSocket = null;
            serverSocket = new ServerSocket(85);
            System.out.println(serverSocket.getInetAddress().getHostName());
            Listen(serverSocket);
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    /**
     * Check of new connections on the socket
     * @param serverSocket Active socket
     */
    private void Listen(ServerSocket serverSocket) {
        for (; ; ) {
            try {
                Socket clientSocket;
                clientSocket = serverSocket.accept();
                System.out.println(clientSocket.getRemoteSocketAddress());
                new Thread(new SpeechServerHandler(clientSocket)).start();
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }

    public void Stop() {
        try {
            serverSocket.close();
        } catch (IOException e) {

        }
    }

    public class SpeechServerHandler implements Runnable {

        private final Socket clientSocket;
        private OutputStream os;
        private InputStream is;

        public SpeechServerHandler(Socket clientSocket) {
            this.clientSocket = clientSocket;
        }

        public void run() {
            try {
                clientSocket.setSoTimeout(10000);
                is = clientSocket.getInputStream();
                os = clientSocket.getOutputStream();

                /* Get Input Data Length */
                byte[] arr = IOUtils.toByteArray(is, 4);
                int length = (arr[0] << 24) & 0xff000000 |
                        (arr[1] << 16) & 0x00ff0000 |
                        (arr[2] << 8) & 0x0000ff00 |
                        (arr[3] << 0) & 0x000000ff;

                /* Get Input Data */
                arr = IOUtils.toByteArray(is, length);

                /* Split Input Data */
                /* [0] Client Identifier */
                /* [1] Input Data */
                String[] result = new String(arr, "UTF-8").split(";");

                /* Determine Input Target */
                CheckInputData(result[0], result[1],arr);
                clientSocket.close();
            } catch (IOException e) {
                e.printStackTrace();
                System.out.println("error");
            } finally {
                System.out.println("CLOSE");
                try {
                    clientSocket.close();
                } catch (IOException e) {
                    e.printStackTrace();
                }
            }
        }

        private void CheckInputData(String identifier, String input, byte[] binaryInput) {
            if (Base.grammars.containsKey(input)) { // If the grammar table knows the input grammar
                boolean foundRecognizer = false;
                for (Recognizer recognizer : runningRecognizers.keySet()) {
                    if (recognizer.getGrammar() == Base.grammars.get(input)) { // If the current running recognizer, runs on the found grammar file
                        if (!runningRecognizers.get(recognizer).contains(identifier)) { // If the current running recognizer has not attached the user attach it
                            System.out.println("Add new identifier to recognizer");
                            runningRecognizers.get(recognizer).add(identifier);
                        }
                        foundRecognizer=true;
                    } else if (runningRecognizers.get(recognizer).contains(identifier)) { // If the recognizer does not run on the grammar file, but it has the identifier of the client, remove it.
                        System.out.println("Remove identifier from recognizer");
                        runningRecognizers.get(recognizer).remove(identifier);
                    }
                }
                if (!foundRecognizer) {
                    System.out.println("Starting new recognizer");
                    Recognizer newRecognizer = new Recognizer(Base.grammars.get(input));
                    runningRecognizers.put(newRecognizer,new ArrayList<String>(){{add(identifier);}});
                    return;
                }
            }
            AnalyzeAudio(identifier,input,binaryInput);
        }

        private void AnalyzeAudio(String identifier, String input, byte[] binaryInput) {
            byte[] audioOnly = Arrays.copyOfRange(binaryInput,13,binaryInput.length);
            if (audioOnly.length<=44){ //Wav file header length == 44 bytes. Less bytes means input is no wav file
                SendResult("Please send a valid 16000khz, mono, wav filegit",os);
            }
            for(Recognizer recognizer : runningRecognizers.keySet()){
                if (runningRecognizers.get(recognizer).contains(identifier)){
                    SendResult(recognizer.recognize(audioOnly),os);
                    return;
                }
            }
            SendResult(Base.getTimedOut(),os);
        }

        private void SendResult(String text, OutputStream stream) {
            PrintWriter out = new PrintWriter(stream, true);
            out.print(text);
            out.flush();
            out.close();
        }
    }


    /**
     * Timed class. Checks for last time use of a recognition server. Closes if hasn't been used after 10 minutes
     */
    private class Cleaner extends TimerTask {
        @Override
        public void run() {
            ArrayList<Recognizer> toRemove = new ArrayList<>();
            for (Recognizer recognizer : runningRecognizers.keySet()){
                /* If the recognizer has no attached users, or has not been used in 10 minutes -> shut it down */
                if (runningRecognizers.get(recognizer).size()==0 || (System.currentTimeMillis()-recognizer.getLastUsed())>600000){
                    recognizer.Stop();
                    toRemove.add(recognizer);
                }
            }
            System.out.println("-----------");
            for (Recognizer recognizer : toRemove){
                runningRecognizers.remove(recognizer);
            }
            for (Recognizer recognizer : runningRecognizers.keySet()){
                System.out.println(recognizer.getGrammar().name+" "+recognizer.getRecognizer().getState());
                System.out.println(runningRecognizers.get(recognizer).size()+" users attached to recognizer");
            }
        }
    }
}
