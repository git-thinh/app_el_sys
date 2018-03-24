using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;

namespace app_el_sys
{
    public static class Translator
    {
        /// <summary>
        /// languagePair are en|vi or en|ja
        /// </summary>
        /// <param name="input"></param>
        /// <param name="languagePair"></param>
        /// <returns></returns>
        public static string TranslateText(string input)
        {
            return TranslateText(input, System.Text.Encoding.UTF7);
        }

        public static string TranslateText(string input, Encoding encoding)
        {
            string result = String.Empty;

            string temp = HttpUtility.UrlEncode(input.Replace(" ", "-----"));
            temp = temp.Replace("-----", "%20");
            string url = String.Format("http://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair={1}", temp, "en|vi");

            string s = String.Empty;
            using (WebClient webClient = new WebClient())
            {
                webClient.Encoding = encoding;
                s = webClient.DownloadString(url);
            }
            string ht = HttpUtility.HtmlDecode(s);

            int p = s.IndexOf("id=result_box");
            if (p > 0)
                s = s.Substring(p, s.Length - p);
            p = s.IndexOf("</span>");
            if (p > 0)
            {
                s = s.Substring(0, p);
                p = s.IndexOf(@"'"">");
                if (p > 0)
                {
                    result = s.Substring(p + 3, s.Length - (p + 3));
                    result = HttpUtility.HtmlDecode(result);
                }
            }
            return result;
        }
    }
}
