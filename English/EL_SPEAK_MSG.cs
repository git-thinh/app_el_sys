using System;
using System.Collections.Generic;
using System.Text;

namespace app_el_sys
{
    public class EL_SPEAK_MSG
    {
        #region

        public long ID { private set; get; }
        public int Repeat { set; get; }
        public bool Translate { set; get; }

        public EL_SPEAK_TYPE Type { set; get; }
        public string Text { set; get; }

        public int WordTimeout { set; get; }
        public int ClauseTimeout { set; get; }
        public int SentenceTimeout { set; get; }

        public int RepeatCounter = 0;
        public string TranslateResult = string.Empty;

        #endregion

        /// <summary>
        /// text^Repeat=true|false^Translate=true|false^EL_SPEAK_TYPE^WordTimeout^ClauseTimeout^SentenceTimeout^...
        ///   0     1                     2                    3           4             5           6               
        /// </summary>
        /// <param name="text"></param>
        public EL_SPEAK_MSG(string text)
        {
            Type = EL_SPEAK_TYPE.SPEAK_WORD;

            string[] a = text.Split('^');
            this.Text = a[0].Trim();
            if (a.Length > 1) this.Repeat = TryParser(a[1], 1);
            if (a.Length > 2) this.Translate = a[2] == "true" ? true : false;
            if (a.Length > 3)
                switch (a[3].ToUpper().Trim())
                {
                    case "CLAUSE":
                        this.Type = EL_SPEAK_TYPE.SPEAK_CLAUSE;
                        break;
                    case "SENTENCE":
                        this.Type = EL_SPEAK_TYPE.SPEAK_SENTENCE;
                        break;
                    case "PARAGRAPH":
                        this.Type = EL_SPEAK_TYPE.SPEAK_PARAGRAPH;
                        break;
                }

            if (a.Length > 4) this.WordTimeout = TryParser(a[4], EL._TIMEOUT_SPEAK_WORD);
            if (a.Length > 5) this.ClauseTimeout = TryParser(a[5], EL._TIMEOUT_SPEAK_CLAUSE);
            if (a.Length > 6) this.SentenceTimeout = TryParser(a[6], EL._TIMEOUT_SPEAK_SENTENCE);

            ID = long.Parse(DateTime.Now.ToString("yyMMddHHmmssfff"));
        }

        private int TryParser(string s, int _valueDefault = 0)
        {
            int k = 0;
            int.TryParse(s, out k);
            if (k <= 0) k = _valueDefault;
            return k;
        }

        public bool RepeatComplete {
            get {
                if (this.Repeat == this.RepeatCounter)
                {
                    this.RepeatCounter = 0;
                    return true;
                }
                return false;
            }
        }
    }
}
