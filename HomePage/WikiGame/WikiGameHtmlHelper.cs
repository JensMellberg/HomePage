using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace HomePage.WikiGame
{

    public static class WikiGameHtmlHelper
    {
        public static (string convertedHtml, List<string> allowedLinks) ConvertHtml(string html, bool alwaysStripLinks)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            RemoveNodes(doc, "//script|//style");

            RemoveNodes(doc, "//span[contains(@class,'mw-editsection')]");
            RemoveNodes(doc, "//base");
            FixTables(doc);
            FixSections(doc);

            var allowedLinks = new List<string>();

            var links = doc.DocumentNode.SelectNodes("//a[@href]");
            if (links != null)
            {
                foreach (var link in links)
                {
                    var href = link.GetAttributeValue("href", "");

                    if (!alwaysStripLinks && IsValidWikiLink(href))
                    {
                        var pageName = ExtractPageName(href);

                        if (!string.IsNullOrEmpty(pageName))
                        {
                            allowedLinks.Add(pageName);
                            link.SetAttributeValue("data-page", pageName);
                            link.Attributes.Remove("href");
                            link.AddClass("wiki-link");
                        }
                    }
                    else
                    {
                        RemoveLinkButKeepText(link);
                    }
                }
            }

            return (doc.DocumentNode.InnerHtml, allowedLinks);
        }

        private static void RemoveNodes(HtmlDocument doc, string xpath)
        {
            var nodes = doc.DocumentNode.SelectNodes(xpath);
            if (nodes == null)
            {
                return;
            }

            foreach (var node in nodes)
            {
                node.Remove();
            }
        }

        private static void FixTables(HtmlDocument doc)
        {
            var xpath = "//div[contains(@class,'pcs-collapse-table-container')]";
            var tableContainers = doc.DocumentNode.SelectNodes(xpath);
            if (tableContainers == null)
            {
                return;
            }

            var nodesToRemove = new List<HtmlNode>();
            foreach (var tableContainer in tableContainers)
            {
                foreach (var child in tableContainer.ChildNodes)
                {
                    if (child.HasClass("pcs-collapse-table-content"))
                    {
                        child.SetAttributeValue("style", "display: block;");
                    } 
                    else
                    {
                        nodesToRemove.Add(child);
                    }
                }
            }

            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
        }

        private static void FixSections(HtmlDocument doc)
        {
            var xpath = "//section";
            var sections = doc.DocumentNode.SelectNodes(xpath);
            if (sections == null)
            {
                return;
            }

            foreach (var section in sections)
            {
                section.SetAttributeValue("style", "display: block;");
            }
        }

        private static bool IsValidWikiLink(string href)
        {
            href = href.Trim();
            if (string.IsNullOrEmpty(href))
            {
                return false;
            }

            if (href.Contains(':'))
            {
                return false;
            }

            if (href.Contains('#'))
            {
                return false;
            }

            if (href.StartsWith("./"))
            {
                return true;
            }

            if (href.StartsWith("/wiki/"))
            {
                return true;
            }

            return false;
        }

        private static string ExtractPageName(string href)
        {
             var page = href
                .Replace("/wiki/", "")
                .Replace("./", "")
                .Trim();

            page = Uri.UnescapeDataString(page);
            page = WebUtility.HtmlDecode(page);
            page = Regex.Unescape(page);

            return page;
        }

        private static void RemoveLinkButKeepText(HtmlNode link)
        {
            var parent = link.ParentNode;
            if (parent == null)
            {
                return;
            }

            var textNode = HtmlTextNode.CreateNode(link.InnerText);
            parent.ReplaceChild(textNode, link);
        }
    }
}
