using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

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
        public static string TranslateText(string input, string languagePair)
        {
            return TranslateText(input, languagePair, System.Text.Encoding.UTF7);
        }

        /// <summary>
        /// Translate Text using Google Translate The string you want translated 2 letter Language Pair, 
        /// delimited by "|". en|vi e.g. "en|da" language pair means to translate from English to Danish The encoding. 
        /// Translated to String
        /// </summary>
        /// <param name="input"></param>
        /// <param name="languagePair">en|vi or en|ja</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string TranslateText(string input, string languagePair, Encoding encoding)
        {
            string result = String.Empty;
            string url = String.Format("http://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair={1}", input, languagePair);
            string s = String.Empty;
            using (WebClient webClient = new WebClient())
            {
                webClient.Encoding = encoding;
                s = webClient.DownloadString(url);
            }
            int p = s.IndexOf("id=result_box");
            if (p > 0)
                s = s.Substring(p, s.Length - p);
            p = s.IndexOf("</span>");
            if (p > 0)
            {
                s = s.Substring(0, p);
                p = s.IndexOf(@"'"">");
                if (p > 0)
                    result = s.Substring(p + 3, s.Length - (p + 3));
            }
            return result;
        }
    }
}
