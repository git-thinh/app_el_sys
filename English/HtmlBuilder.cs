using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace app_el_sys
{

    public class HtmlBuilder
    {


        public static void fromText(string text_plain)
        {
            string[] a = File.ReadAllLines("demo.txt").Where(x => x.Trim() != "").ToArray();
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
            File.WriteAllText("demo-output.txt", htm);
        }
    }
}
