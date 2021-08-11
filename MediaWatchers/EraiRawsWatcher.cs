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
using Telegram.Bot.Types.Enums;

#nullable enable
namespace DiegoG.WebWatcher
{
    [Watcher]
    public class EraiRawsWatcher : IWebWatcher
    {
        private const string RSSFeedLink = "https://beta.erai-raws.info/feed/?res=720p&type=torrent&subs[]=us";
        
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

            if(Settings<EraiRawsWatcherSettings>.Current.ChatId is 0)
            {
                Log.Warning("Erai Raws ChatId is 0, skipping Check procedure");
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

                        var cancel = new CancellationTokenSource();
                        foreach (var pattern in Settings<EraiRawsWatcherSettings>.Current.MatchPatterns)
                            if (await Task.Run(() => Regex.IsMatch(article.Title, pattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)), cancel.Token).AwaitWithTimeout(
                                1000,
                                ifError: () =>
                                {
                                    Log.Error($"Regex pattern {pattern} timed out with title {article.Title}");
                                    cancel.Cancel();
                                }
                                ))
                            {
                                Log.Information($"Found {article.Title} to be interesting");
                                lock (relevantArticles)
                                    relevantArticles.Add(article);
                                goto Matched;
                            }
                        Log.Debug($"Ignoring {article.Title}");

                    Matched:;

                        await tasks;

                        var id = Settings<EraiRawsWatcherSettings>.Current.ChatId;
                        foreach (var art in relevantArticles)
                            OutBot.EnqueueAction(b => b.SendTextMessageAsync(id, $"<strong>{art.Title}</strong> @ {art.Published:g}\n-&gt; <a href=\"{art.Link}\">Link</a>", ParseMode.Html));
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
            Log.Information("Finished processing RSS feed data");
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