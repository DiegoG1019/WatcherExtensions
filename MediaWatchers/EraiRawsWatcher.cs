using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DiegoG.TelegramBot;
using DiegoG.Utilities;
using DiegoG.Utilities.IO;
using DiegoG.Utilities.Settings;
using HtmlAgilityPack;
using RssFeedParser;
using RssFeedParser.Models;
using Serilog;

#nullable enable
namespace DiegoG.WebWatcher
{
    [Watcher]
    public class EraiRawsWatcher : IWebWatcher
    {
        private const string RSSFeedLink = "https://beta.erai-raws.info/feed/?res=720p&type=magnet&subs[]=us";
        
        private const long EraiRawsChatID = -1001267384658;

        public TimeSpan Interval { get; } = TimeSpan.FromHours(.5);

        public string Name => "EraiRawsWatcher";

        public RssFeedArticle? LastPost { get; private set; }
        
        private FeedParser RssFeedParser;
        public Task<RssFeed> ParseFeed()
            => RssFeedParser.ParseFeed(RSSFeedLink);


        public async Task Check()
        {
            if(Settings<EraiRawsWatcherSettings>.Current.MatchPatterns.Count is 0)
            {
                Log.Warning("Erai Raws patterns for matching series is empty, skipping Check procedure");
                return;
            }

            int i = 0;
            Log.Information("Parsing Erai Raws Feed");
            
            var articles = (await ParseFeed()).Articles;

            var lastpost = articles.First();

            AsyncTaskManager tasks = new();

            foreach (var article in articles) 
                try
                {
                    Log.Information($"Reviewing article {++i}: {article.Title}");

                    Log.Debug($"Reviewing data in article {i}");

                    if (LastPost is not null && article.Published == LastPost.Published && article.Title == LastPost.Title)
                    {
                        Log.Information($"Nothing new after article {i}: {article.Title}");
                        break;
                    }

                    try
                    {
                        List<RssFeedArticle> relevantArticles = new();

                        foreach (var pattern in Settings<EraiRawsWatcherSettings>.Current.MatchPatterns)
                            tasks.Run(async () => 
                            {
                                var pat = pattern;
                                var cancel = new CancellationTokenSource();
                                await Task.Run(() => Regex.IsMatch(article.Title, pat, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)), cancel.Token).AwaitWithTimeout(
                                    10000,
                                    () =>
                                    {
                                        lock (relevantArticles)
                                            relevantArticles.Add(article);
                                    },
                                    () =>
                                    {
                                        Log.Error($"Regex pattern {pat} timed out with title {article.Title}");
                                        cancel.Cancel();
                                    }
                                    );
                            });

                        await tasks;

                        foreach (var art in relevantArticles)
                            OutBot.EnqueueAction(b => b.SendTextMessageAsync(-1001526952787, $"**{art.Title}** @ {art.Published:g}\n-> [Magnet]({art.Link})"));

                        Log.Information("Finished processing RSS feed data");
                    }
                    catch
                    {
                        Log.Error("One of the regex patterns timed out. Please verify the patterns");
                        throw;
                    }
                    finally
                    {
                        tasks.Clear();
                    }

                    await Task.Delay(50);
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to check latest upload of Erai-raws: {e.GetType().Name}::{e.Message}");
                    break;
                }

            Log.Debug("Updating LastPost");
            LastPost = lastpost;
            await Serialization.Serialize.JsonAsync(LastPost, LastPostDir, LastPostFile);
        }

        public async Task FirstCheck()
        {
            RssFeedParser = new FeedParser();

            Directory.CreateDirectory(LastPostDir);
            if (File.Exists(Path.Combine(LastPostDir, LastPostFile)))
                LastPost = await Serialization.Deserialize<RssFeedArticle>.JsonAsync(LastPostDir, LastPostFile);

            await Check();
        }

        readonly static string LastPostDir = Directories.InData("EraiRaws");
        readonly static string LastPostFile = "lastpost";
        public EraiRawsWatcher()
        {
            Settings<EraiRawsWatcherSettings>.Initialize(Directories.Configuration, "erw_config.cfg");
        }
    }
}