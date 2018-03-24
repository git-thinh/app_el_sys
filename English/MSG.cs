using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Fleck2;
using Fleck2.Interfaces;
using Newtonsoft.Json;
using System.Speech.Synthesis;

namespace app_el_sys
{ 
    public class dir
    {
        public string name { get; set; }
        public int count { get; set; }
    }

    public class file
    {
        public string name { get; set; }
        public string title { get; set; }
    }

    public class MSG
    {
        public bool ok { set; get; } = false;

        public string id { set; get; }
        public string selector { set; get; }
        public string callback { set; get; }
        public string action { set; get; }

        public Dictionary<string, string> config { set; get; }
        public Dictionary<string, string> data { set; get; }

        public string result { set; get; }
    }

    public static class msgProcess
    {
        static string root = AppDomain.CurrentDomain.BaseDirectory;
        const string _output = "[###]";
        static Object lockMSG = new Object();
        static Object lockClient = new Object();
        private static Dictionary<string, IWebSocketConnection> dicClient = new Dictionary<string, IWebSocketConnection>() { };
        private static Dictionary<long, string> dicMSG = new Dictionary<long, string>() { };
        private static void DoMethod(object obj)
        {
            string json = "";
            int count = 0;
            //Tuple<Dictionary<string, IWebSocketConnection>, Dictionary<long, string>> dic = (Tuple<Dictionary<string, IWebSocketConnection>, Dictionary<long, string>>)obj;
            while (true)
            {
                lock (lockMSG)
                    count = dicMSG.Count;
                if (count > 0)
                {
                    string msg = "";
                    long id = 0;
                    lock (lockMSG)
                    {
                        var it = dicMSG.ElementAt(0);
                        id = it.Key;
                        msg = it.Value;
                        dicMSG.Remove(id);
                    }

                    if (!string.IsNullOrEmpty(msg) && msg.Length > 36)
                    {
                        string socket_id = msg.Substring(0, 36);
                        msg = msg.Substring(36, msg.Length - 36);

                        IWebSocketConnection socket;
                        lock (lockClient)
                            socket = dicClient[socket_id];

                        MSG m = null;
                        bool ok = true;
                        try
                        {
                            m = JsonConvert.DeserializeObject<MSG>(msg);
                        }
                        catch (Exception ex)
                        {
                            ok = false;
                            json = @"{""ok"":false,""result"":""" + ex.Message + @"""," + msg.Substring(1);
                            socket.Send(json);
                            continue;
                        }

                        if (ok && m != null)
                        {
                            m.result = _output;
                            Execute(socket, m);
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }


        public static void Execute(IWebSocketConnection socket, MSG m)
        {
            m.ok = false;
            string json = "", result = "";
            switch (m.action)
            {
                case "LOAD_SUB_DIR_FILE":
                    #region
                    string _ext = "", _folder = "", _root = "";
                    if (m.data.ContainsKey("ext") && m.data.ContainsKey("folder") && m.data.ContainsKey("root"))
                    {
                        _ext = m.data["ext"];
                        _folder = m.data["folder"]; 
                        _root = m.data["root"];

                        if (string.IsNullOrEmpty(_root))
                            _root = root;

                        string path = _root; //Path.Combine(_root, _folder);
                        if (Directory.Exists(path))
                        {
                            string exts = "*.*";
                            if (!string.IsNullOrEmpty(_ext)) exts = string.Join("|", _ext.Split('|').Select(x => "*." + x).ToArray());
                            file[] files = new file[] { };
                            dir[] dirs = new dir[] { };
                            files = Directory.GetFiles(path, exts).Select(x => new file() { name = Path.GetFileName(x), title = File.ReadAllLines(x)[0] }).ToArray();
                            dirs = Directory.GetDirectories(path).Select(x => new dir() { name = Path.GetFileName(x), count = Directory.GetFiles(x, exts).Length }).ToArray();
                            result = @"{""root"":" + JsonConvert.SerializeObject(path) + @",""dirs"":" + JsonConvert.SerializeObject(dirs) + @",""files"":" + 
                                JsonConvert.SerializeObject(files) + @",""count"":" + files.Length + "}";
                            m.ok = true;
                        }
                        else
                            m.result = "Cannot find path: " + path;
                    }
                    else
                    {
                        m.result = "Field [data] must contain paramenter: ext, folder, path be not NULL.";
                    }
                    break;
                #endregion
                default:
                    m.result = "Cannot find action.";
                    break;
            }
            json = JsonConvert.SerializeObject(m);
            if (m.ok)
                json = json.Replace(@"""" + _output + @"""", result);
            socket.Send(json);
        }//end function

        public static void Init()
        {
            Thread thread = new Thread(new ParameterizedThreadStart(DoMethod));
            thread.Start(new Tuple<Dictionary<string, IWebSocketConnection>, Dictionary<long, string>>(dicClient, dicMSG));
        }

        public static void Join(IWebSocketConnection socket)
        {
            lock (lockClient)
                dicClient.Add(socket.ConnectionInfo.Id.ToString(), socket);
        }

        public static long Push(string socket_id, string msg)
        {
            long id = long.Parse(DateTime.Now.ToString("yyyymmddHHmmssfff"));
            lock (lockMSG)
                dicMSG.Add(id, socket_id + msg);
            return id;
        }

    }
}
