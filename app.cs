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
    [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"), PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class app
    {
        static Dictionary<string, string> dicTranslate = new Dictionary<string, string>() { };
        static AutoResetEvent _autoEvent = new AutoResetEvent(false);
        static IWebSocketConnection _socketCurrent = null;
        static SpeechSynthesizer _speaker = new SpeechSynthesizer();
        static HttpServer _serverHTTP = null;
        static int _portHTTP = 0;

        static EL_SPEAK_MSG _data = null;
        static SynthesizerState _state = SynthesizerState.Ready;
        static bool _statePause = false;

        static void translateExecute(string text)
        {
            text = text.Trim().ToLower();
            string result = string.Empty;
            if (dicTranslate.ContainsKey(text))
                result = dicTranslate[text];
            else
            {
                string temp = Translator.TranslateText(text);
                if (!string.IsNullOrEmpty(temp))
                {
                    dicTranslate.Add(text, temp);
                    result = temp;
                }
            }
            _socketCurrent.Send("=" + result);
        }

        static void speakExecute()
        {
            while (true)
            {
                _autoEvent.WaitOne();
                if (_statePause) continue;
                if (_data == null) continue;
                if (_data.RepeatComplete) continue;

                if (_data.TextCounter >= _data.Text.Length)
                {
                    _data.TextCounter = 0;
                    _data.RepeatCounter++;
                    if (_data.RepeatComplete)
                        continue;
                }
                _state = SynthesizerState.Speaking;
                switch (_data.Type)
                {
                    case EL_SPEAK_TYPE.SPEAK_WORD:
                        _speaker.Speak(_data.Text[_data.TextCounter]);
                        break;
                    case EL_SPEAK_TYPE.SPEAK_CLAUSE:
                        break;
                    case EL_SPEAK_TYPE.SPEAK_KEYWORD:
                        break;
                    case EL_SPEAK_TYPE.SPEAK_SENTENCE:
                        break;
                    case EL_SPEAK_TYPE.SPEAK_PARAGRAPH:
                        break;
                }
                _data.TextCounter++;
            }
        }

        private static void _speaker_StateChanged(object sender, StateChangedEventArgs e)
        {
            if (e.State == SynthesizerState.Ready)
            {
                _state = SynthesizerState.Ready;
                _autoEvent.Set();
            }
        }

        private static void _speaker_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            _socketCurrent.Send(e.Text);
        }

        private static void _speaker_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            // The SpeakAsync operation was cancelled
        }

        static void processMessage(string s)
        {
            string temp = string.Empty, file = string.Empty;
            if (s.IndexOf('|') > 0)
            {
                string[] a = s.Split('|');
                string script_key = a[0].Trim(),
                    text = a[1].Trim();
                if (EL.dicScript.ContainsKey(script_key)) {
                    SCRIPT[] scrs = EL.dicScript[script_key];

                }
            }
            else
            {
                switch (s)
                {
                    #region [ SCRIPT_LOAD ]
                    case EL._SCRIPT_CMD_LOAD:
                        if (File.Exists("script.json"))
                        {
                            try
                            {
                                file = File.ReadAllText("script.json");
                                EL.dicScript = JsonConvert.DeserializeObject<Dictionary<string, SCRIPT[]>>(file);
                                _socketCurrent.Send(s + Environment.NewLine + JsonConvert.SerializeObject(EL.dicScript, Formatting.Indented));
                            }
                            catch { }
                        }
                        break;
                    #endregion

                    #region [ PAUSE - REPLAY - STOP ]
                    case EL._SOCKET_CMD_REPLAY:
                        _statePause = false;
                        _autoEvent.Set();
                        break;
                    case EL._SOCKET_CMD_PAUSE:
                        if (_statePause)
                        {
                            _statePause = false;
                            _speaker.Resume();
                        }
                        else
                        {
                            _statePause = true;
                            _speaker.Pause();
                        }
                        break;
                    case EL._SOCKET_CMD_STOP:
                        _speaker.SpeakAsyncCancelAll();
                        _statePause = true;
                        break;
                    #endregion

                    #region [ SPEAK ]
                    default:
                        if (_state == SynthesizerState.Ready)
                        {
                            switch (s[0])
                            {
                                case '!': // SHOW HTTP PORT 
                                    _socketCurrent.Send("!" + _portHTTP.ToString());
                                    break;
                                case '@': // TRANSLATE
                                    s = s.ToLower();
                                    switch (s)
                                    {
                                        case "@write":
                                            temp = string.Join(Environment.NewLine,
                                                dicTranslate.Select(x => string.Format("{0}:{1}", x.Key, x.Value)).ToArray());
                                            file = string.Format("en-vi.{0}.txt", DateTime.Now.ToString("yyMMdd"));
                                            File.WriteAllText(file, temp);
                                            _socketCurrent.Send(string.Format("@write{0}", file));
                                            break;
                                        case "@getall":
                                            temp = string.Join(Environment.NewLine,
                                                dicTranslate.Select(x => string.Format("{0}:{1}", x.Key, x.Value)).ToArray());
                                            _socketCurrent.Send(string.Format("@getall{0}{1}", Environment.NewLine, temp));
                                            break;
                                        default:
                                            translateExecute(s.Substring(1));
                                            break;
                                    }
                                    break;
                                case '%': // set speech Rate from -10 to 10 
                                    int rate = 10;
                                    int.TryParse(s.Substring(1), out rate);
                                    if (rate > -10 && rate < 11)
                                    {
                                        _speaker.Rate = rate;
                                        _socketCurrent.Send(s);
                                    }
                                    break;
                                default:
                                    switch (s)
                                    {
                                        case EL._SOCKET_CMD_REPLAY:
                                            _autoEvent.Set();
                                            break;
                                        default:
                                            _data = new EL_SPEAK_MSG(s);
                                            _autoEvent.Set();
                                            _socketCurrent.Send(string.Format("{0}:{1}", EL._STATUS_SPEAK_OK, _data.ID));
                                            break;
                                    }
                                    break;
                            }
                        }
                        else
                            _socketCurrent.Send(EL._STATUS_SPEAK_FAIL);
                        break;
                        #endregion
                }
            }
        }

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
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            _portHTTP = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            Console.Title = _portHTTP.ToString();

            //http://127.0.0.1:8888/http_-_genk.vn/ai-nay-da-danh-bai-20-luat-su-hang-dau-nuoc-my-trong-linh-vuc-ma-ho-gioi-nhat-20180227012111793.chn?_format=text
            //HttpServer _serverHTTP = null;
            _serverHTTP = new HttpProxyServer();
            _serverHTTP.Start(string.Format("http://127.0.0.1:{0}/", _portHTTP));
            Console.Title = _portHTTP.ToString();
            //_serverHTTP.Stop();

            new Thread(new ThreadStart(speakExecute)).Start();

            // Configure the audio output. 
            _speaker.SetOutputToDefaultAudioDevice();
            _speaker.Rate = EL._SPEAK_RATE_DEFAULT_WORD;

            _speaker.StateChanged += new EventHandler<StateChangedEventArgs>(_speaker_StateChanged);
            _speaker.SpeakProgress += new EventHandler<SpeakProgressEventArgs>(_speaker_SpeakProgress);
            _speaker.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(_speaker_SpeakCompleted);


            //FleckLog.Level = LogLevel.Debug;
            FleckLog.Level = LogLevel.Info;
            var allSockets = new List<IWebSocketConnection>();
            var server = new WebSocketServer("ws://localhost:8889");

            server.Start(socket =>
            {
                socket.OnMessage = message => processMessage(message);
                socket.OnOpen = () => _socketCurrent = socket;
                socket.OnClose = () => _socketCurrent = null;
            });
            Console.ReadLine();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            app.RUN();
        }
    }
}

