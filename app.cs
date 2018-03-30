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
        public class _action_key
        {
            public const string FILE_LOAD = "FILE_LOAD";
            public const string TREE_NODE = "TREE_NODE";
            public const string USER_LOGIN = "USER_LOGIN";
            public const string GRAM_ALL_KEY = "GRAM_ALL_KEY";
            public const string GRAM_ALL_WORD = "GRAM_ALL_WORD";
            public const string GRAM_DETAIL_BY_KEY = "GRAM_DETAIL_BY_KEY";
        };

        const char _split_key = '¦';
        const char _split_data = '‖';
        static string _path_root = AppDomain.CurrentDomain.BaseDirectory;

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
            if (s.IndexOf(_split_key) > 0)
            {
                string[] a = s.Split(_split_key);
                string action = a[0].Trim(), result = string.Empty;
                string[] para = a.Where((x, k) => k != 0).ToArray();
                switch (action)
                {
                    case _action_key.TREE_NODE:
                        if (a.Length > 2)
                        {
                            string _path = a[1].Trim(), _folder = a[2].Trim();
                            if (_path == string.Empty) _path = _path_root;
                            string _pf = Path.Combine(_path.Replace('/', '\\'), _folder);
                            if (Directory.Exists(_pf))
                            {
                                var dirs = Directory.GetDirectories(_pf).Select(x => new
                                {
                                    name = Path.GetFileName(x),
                                    count = Directory.GetFiles(x, "*.txt").Length + Directory.GetDirectories(x).Length
                                }).ToArray();
                                var files = Directory.GetFiles(_pf, "*.txt").Select(x => new
                                {
                                    name = Path.GetFileName(x),
                                    type = string.Empty,
                                    title = Regex.Replace(Regex.Replace(File.ReadAllLines(x)[0], "<.*?>", " "), "[ ]{2,}", " ")
                                }).ToArray();
                                result = s + _split_data + JsonConvert.SerializeObject(new
                                {
                                    path = _pf.Replace('\\', '/'),
                                    dirs = dirs,
                                    files = files
                                });
                                _socketCurrent.Send(result);
                            }
                        }
                        break;
                    case _action_key.USER_LOGIN:
                        break;
                    case _action_key.FILE_LOAD:
                        string fi = Path.Combine(_path_root, para[0]), htm = string.Empty, text = string.Empty, word = string.Empty;
                        string[] line = new string[] { };
                        if (File.Exists(fi))
                        {
                            text = File.ReadAllText(fi);
                            line = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                            string tmp = Regex.Replace(text, "[^0-9a-zA-Z]+", " ").ToLower();
                            var ws = tmp.Split(' ').Where(x => x.Length > 0)
                                .GroupBy(x => x)
                                .OrderByDescending(x => x.Count())
                                .Select(x => new { w = x.Key, k = x.Count() })
                                .ToArray();
                            word = JsonConvert.SerializeObject(ws);
                            htm = app.renderFile(line);
                        }
                        _socketCurrent.Send(string.Format("{0}.{1}.{2}{3}{4}", action, para[0], "text", _split_data, text));
                        _socketCurrent.Send(string.Format("{0}.{1}.{2}{3}{4}", action, para[0], "html", _split_data, htm));
                        _socketCurrent.Send(string.Format("{0}.{1}.{2}{3}{4}", action, para[0], "word", _split_data, word));
                        break;
                    default:
                        if (EL.dicScript.ContainsKey(action))
                        {
                            SCRIPT[] scrs = EL.dicScript[action];
                        }
                        break;
                }
            }
            else
            {
                switch (s)
                {
                    #region [ GRAMMAR_LOAD ]
                    case EL._GRAMMAR_CMD_LOAD:
                        EL.listGrammar = GRAMMAR.parserFromFile("grammar.txt");
                        _socketCurrent.Send(s + ":" + Environment.NewLine + JsonConvert.SerializeObject(EL.listGrammar, Formatting.Indented));
                        break;
                    #endregion

                    #region [ SCRIPT_LOAD ]
                    case EL._SCRIPT_CMD_LOAD:
                        if (File.Exists("script.json"))
                        {
                            try
                            {
                                file = File.ReadAllText("script.json");
                                EL.dicScript = JsonConvert.DeserializeObject<Dictionary<string, SCRIPT[]>>(file);
                                _socketCurrent.Send(s + ":" + Environment.NewLine + JsonConvert.SerializeObject(EL.dicScript, Formatting.Indented));
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

        public static string renderFile(string[] lines)
        {
            if (lines.Length == 0) return string.Empty;

            string[] a = lines.Where(x => x.Trim() != "").ToArray();
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
            //File.WriteAllText("demo-output.txt", htm);
            return htm;
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
                socket.OnOpen = () => { _socketCurrent = socket; socket.Send(string.Format("HTTP_PORT:{0}", _portHTTP)); };
                socket.OnClose = () => _socketCurrent = null;
            });
            Console.ReadLine();
        }
    }

    class Program
    {
        //static string _path_root = AppDomain.CurrentDomain.BaseDirectory;
        static void Main(string[] args)
        {
            app.RUN();
        }
    }
}

