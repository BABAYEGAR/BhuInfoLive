using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;
using HtmlAgilityPack;

namespace BhuInfo.Data.Service.TextFormatter
{
    public class RemoveCharacters
    {
        /// <summary>
        ///     This method removes specified special characters from a text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string RemoveSpecialCharacters(string text)
        {
            var pattern = @"<(.|\n)*?>";
            return Regex.Replace(text, pattern, string.Empty);
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
                    html = ((HtmlTextNode)node).Text;

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
        private void ConvertContentTo(HtmlNode node, TextWriter outText)
        {
            foreach (HtmlNode subnode in node.ChildNodes)
            {
                ConvertTo(subnode, outText);
            }
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

        /// <summary>
        ///     This method removes enodings froma string
        /// </summary>
        /// <param name="inputValue"></param>
        /// <returns></returns>
        public static string StripUnicodeCharactersFromString(string inputValue)
        {
            return Regex.Replace(inputValue, @"[^\u0000-\u007F]", string.Empty);
        }
        public string ConvertToText(string html)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlDocument xsl = new XmlDocument();
            xmlDoc.LoadXml(html.ToString());
            xsl.CreateEntityReference("nbsp");
            xsl.Load(System.Web.HttpContext.Current.Server.MapPath("~/Markdown.xslt"));

            //creating xslt
            XslTransform xslt = new XslTransform();
            xslt.Load(xsl, null, null);

            //creating stringwriter
            StringWriter writer = new System.IO.StringWriter();

            //Transform the xml.
            xslt.Transform(xmlDoc, null, writer, null);

            //return string
            var text = writer.ToString();
            writer.Close();

            return text;
        }
        public  string RemoveHtmlTags(string htmlText, bool preserveNewLine)
        {
            System.Web.UI.HtmlControls.HtmlGenericControl divNew = new System.Web.UI.HtmlControls.HtmlGenericControl("div");
            divNew.InnerHtml = htmlText;
            if (preserveNewLine)
            {
                divNew.InnerHtml = divNew.InnerHtml.Replace("<br>", "\n");
                divNew.InnerHtml = divNew.InnerHtml.Replace("<br/>", "\n");
                divNew.InnerHtml = divNew.InnerHtml.Replace("<br />", "\n");
                divNew.InnerHtml = divNew.InnerHtml.Replace("<p>", Environment.NewLine);
                divNew.InnerHtml = divNew.InnerHtml.Replace("</p>", Environment.NewLine);
            }
            return Regex.Replace(divNew.InnerText, "<[^>]*>", "");
        }

    }
}