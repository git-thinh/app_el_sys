#region

using HtmlAgilityPack;
using System;
using System.Collections;
using System.IO;

#endregion

namespace app_el_sys
{

    /// <summary>
    /// Represents a document that needs linked files to be rendered, such as images or css files, and points to other HTML documents.
    /// </summary>
    public class DocumentWithLinks
    {
        private ArrayList _links;
        private ArrayList _references;
        private HtmlDocument _doc;

        /// <summary>
        /// Creates an instance of a DocumentWithLinkedFiles.
        /// </summary>
        /// <param name="doc">The input HTML document. May not be null.</param>
        public DocumentWithLinks(HtmlDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }
            _doc = doc;
            GetLinks();
            GetReferences();
        }

        private void GetLinks()
        {
            _links = new ArrayList();
            HtmlNodeCollection atts = _doc.DocumentNode.SelectNodes("//*[@background or @lowsrc or @src or @href]");
            if (atts == null)
                return;

            foreach (HtmlNode n in atts)
            {
                ParseLink(n, "background");
                ParseLink(n, "href");
                ParseLink(n, "src");
                ParseLink(n, "lowsrc");
            }
        }

        private void GetReferences()
        {
            _references = new ArrayList();
            HtmlNodeCollection hrefs = _doc.DocumentNode.SelectNodes("//a[@href]");
            if (hrefs == null)
                return;

            foreach (HtmlNode href in hrefs)
            {
                _references.Add(href.Attributes["href"].Value);
            }
        }


        private void ParseLink(HtmlNode node, string name)
        {
            HtmlAttribute att = node.Attributes[name];
            if (att == null)
                return;

            // if name = href, we are only interested by <link> tags
            if ((name == "href") && (node.Name != "link"))
                return;

            _links.Add(att.Value);
        }

        /// <summary>
        /// Gets a list of links as they are declared in the HTML document.
        /// </summary>
        public ArrayList Links
        {
            get
            {
                return _links;
            }
        }

        /// <summary>
        /// Gets a list of reference links to other HTML documents, as they are declared in the HTML document.
        /// </summary>
        public ArrayList References
        {
            get
            {
                return _references;
            }
        }
    }
    public class HtmlToText
    {
        #region Public Methods

        public string Convert(string path)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(path);

            StringWriter sw = new StringWriter();
            ConvertTo(doc.DocumentNode, sw);
            sw.Flush();
            return sw.ToString();
        }

        public string ConvertHtml(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            StringWriter sw = new StringWriter();
            ConvertTo(doc.DocumentNode, sw);
            sw.Flush();
            return sw.ToString();
        }

        public void ConvertTo(HtmlNode node, TextWriter outText)
        {
            string html;
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    // don't output comments
                    break;

                case HtmlNodeType.Document:
                    ConvertContentTo(node, outText);
                    break;

                case HtmlNodeType.Text:
                    // script and style must not be output
                    string parentName = node.ParentNode.Name;
                    if ((parentName == "script") || (parentName == "style"))
                        break;

                    // get text
                    html = ((HtmlTextNode) node).Text;

                    // is it in fact a special closing node output as text?
                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    // check the text is meaningful and not a bunch of whitespaces
                    if (html.Trim().Length > 0)
                    {
                        outText.Write(HtmlEntity.DeEntitize(html));
                    }
                    break;

                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
                        case "p":
                            // treat paragraphs as crlf
                            outText.Write("\r\n");
                            break;
                    }

                    if (node.HasChildNodes)
                    {
                        ConvertContentTo(node, outText);
                    }
                    break;
            }
        }

        #endregion

        #region Private Methods

        private void ConvertContentTo(HtmlNode node, TextWriter outText)
        {
            foreach (HtmlNode subnode in node.ChildNodes)
            {
                ConvertTo(subnode, outText);
            }
        }

        #endregion
    }
}