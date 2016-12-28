using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;

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
    }
}