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
        static AutoResetEvent _autoEvent = new AutoResetEvent(false);
        static IWebSocketConnection _socketCurrent = null;
        static SpeechSynthesizer _speaker = new SpeechSynthesizer();
        static HttpServer _serverHTTP = null;
        static int _portHTTP = 0;

        static EL_SPEAK_MSG _data = null;
        static SynthesizerState _state = SynthesizerState.Ready;

        static void speakExecute()
        {
            while (true)
            {
                _autoEvent.WaitOne();
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
        }

        static void processMessage(string s)
        {
            if (_state == SynthesizerState.Ready)
            {
                switch (s[0])
                {
                    case '@': // TRANSLATE
                        s = s.Substring(1).Trim();
                        string temp = Translator.TranslateText(s);
                        _socketCurrent.Send("=" + temp);
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
                            case EL._SOCKET_CMD_STOP:
                                _speaker.SpeakAsyncCancel(null);
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
            new Thread(new ThreadStart(speakExecute)).Start();

            // Configure the audio output. 
            _speaker.SetOutputToDefaultAudioDevice();
            _speaker.Rate = EL._SPEAK_RATE_DEFAULT_WORD;

            _speaker.StateChanged += new EventHandler<StateChangedEventArgs>(_speaker_StateChanged);
            _speaker.SpeakProgress += new EventHandler<SpeakProgressEventArgs>(_speaker_SpeakProgress);
            _speaker.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(_speaker_SpeakCompleted);

            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            _portHTTP = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            _portHTTP = 8888;
            string uri = string.Format("http://127.0.0.1:{0}/", _portHTTP);
            Console.Title = _portHTTP.ToString();


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

