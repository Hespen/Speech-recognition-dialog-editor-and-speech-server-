using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;

public abstract class SpeechRecognition : MonoBehaviour
{
    //Api code for the speech recognition service
    public static String ApiKey = null;

    //Language code used by speech recognition service
    protected static String LanguageCode = "en";

    //Reference to Scene Controller
    public SceneController SceneController;

    /// <summary>
    /// Possible speech recognition systems
    /// </summary>
    public enum SpeechRecognitionSystem
    {
        Wit,
        Google,
        Sphinx,
        None
    }

    /// <summary>
    /// Set the API Key
    /// </summary>
    /// <param name="key">API Key</param>
    public void SetKey(String key)
    {
        ApiKey = key;
    }

    /// <summary>
    /// Set values for the SpeechSystems
    /// </summary>
    /// <param name="key">API Key</param>
    /// <param name="sceneController">sceneController, main thread in our case</param>
    public void SetVariables(string key, SceneController sceneController)
    {
        SceneController = sceneController;
        ApiKey = key;
    }

    /// <summary>
    /// Recognize the audio
    /// </summary>
    /// <param name="fileName">File location of audio</param>
    public abstract void Recognize(string fileName);


    public class Job : ThreadedJob
    {
        public SceneController MainThread;
        private byte[] _audioDataInBytes;
        private readonly String _identificationKey;
        private string _recognizedText = "";
        private readonly SpeechRecognitionSystem _recognitionSystem;

        /// <summary>
        /// Constructor to pass the to be used values.
        /// </summary>
        /// <param name="mainThread">Main Thread, Class which contains the callback function</param>
        /// <param name="audioDataInBytes">Audio</param>
        /// <param name="identificationKey">API Key</param>
        /// <param name="recognitionSystem">The to be used Speech Recognition System</param>
        public Job(SceneController mainThread, byte[] audioDataInBytes, string identificationKey, SpeechRecognitionSystem recognitionSystem)
        {
            MainThread = mainThread;
            _audioDataInBytes = audioDataInBytes;
            _identificationKey = identificationKey;
            _recognitionSystem = recognitionSystem;
        }

        /// <summary>
        /// This function is called on a different thread! Do not call the main thread or Unity API from here
        /// </summary>
        protected override void ThreadFunction()
        {
            ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
            switch (_recognitionSystem)
            {
                case SpeechRecognitionSystem.Wit:
                    WitRecognition();
                    break;
                case SpeechRecognitionSystem.Google:
                    GoogleRecognition();
                    break;
                case SpeechRecognitionSystem.Sphinx:
                    SphinxRecognition();
                    break;
            }
        }

        /// <summary>
        /// This function is called when ThreadFunction() is done. This function is called on the main thread
        /// </summary>
        protected override void OnFinished()
        {
            if(File.Exists(Application.dataPath + "/audio.wav")) File.Delete(Application.dataPath + "/audio.wav");
            MainThread.RecognitionCallback(_recognizedText);
        }

        private void WitRecognition()
        {
            ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.wit.ai/speech?v=20161011");

            request.Method = "POST";
            request.Headers["Authorization"] = "Bearer " + _identificationKey;
            request.ContentType = "audio/wav";
            request.ContentLength = _audioDataInBytes.Length;
            request.GetRequestStream().Write(_audioDataInBytes, 0, _audioDataInBytes.Length);

            // Process the wit.ai response
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader response_stream = new StreamReader(response.GetResponseStream());
                    var responseText = response_stream.ReadToEnd();
                    _recognizedText = JsonUtility.FromJson<WitValues>(responseText)._text;
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        private void GoogleRecognition()
        {
            HttpWebRequest googleSpeechRequest = (HttpWebRequest)WebRequest.Create("https://speech.googleapis.com/v1beta1/speech:syncrecognize?key=" + ApiKey);
            googleSpeechRequest.ContentType = "application/json";
            googleSpeechRequest.Credentials = CredentialCache.DefaultCredentials;
            googleSpeechRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(googleSpeechRequest.GetRequestStream()))
            {
                String config = "{\"config\":{\"encoding\": \"FLAC\",\"sampleRate\": 16000,\"languageCode\": \"" +
                                LanguageCode + "\" },\"audio\":{\"content\":\"" +
                                Convert.ToBase64String(_audioDataInBytes, 0, _audioDataInBytes.Length) + "\"}}";
                streamWriter.Write(config);
                streamWriter.Flush();
                streamWriter.Close();
            }
            HttpWebResponse HWR_Response = (HttpWebResponse)googleSpeechRequest.GetResponse();

            if (HWR_Response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader SR_Response = new StreamReader(HWR_Response.GetResponseStream());
                var response = SR_Response.ReadToEnd();
                var deserialized = JsonUtility.FromJson<GoogleResponse>(response);
                if (deserialized.results != null)
                {
                    foreach (var googleResponse in deserialized.results)
                    {
                        foreach (var result in googleResponse.alternatives)
                        {
                            _recognizedText = result.transcript;
                        }
                    }
                }
                SR_Response.Close();
            }
        }

        private void SphinxRecognition()
        {
            try
            {
                TcpClient tcp = new TcpClient("localhost", 85);
                Stream stream = tcp.GetStream();
                byte[] start = getIdentifier();
                byte[] seperator = Encoding.UTF8.GetBytes(";");
                byte[] part1 = start.Concat(seperator).ToArray();
                byte[] arr = part1.Concat(_audioDataInBytes).ToArray();
                BinaryWriter sw = new BinaryWriter(stream);
                var length = BitConverter.GetBytes(arr.Length);
                Array.Reverse(length);
                sw.Write(length);
                sw.Flush();
                sw.Write(arr, 0, arr.Length);
                sw.Flush();
                StreamReader streamReader = new StreamReader(stream);
                _recognizedText = streamReader.ReadToEnd();
                tcp.Close();
            }
            catch (SocketException e)
            {
                Debug.Log(e);
            }
        }
        private byte[] getIdentifier()
        {
            String identifier = NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up).Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault();
            if (identifier.Length < 12)
            {
                identifier += "000000000000";
            }
            return Encoding.UTF8.GetBytes(identifier.ToCharArray(), 0, 12);
        }

        /// <summary>
        /// Use like this: ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
        /// Place this line of code before an HTTP request to overcome SSL errors
        /// </summary>
        private bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;
            // If there are errors in the certificate chain, look at each error to determine the cause.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                        bool chainIsValid = chain.Build((X509Certificate2)certificate);
                        if (!chainIsValid)
                        {
                            isOk = false;
                        }
                    }
                }
            }
            return isOk;
        }

    }
}
