using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using LiteDB;
using System.Collections.Generic;
using System.IO;

namespace curl
{
    public class Rest
    {
        // [{ "model":"test", "action":"create", "data":[{"key":"value1", "key2":"tiếng việt"}] }]
        public static string create(Message m)
        {
            if (m.method != "POST")
                return JsonConvert.SerializeObject(new { ok = false, total = 0, count = 0, msg = "The method action [" + m.action + "] for model [" + m.model + "] must be POST" });

            string json = "{}";
            //string indexes = m.jobject.getValue("indexes");

            bool isOk = dbi.Create(m.model);

            if (isOk && !string.IsNullOrEmpty(m.input))
                json = insert(new Message() { action = "insert", model = m.model, input = m.input });
            else
                json = @"{""ok"":true,""total"":0,""output"":""Create database " + m.model + @" successfully.""}";

            return json;
        }

        // [{ "model":"test", "action":"insert", "data":[{"key":"value1", "key2":"tiếng việt"}] }]
        public static string insert(Message m)
        {
            if (m.method != "POST")
                return JsonConvert.SerializeObject(new { ok = false, total = 0, count = 0, msg = "The method action [" + m.action + "] for model [" + m.model + "] must be POST" });

            string json = "{}";

            var ips = JsonConvert.DeserializeObject<JObject[]>(m.input);
            if (ips.Length > 0)
            {
                IDB db = dbi.Get(m.model);
                if (db != null)
                {
                    string[] IDs = db.InsertBulk(ips.convertBsonDocument(true));
                    json = @"{""ok"":true,""total"":" + db.Count().ToString() + @",""count"":" + IDs.Length.ToString() + @",""items"":" + JsonConvert.SerializeObject(IDs) + @"}";
                }
            }

            return json;
        }

        // [{ "model":"test", "action":"update", "data":[{"key":"value1", "key2":"tiếng việt"}] }]
        public static string update(Message m)
        {
            if (m.method != "POST")
                return JsonConvert.SerializeObject(new { ok = false, total = 0, count = 0, msg = "The method action [" + m.action + "] for model [" + m.model + "] must be POST" });

            var ips = JsonConvert.DeserializeObject<JObject[]>(m.input);
            if (ips.Length > 0)
            {
                IDB db = dbi.Get(m.model);
                if (db != null)
                    return db.UpdateByIDs(ips.convertBsonDocument(true));
            }

            return "{}";
        }

        //http://127.0.0.1:8888?model=test&action=getbyid&_id=5744a604f1fa4b04a82cb94a
        public static string removebyid(Message m)
        {
            if (m.method != "POST")
                return JsonConvert.SerializeObject(new { ok = false, total = 0, count = 0, msg = "The method action [" + m.action + "] for model [" + m.model + "] must be POST" });

            string json = "{}";
            try
            {
                string[] ids = new string[] { };
                try
                {
                    ids = JsonConvert.DeserializeObject<string[]>(m.input);
                }
                catch (Exception ex)
                {
                    return JsonConvert.SerializeObject(new { ok = false, output = "The IDs is invalid format JSON" });
                }

                if (ids.Length > 0)
                {
                    IDB db = dbi.Get(m.model);
                    {
                        string id = string.Empty;
                        List<string> lsOk = new List<string>() { };
                        List<string> lsFail = new List<string>() { };
                        for (int i = 0; i < ids.Length; i++)
                        {
                            id = ids[i];
                            if (id.Length == 24)
                            {
                                bool result = db.RemoveById(id);
                                if (result)
                                    lsOk.Add(id);
                                else
                                    lsFail.Add(id);
                            }
                            else
                                lsFail.Add(id);
                            //json = JsonConvert.SerializeObject(new { ok = false, output = "The field _id should be 24 hex characters" });
                        }
                        json = @"{""ok"":true,""total"":" + db.Count().ToString() + @",""remove"":{""ok"":" +
                            JsonConvert.SerializeObject(lsOk) + @", ""fail"":" + JsonConvert.SerializeObject(lsFail) + @"}}";
                    }
                }
            }
            catch (Exception ex)
            {
                json = JsonConvert.SerializeObject(new { ok = false, output = ex.Message });
            }
            return json;
        }

        //http://127.0.0.1:8888/?model=test&action=count
        public static string count(Message m)
        {
            string json = @"{""ok"":false,""model"":""" + m.model + @""",""count"":-1}";
            IDB db = dbi.Get(m.model);
            if (db != null)
                json = @"{""ok"":true,""model"":""" + m.model + @""",""count"":" + db.Count().ToString() + @"}";
            return json;
        }

        //http://127.0.0.1:8888/?model=test&action=fetch
        //http://127.0.0.1:8888/?model=test&action=fetch&skip=0&limit=10
        public static string fetch(Message m)
        {
            var jobject = JsonConvert.DeserializeObject<JObject>(m.input);
            string json = @"{""ok"":false,""total"":-1,""count"":-1,""msg"":""Can not find model [" + m.model + @"]""}";
            string skip = jobject.getValue("skip");
            string limit = jobject.getValue("limit");

            long _skip = 0;
            long _limit = 0;

            long.TryParse(skip, out _skip);
            long.TryParse(limit, out _limit);

            if (_skip < 0) _skip = _LITEDB_CONST._SKIP;
            if (_limit <= 0) _limit = _LITEDB_CONST._LIMIT;

            IDB db = dbi.Get(m.model);
            if (db != null)
            {
                var result = db.Fetch(_skip, _limit).Select(x => x.toJson).ToArray();
                json = @"{""ok"":true,""total"":" + db.Count().ToString() + @",""count"":" + result.Length.ToString() + @",""items"":[" + string.Join(",", result) + @"]}";
            }
            return json;
        }

        //http://localhost:8888/?model=test&action=select&skip=0&limit=10&f.1=_id&o.1=eq&v.1=d984a8710c734d9bac3e98ef&or=2.3&f.2=___dc&o.2=eq&v.2=20180303&f.3=___dc&o.3=eq&v.3=20180304
        public static string select(Message m)
        {
            if (string.IsNullOrEmpty(m.query_string))
                return JsonConvert.SerializeObject(new { ok = false, total = 0, count = 0, msg = "The data of QueryString is NULL. It has format: model=test&action=select&skip=0&limit=10&_op.1=eq&_id.1=8499849689d044a7a5b0ffe9&_andor.1=and&_op.2=eq&___dc.2=20180303" });

            string json = JsonConvert.SerializeObject(new { ok = false, total = 0, count = 0, msg = "Can not find model [" + m.model + @"]" });

            IDB db = dbi.Get(m.model);
            if (db != null)
                json = db.Select(m.query_string);

            return json;
        }

        //http://127.0.0.1:8888?model=test&action=getbyid&_id=5744a604f1fa4b04a82cb94a
        public static string getbyid(Message m)
        {
            string json = "{}";
            try
            {
                var jobject = JsonConvert.DeserializeObject<JObject>(m.input);
                string id = jobject.getValue(_LITEDB_CONST.FIELD_ID);

                if (id.Length == 24)
                {
                    IDB db = dbi.Get(m.model);
                    {
                        var result = db.FindById(id);
                        json = @"{""ok"":true,""total"":" + db.Count().ToString() + @",""count"":" + (result == null ? "0" : "1") + @", ""items"":" + (result == null ? "null" : result.toJson) + @"}";
                    }
                }
                else
                    json = JsonConvert.SerializeObject(new { ok = false, output = "The field _id should be 24 hex characters" });
            }
            catch (Exception ex)
            {
                json = JsonConvert.SerializeObject(new { ok = false, output = ex.Message });
            }
            return json;
        }

        public static string import_file(Message m)
        {
            string json = "{}";
            return json;
        }



        //http://127.0.0.1:8888/?model=&action=file_browser&ext=html&path=C:\nginx\admin\module
        public static string file_browser(Message m)
        {
            var jobject = JsonConvert.DeserializeObject<JObject>(m.input); 
            string ext = jobject.getValue("ext");
            string path = jobject.getValue("path");
             
            string exts = "*.*";
            if (!string.IsNullOrEmpty(ext)) exts = string.Join("|", ext.Split('|').Select(x => "*." + x).ToArray());
            string[] files = new string[] { };
            dirs[] dirs = new dirs[] { };
            if (Directory.Exists(path))
            {
                files = Directory.GetFiles(path, exts).Select(x => Path.GetFileName(x)).ToArray();
                dirs = Directory.GetDirectories(path).Select(x => new dirs() { name = Path.GetFileName(x), count = Directory.GetFiles(x, exts).Length }).ToArray();
            }

            return @"{""ok"":false,""root"":" + JsonConvert.SerializeObject(path) + @",""dirs"":" + JsonConvert.SerializeObject(dirs) + @",""files"":" + JsonConvert.SerializeObject(files) + @",""count"":" + files.Length+"}";
        }
    }
    public class dirs
    {
        public string name { get; set; }
        public int count { get; set; }
    }
}
