using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using CfPeopleAnalyzer.db;
using CfPeopleAnalyzer.db.models;
using HtmlAgilityPack;

namespace CfPeopleAnalyzer;

public class CfParserApi : CfParser
{
    private const string UserApiUrl = BaseUrl + "/api/user.info?handles=";
    private const int BadRequestDelay = 360_000;
    
    private const int PagePackageSize = 3;
    private readonly WebClient _webClient = new();


    public override void ParseWebSiteAsync()
    {
        var handlesPackage = new List<string>();
        for (var pageId = 2560; pageId <= PageNumber; pageId++)
        {
            CollectHandlesFromRatingPage(handlesPackage, pageId, true);
            if (pageId % PagePackageSize == 0)
            {
                ProcessPackage(handlesPackage);
                handlesPackage.Clear();
                Thread.Sleep(10_000);
            }
        }
    }

    private void ProcessPackage(List<string> handlesPackage)
    {
        try
        {
            var json = _webClient.DownloadString(BuildApiRequest(handlesPackage));

            var mappedUsers = JsonNode.Parse(json)?["result"]?.AsArray();
            if (mappedUsers == null)
            {
                return;
            }

            ParseJsonUsers(mappedUsers);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static void ParseJsonUsers(JsonArray mappedUsers)
    {
        foreach (var userJson in mappedUsers)
        {
            if (userJson == null)
            {
                continue;
            }

            ParseJsonUser(userJson);
        }
    }

    private static void ParseJsonUser(JsonNode userJson)
    {
        if (userJson["handle"] == null)
        {
            return;
        }


        var handle = userJson["handle"]!.ToString();
        var country = "Undefined";
        if (userJson["country"] != null)
        {
            country = userJson["country"]!.ToString();
        }

        var rating = int.Parse(userJson["rating"]!.ToString());
        if (!DbManagerFactory.DbManager.GetUserDao().Exist(handle))
        {
            DbManagerFactory.DbManager.GetUserDao().AddUser(new User(handle, country, rating));
        }
    }

    private static string BuildApiRequest(List<string> handlesPackage)
    {
        var apiRequest = new StringBuilder(UserApiUrl);
        foreach (var handle in handlesPackage)
        {
            apiRequest.Append(handle).Append(';');
        }

        return apiRequest.ToString();
    }

    private void CollectHandlesFromRatingPage(List<string> handlesPackage, int pageId, bool firstAttempt)
    {
        var document = LoadDocument(RatingsPage + pageId);
        var nodes = SelectHandleNodes(document);
        if (nodes != null)
        {
            ParseRatingPageNodes(handlesPackage, nodes);
        }
        else
        {
            ProcessRatingsPageNotFound(handlesPackage, pageId, firstAttempt);
        }
    }

    private void ProcessRatingsPageNotFound(List<string> handlesPackage, int pageId, bool firstAttempt)
    {
        Console.Error.WriteLineAsync("NO HANDLES FOUND on page #" + pageId);
        Thread.Sleep(BadRequestDelay);
        if (firstAttempt)
        {
            CollectHandlesFromRatingPage(handlesPackage, pageId, false);
        }
    }

    private static void ParseRatingPageNodes(List<string> handlesPackage, IEnumerable<HtmlNode> nodes)
    {
        handlesPackage.AddRange(from node in nodes where CanTextBeHandleInRatings(node) select node.InnerText);
    }
}