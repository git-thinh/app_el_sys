using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace app_el_sys
{
    public class Paragraph
    {
        public int id { set; get; }
        public SENTENCE type { set; get; }
        public string text { set; get; }
        public string html { set; get; }

        public Paragraph() { }

        public Paragraph(int _id, string _s)
        {
            if (!string.IsNullOrEmpty(_s) && _s.Trim().Length > 0)
            {
                text = _s;
                if (_id == 0)
                {
                    type = SENTENCE.TITLE;
                    html = string.Format("<{0}>{1}</{0}>", EL.TAG_TITLE, text.generalHtmlWords());
                }
                else
                {
                    id = _id;
                    string si = _s.ToLower().Trim();
                    if (si.IndexOf("http") == 0)
                    {
                        type = SENTENCE.LINK;
                        html = string.Format("<{0}>{1}</{0}>", EL.TAG_LINK, text);
                    }
                    else if (si.IndexOf("note") == 0)
                    {
                        type = SENTENCE.NOTE;
                        html = string.Format("<{0}>{1}</{0}>", EL.TAG_NOTE, text.generalHtmlWords());
                    }
                    else
                    {
                        type = SENTENCE.PARAGRAPH;
                        html = string.Format("<{0}>{1}</{0}>", EL.TAG_PARAGRAPH, text.generalHtmlWords());
                    }
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}: {2}", id, type.ToString(), text);
        }
    }

    public enum SENTENCE
    {
        TITLE,
        PARAGRAPH,
        HEADING,
        NOTE,
        CODE,
        UL_LI,
        LINK,
    }

    public static class EnglishExt
    {
        public static string generalHtmlWords(this string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            char _charEnd = text[text.Length - 1];
            string html = string.Empty, _clause = string.Empty, _word = string.Empty;
            string[] aSen = text.Split(EL._SPLIT_PARAGRAPH_TO_SENTENCE, StringSplitOptions.None).Where(x => x.Trim() != string.Empty).ToArray();
            foreach (string se in aSen)
            {
                string[] arrParts = se.Split(EL._SPLIT_PARAGRAPH_TO_CLAUSE, StringSplitOptions.None).Where(x => x.Trim() != string.Empty).ToArray();
                _clause = se;
                foreach(string _part in arrParts)
                {
                    _word = "<i>" + string.Join("</i> <i>", _part.Split(' ')) + "</i>";
                    _clause = _clause.Replace(_part, string.Format("<{0}>{1}</{0}>", EL.TAG_CLAUSE, _word));
                }

                html += string.Format("<{0}>{1}</{0}>", EL.TAG_SENTENCE, _clause);
                if (_charEnd != ':' && _charEnd != '?')
                    html += ".";
            }

            return html.Replace("<i></i>", string.Empty);
        }

        //public static string generalHtmlParagraph(this string text)
        //{
        //    if (string.IsNullOrEmpty(text)) return string.Empty;

        //    char _charEnd = text[text.Length - 1];
        //    string[] aSen = text.Split(EL._SPLIT_PARAGRAPH_TO_SENTENCE, StringSplitOptions.None).Where(x => x.Trim() != string.Empty).ToArray();
        //    string _sen = string.Empty, hi = string.Empty;
        //    foreach (string se in aSen)
        //    {
        //        string[] ap = se.Split(EL._SPLIT_PARAGRAPH_TO_CLAUSE, StringSplitOptions.None).Where(x => x.Trim() != string.Empty).ToArray();
        //        hi = se;
        //        for (int ki = 0; ki < ap.Length; ki++)
        //            hi = hi.Replace(ap[ki], string.Format("<{0}>{1}</{0}>", EL.TAG_WORD, ap[ki]));

        //        _sen += string.Format("<{0}>{1}</{0}>", EL.TAG_SENTENCE , hi);
        //        if (_charEnd != ':' && _charEnd != '?')
        //            _sen += ".";
        //    }

        //    //string html = string.Format("<{0}>{1}</{0}>", EL.TAG_PARAGRAPH, _sen);
        //    return _sen;
        //}

        //public static string generalHTMLAttr(this string text, SENTENCE type, string func = null, string attrClass = null)
        //{
        //    Dictionary<string, string> dic = null;
        //    if (!string.IsNullOrEmpty(attrClass))
        //    {
        //        dic = new Dictionary<string, string>();
        //        dic.Add("class", attrClass);
        //    }
        //    return generalHTML(text, type, func, dic);
        //}

        //public static string generalHTML(this string text, SENTENCE type, string func = null, Dictionary<string, string> attributes = null)
        //{
        //    string attr = string.Empty, f = string.Empty, tag = string.Empty;
        //    switch (type)
        //    {
        //        case SENTENCE.TITLE:
        //            tag = EL.TAG_TITLE;
        //            break;
        //        case SENTENCE.LINK:
        //            tag = EL.TAG_LINK;
        //            break;
        //        case SENTENCE.HEADING:
        //            tag = EL.TAG_HEADING;
        //            text = text.generalHtmlWords();
        //            break;
        //        case SENTENCE.NOTE:
        //            tag = EL.TAG_NOTE;
        //            break;
        //        case SENTENCE.CODE:
        //            tag = EL.TAG_CODE;
        //            break;
        //        case SENTENCE.PARAGRAPH:
        //            tag = EL.TAG_PARAGRAPH;
        //            break;
        //    }

        //    if (attributes != null && attributes.Count > 0)
        //        attr = " " + string.Join(" ", attributes.Select(kv => string.Format(@"""{0}""=""{1}""", kv.Key, kv.Value)).ToArray()) + " ";
        //    if (!string.IsNullOrEmpty(func))
        //        f = string.Format(@" do={0} ", func);
        //    return string.Format("<{0}{1}{2}>{3}</{0}>", tag, f, attr, text);
        //}
    }
}
