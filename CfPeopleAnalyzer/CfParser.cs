using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CfPeopleAnalyzer;

public abstract class CfParser : IParser
{
    protected const int StartPage = 1;
    protected const int PageNumber = 2992;
    protected const string BaseUrl = "https://codeforces.com";
    protected const string RatingsPage = BaseUrl + "/ratings/all/true/page/";
    protected const string ProfilePage = BaseUrl + "/profile/";
    private static readonly char[] RestrictedSymbols = {'&'};
    private readonly HtmlWeb _htmlWeb = new();

    public abstract void ParseWebSiteAsync();

    protected static bool CanTextBeHandleInRatings(HtmlNode item)
    {
        var canBeHandle = true;
        foreach (var restrictedSymbol in RestrictedSymbols)
        {
            if (item.InnerText.Contains(restrictedSymbol))
            {
                canBeHandle = false;
            }
        }

        return canBeHandle;
    }

    protected async Task<HtmlDocument> LoadDocumentAsync(string url)
    {
        await Console.Out.WriteLineAsync("url loading: " + url);
        return await _htmlWeb.LoadFromWebAsync(url);
    }

    protected HtmlDocument LoadDocument(string url)
    {
        Console.Out.WriteLineAsync("url loading: " + url);
        return _htmlWeb.Load(url);
    }

    protected static IEnumerable<HtmlNode>? SelectHandleNodes(HtmlDocument document)
    {
        return document.DocumentNode.SelectNodes("//td[2]/a");
    }
}