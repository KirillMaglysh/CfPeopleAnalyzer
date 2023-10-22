using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CfPeopleAnalyzer.db;
using CfPeopleAnalyzer.db.models;
using HtmlAgilityPack;

namespace CfPeopleAnalyzer;

public class CfParser : IParser
{
    private const int StartPage = 1;
    private const int FinishPage = 2992;
    private const string BaseUrl = "https://codeforces.com";
    private const string RatingsPage = BaseUrl + "/ratings/all/true/page/";
    private const string ProfilePage = BaseUrl + "/profile/";
    private const int TimeoutForRequestPackage = 1000;
    private const int RequestPackageSize = 9;

    private static readonly char[] RestrictedSymbols = {'&'};
    private static readonly string[] RestrictedSubstrings = {"Round", "Register now"};
    private readonly HtmlWeb _htmlWeb = new();
    private readonly Queue<string> _failedHandles = new();
    private readonly List<User> _parsedUsers = new();

    private int _currentRequestPackageSize;
    private int _addedUserIdx;


    public void ParseWebSiteAsync()
    {
        Console.Out.WriteLine(".............. CF-parsing started ..............");
        var startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        for (var pageId = StartPage; pageId <= FinishPage; pageId++)
        {
            ParseRatingsPage(pageId);
            Thread.Sleep(200_000);
        }

        var finishTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        Console.Out.WriteLine("=============== CF-parsing ===============" + "\n" + "=============== Time: " +
                              (finishTime - startTime) + " ===============");
    }

    private async void ParseRatingsPage(int pageId)
    {
        await Console.Out.WriteLineAsync("............. Rating Page#" + pageId + " parsing .............");
        var startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        var document = await LoadDocumentAsync(RatingsPage + pageId);
        var nodes = SelectHandleNodes(document);
        if (nodes != null)
        {
            ParseRatingPageNodes(nodes);
        }
        else
        {
            await Console.Error.WriteLineAsync("NO HANDLES FOUND on page #" + pageId);
        }

        CheckFailedHandles();

        var finishTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        await Console.Out.WriteLineAsync("=============== Rating Page#" + pageId + " parsing ===============" + "\n" +
                                         "=============== Time: " + (finishTime - startTime) + " ===============");
        Thread.Sleep(10_000);
        checkParsed();
    }

    private void checkParsed()
    {
        for (; _addedUserIdx < _parsedUsers.Count; _addedUserIdx++)
        {
            if (!DbManagerFactory.DbManager.GetUserDao().Exist(_parsedUsers[_addedUserIdx].Handle))
            {
                DbManagerFactory.DbManager.GetUserDao().AddUser(_parsedUsers[_addedUserIdx]);
            }
        }
    }

    private void CheckFailedHandles()
    {
        while (_failedHandles.Count > 0)
        {
            ParseProfilePage(_failedHandles.Dequeue(), false);
        }
    }

    private void ParseRatingPageNodes(IEnumerable<HtmlNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (!CanTextBeHandleInRatings(node))
            {
                continue;
            }

            CheckPackageSizeLimit();

            AskApiForProfile(node.InnerText);
            // ParseProfilePage(node.InnerText, true);
        }
    }

    private void AskApiForProfile(string nodeInnerText)
    {
        throw new NotImplementedException();
    }

    private void CheckPackageSizeLimit()
    {
        if (_currentRequestPackageSize++ < RequestPackageSize)
        {
            return;
        }

        Thread.Sleep(TimeoutForRequestPackage);
        _currentRequestPackageSize = 0;
    }

    private async void ParseProfilePage(string handle, bool isFirstChance)
    {
        await Console.Out.WriteLineAsync("STARTED PROCESSING : " + handle);

        var document = await LoadDocumentAsync(ProfilePage + handle);

        var countryNodes = SelectCountryNodes(document);
        if (!CheckCountryNodes(handle, isFirstChance, countryNodes))
        {
            return;
        }

        var country = FindCountryInNodes(handle, countryNodes!);
        if (country.Length == 0)
        {
            await Console.Error.WriteLineAsync("FAILED: " + handle + " : no suitable node");
            return;
        }

        var ratingNodes = SelectRatingNodes(document)!.ToArray();
        var rating = Convert.ToInt32(ratingNodes[0].InnerText);
        await Console.Out.WriteLineAsync("SUCCESS: " + handle + "=" + country + "---" + rating);
        SaveParsedUser(handle, country, rating);
    }

    private void SaveParsedUser(string handle, string country, int rating)
    {
        _parsedUsers.Add(new User(handle, country, rating));
    }

    private bool CheckCountryNodes(string handle, bool isFirstChance, IEnumerable? countryNodes)
    {
        if (countryNodes != null)
        {
            return true;
        }

        Console.Error.WriteLine("FAILED: " + handle + " : htmlNode is EMPTY");
        if (isFirstChance)
        {
            _failedHandles.Enqueue(handle);
        }

        return false;
    }

    private static string FindCountryInNodes(string handle, HtmlNodeCollection htmlNodeCollection)
    {
        var country = "";
        foreach (var node in htmlNodeCollection.Reverse())
        {
            if (!CanTextBeCountryInProfile(node))
            {
                continue;
            }

            Console.Out.WriteLine("Country Found: " + handle + " : " + node.InnerText);
            country = node.InnerText;
            break;
        }

        return country;
    }

    private static HtmlNodeCollection? SelectCountryNodes(HtmlDocument document)
    {
        return document.DocumentNode.SelectNodes("//div[2]/div[1]/a");
    }

    private static bool CanTextBeCountryInProfile(HtmlNode node)
    {
        if (node.InnerText == null)
        {
            return false;
        }

        var text = node.InnerText;
        if (text.Length == 0)
        {
            return false;
        }

        foreach (var substring in RestrictedSubstrings)
        {
            if (text.Contains(substring))
            {
                return false;
            }
        }

        return true;
    }

    private static IEnumerable<HtmlNode>? SelectHandleNodes(HtmlDocument document)
    {
        return document.DocumentNode.SelectNodes("//td[2]/a");
    }

    private static IEnumerable<HtmlNode>? SelectRatingNodes(HtmlDocument document)
    {
        return document.DocumentNode.SelectNodes("//div[contains(@class,'info')]/ul[1]/li[1]/span[1]");
    }

    private static bool CanTextBeHandleInRatings(HtmlNode item)
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

    private async Task<HtmlDocument> LoadDocumentAsync(string url)
    {
        await Console.Out.WriteLineAsync("url loading: " + url);
        return await _htmlWeb.LoadFromWebAsync(url);
    }
}