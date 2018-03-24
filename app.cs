using System;
using System.Security.Permissions;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using Fleck2;
using Fleck2.Interfaces;
using Newtonsoft.Json;
using System.Speech.Synthesis;
using System.Text.RegularExpressions; 

namespace app_el_sys
{
    class Program
    {
        static void Main(string[] args)
        {
            app.RUN();
            //app.test();
        }
    }


    [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"), PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class app
    {
        static IWebSocketConnection _socketCurrent = null;
        static SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();
        static HttpServer _serverHTTP = null;
        static int _portHTTP = 0;

        static string[] _speakWords = new string[] { };
        static Int16 _speakCounter = 0;
        static SynthesizerState _speakState = SynthesizerState.Ready;

        static app()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (se, ev) =>
            {
                Assembly asm = null;
                string comName = ev.Name.Split(',')[0];
                string resourceName = @"DLL\" + comName + ".dll";
                var assembly = Assembly.GetExecutingAssembly();
                resourceName = typeof(app).Namespace + "." + resourceName.Replace(" ", "_").Replace("\\", ".").Replace("/", ".");
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        byte[] buffer = new byte[stream.Length];
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int read;
                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                                ms.Write(buffer, 0, read);
                            buffer = ms.ToArray();
                        }
                        asm = Assembly.Load(buffer);
                    }
                }
                return asm;
            };
        }

        public static void RUN()
        {
            // Configure the audio output. 
            _speechSynthesizer.SetOutputToDefaultAudioDevice();

            _speechSynthesizer.StateChanged += (se, ev) => { }; 
            _speechSynthesizer.SpeakProgress += (se, ev) => { }; 
            _speechSynthesizer.SpeakCompleted += (se, ev) => { };




        }













        public static void test()
        {
            string[] a = File.ReadAllLines("demo.txt").Where(x => x.Trim() != "").ToArray();
            Paragraph p;
            List<Paragraph> ls = new List<Paragraph>() { new Paragraph(0, a[0]) };
            string si = string.Empty, _code = string.Empty, _ul = string.Empty;
            bool _isCode = false, _isLI = false;
            int _id = 0;
            for (int i = 1; i < a.Length; i++)
            {
                si = a[i];
                if (si == EL._TAG_CODE_CHAR_BEGIN || _isCode)
                {
                    #region [ PRE - CODE ]
                    if (si != EL._TAG_CODE_CHAR_BEGIN) _id = i;

                    _isCode = true;
                    if (si != EL._TAG_CODE_CHAR_BEGIN && si != EL._TAG_CODE_CHAR_END)
                        _code += Environment.NewLine + si;

                    if (i == a.Length - 1 || si == EL._TAG_CODE_CHAR_END)
                    {
                        _isCode = false;
                        p = new Paragraph() { id = _id, text = _code, type = SENTENCE.CODE, html = string.Format("<{0}>{1}</{0}>", EL.TAG_CODE, _code) };
                        ls.Add(p);
                    }
                    #endregion
                }
                else
                {
                    switch (si[0])
                    {
                        case '*':
                            #region [ HEADING ]
                            si = si.Substring(1).Trim();
                            p = new Paragraph() { id = i, type = SENTENCE.HEADING, text = si, html = string.Format("<{0}>{1}</{0}>", EL.TAG_HEADING, si.generalHtmlWords()) };
                            ls.Add(p);
                            break;
                        #endregion
                        case '#':
                            #region [ UL_LI ]
                            si = si.Substring(1).Trim();
                            if (_isLI == false)
                            {
                                _id = i;
                                _isLI = true;
                                _ul = "<ul><li>" + si.generalHtmlWords() + "</li>";
                            }
                            else
                                _ul += "<li>" + si.generalHtmlWords() + "</li>";

                            if (i == a.Length - 1)
                            {
                                _ul += "</ul>";
                                _isLI = false;
                                p = new Paragraph() { id = _id, text = _ul, type = SENTENCE.UL_LI, html = _ul };
                                ls.Add(p);
                            }
                            break;
                        #endregion
                        default:
                            #region [ UL_LI ]
                            if (_isLI)
                            {
                                _ul += "</ul>";
                                _isLI = false;
                                p = new Paragraph() { id = _id, text = _ul, type = SENTENCE.UL_LI, html = _ul };
                                ls.Add(p);
                            }
                            #endregion

                            p = new Paragraph(i, si);
                            ls.Add(p);
                            break;
                    }
                }
            }
            string htm = string.Join(Environment.NewLine, ls.Select(x => x.html).ToArray());
            htm = string.Format("<{0}>{1}</{0}>", EL.TAG_ARTICLE, htm);

            //string ss = Translator.TranslateText("hello", "en|vi");
            //Console.WriteLine("{0} = {1}", "hello", ss);
            //Console.ReadLine();
            File.WriteAllText("demo-output.txt", htm);
        }



        #region

        // Initialize a new instance of the SpeechSynthesizer.
        private static void DoMethod(object obj)
        {
            IWebSocketConnection socket = (IWebSocketConnection)obj;
            while (true)
            {
                try
                {
                    socket.Send(JsonConvert.SerializeObject(new { action = "NOTI_TEST", msg = Guid.NewGuid().ToString() }));
                }
                catch
                {
                    break;
                    Console.WriteLine("CLOSE .....");
                }
                Thread.Sleep(5000);
            }
        }
        // Write to the console when the SpeakAsync operation has been cancelled.
        static void synth_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            Console.WriteLine("\nThe SpeakAsync operation was cancelled!!");
            _socketCurrent.Send(string.Format("!{0}", e.Error.Message));
            _speakState = SynthesizerState.Ready;

        }

        // When it changes, write the state of the SpeechSynthesizer to the console.
        static void synth_StateChanged(object sender, StateChangedEventArgs e)
        {
            _speakState = e.State;
            //Console.WriteLine("\nSynthesizer State: {0}    Previous State: {1}\n", e.State, e.PreviousState);
            //_socketCurrent.Send(string.Format("@{0}", e.State));
            if (e.State == SynthesizerState.Ready)
            {
                _speakCounter++;
                if (_speakCounter == _speakWords.Length - 1) _speakCounter = 0;
                _speechSynthesizer.Speak(_speakWords[_speakCounter]);
            }
        }

        // Write the text being spoken by the SpeechSynthesizer to the console.
        static void synth_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            _socketCurrent.Send(string.Format("#{0}", e.Text));
            //Console.WriteLine(e.Text);
        }

        public static void Start()
        {
            #region
            //new Thread(()=> {
            //    int timeOut = 100;
            //    while (true) {
            //        if (_speakState == SynthesizerState.Ready && _speakWords.Length != 0)
            //        { 
            //            speechSynthesizer.Speak(_speakWords[_speakCounter]);
            //            _speakCounter++;
            //            if (_speakCounter == _speakWords.Length) {
            //                _speakCounter = 0;
            //                timeOut = 3000;
            //            }
            //        }
            //        Thread.Sleep(timeOut);
            //    }
            //}).Start();
            #endregion

            #region
            // Configure the audio output. 
            _speechSynthesizer.SetOutputToDefaultAudioDevice();
            // Subscribe to the StateChanged event.
            _speechSynthesizer.StateChanged += new EventHandler<StateChangedEventArgs>(synth_StateChanged);
            // Subscribe to the SpeakProgress event.
            _speechSynthesizer.SpeakProgress += new EventHandler<SpeakProgressEventArgs>(synth_SpeakProgress);
            // Subscribe to the SpeakCompleted event.
            _speechSynthesizer.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(synth_SpeakCompleted);

            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            _portHTTP = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            _portHTTP = 8888;
            string uri = string.Format("http://127.0.0.1:{0}/", _portHTTP);
            Console.Title = _portHTTP.ToString();
             
            msgProcess.Init();
            //FleckLog.Level = LogLevel.Debug;
            FleckLog.Level = LogLevel.Info;
            var allSockets = new List<IWebSocketConnection>();
            var server = new WebSocketServer("ws://localhost:8889");
            #endregion

            server.Start(socket =>
            {
                socket.OnMessage = message =>
                {
                    Console.WriteLine(message);

                    if (message.Length > 0)
                    {
                        switch (message[0])
                        {
                            case '#':
                                try
                                {
                                    message = message.Substring(1).Trim();
                                    _speakWords = message.ToLower().Split(' ').Where(x => !EL._WORD_SKIP_WHEN_READING.Any(w => w == x)).ToArray();
                                    _speechSynthesizer.Speak(_speakWords[_speakCounter]);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                                break;
                            case '!':
                                // Cancel the SpeakAsync operation and wait one second.
                                _speechSynthesizer.SpeakAsyncCancelAll();
                                break;
                            default:
                                long id = msgProcess.Push(socket.ConnectionInfo.Id.ToString(), message);
                                //socket.Send(id.ToString());
                                break;
                        }
                    }
                };
                socket.OnOpen = () =>
                {
                    _socketCurrent = socket;
                    //Thread thread = new Thread(new ParameterizedThreadStart(DoMethod));
                    //thread.Start(socket); 
                    //msgProcess.Join(socket);
                    //socket.Send("ID=" + socket.ConnectionInfo.Id.ToString());
                };
                socket.OnClose = () =>
                {
                    //Console.WriteLine("Close!");
                    //allSockets.Remove(socket);
                };
            });

            //Process.Start("client.html");

            //http://127.0.0.1:8888/http_-_genk.vn/ai-nay-da-danh-bai-20-luat-su-hang-dau-nuoc-my-trong-linh-vuc-ma-ho-gioi-nhat-20180227012111793.chn?_format=text
            //HttpServer Server = null;
            _serverHTTP = new HttpProxyServer();
            _serverHTTP.Start(uri);
            //Server.Stop();
            //Console.WriteLine(uri);
            ////Console.ReadLine();


            var input = Console.ReadLine();
            while (input != "exit")
            {
                if (input == "cls")
                    Console.Clear();
                else
                {
                    //foreach (var socket in allSockets)
                    //{
                    //    socket.Send(input);
                    //}

                    switch (input[0])
                    {
                        case '!':
                            // Cancel the SpeakAsync operation and wait one second.
                            _speechSynthesizer.SpeakAsyncCancelAll();
                            break;
                        default:
                            try
                            {
                                _speechSynthesizer.Speak(input);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            break;
                    }
                }
                input = Console.ReadLine();
            }






        }

        public static void Stop()
        {
            _serverHTTP.Stop();
        }
        #endregion
    }

}

