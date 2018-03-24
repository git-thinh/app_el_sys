using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace app_el_sys
{
    public class EL_SPEAK_MSG
    {
        #region

        public long ID { private set; get; }
        public int Repeat { set; get; } = 1; 

        public EL_SPEAK_TYPE Type { set; get; }
        public string[] Text { set; get; }

        public int WordTimeout { set; get; }
        public int ClauseTimeout { set; get; }
        public int SentenceTimeout { set; get; }

        public int TextCounter { set; get; } = 0;
        public int RepeatCounter { set; get; } = 0;

        #endregion

        /// <summary>
        /// text1|text2^Repeat=1,2..^EL_SPEAK_TYPE^WordTimeout^ClauseTimeout^SentenceTimeout^...
        ///   [0,1..]       0                1            2           3             4                
        /// </summary>
        /// <param name="text"></param>
        public EL_SPEAK_MSG(string text)
        {
            Type = EL_SPEAK_TYPE.SPEAK_WORD;
            string config = string.Empty;
            int p = text.IndexOf('^');
            if (p != -1)
            {
                config = text.Substring(p + 1, text.Length - (p + 1));
                text = text.Substring(0, p);
            }

            this.Text = text.Split(new char[] { '|' });
            this.Repeat = 1;
            if (!string.IsNullOrEmpty(config))
            {
                string[] cf = config.Split('^');
                if (cf.Length > 0) this.Repeat = TryParser(cf[0], 1);
                if (cf.Length > 1)
                    switch (cf[1].ToUpper().Trim())
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

                if (cf.Length > 2) this.WordTimeout = TryParser(cf[2], EL._TIMEOUT_SPEAK_WORD);
                if (cf.Length > 3) this.ClauseTimeout = TryParser(cf[3], EL._TIMEOUT_SPEAK_CLAUSE);
                if (cf.Length > 4) this.SentenceTimeout = TryParser(cf[4], EL._TIMEOUT_SPEAK_SENTENCE);
            }

            ID = long.Parse(DateTime.Now.ToString("yyMMddHHmmssfff"));
        }

        private int TryParser(string s, int _valueDefault = 0)
        {
            int k = 0;
            int.TryParse(s, out k);
            if (k <= 0) k = _valueDefault;
            return k;
        }

        public bool RepeatComplete
        {
            get
            {
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
