using System;
namespace dscapi.sdk
{
    public class DeHtml
    {
        HtmlAgilityPack.HtmlDocument xd = null;
        public DeHtml()
        {
            xd = new HtmlAgilityPack.HtmlDocument();
        }

        public HtmlAgilityPack.HtmlDocument htmlformat(string htmlstr)
        {
            htmlstr = htmlstr.Replace("\r\n", "");
            htmlstr = htmlstr.Replace("\r", "");
            htmlstr = htmlstr.Replace("\n", "");
            xd.LoadHtml(htmlstr);
            return xd;
        }
    }
}
