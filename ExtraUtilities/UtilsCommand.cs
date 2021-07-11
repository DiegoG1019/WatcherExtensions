using DiegoG.TelegramBot;
using DiegoG.TelegramBot.Types;
using DiegoG.Utilities.Settings;
using DiegoG.WebWatcher;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ExtraUtilities
{
    [BotCommand]
    public class UtilsCommand : IBotCommand
    {
        public string HelpExplanation => "Provides access to misc utilities";

        public string HelpUsage => "/utils [option] (...)";

        public IEnumerable<(string Option, string Explanation)> HelpOptions { get; } = new[]
        {
            ("option","getlogchat"),
            ("/utils getlogchat", "Attempts to obtain an invite link to the log chat")
        };

        public string Trigger => "/utils";

        public string Alias => null;

        public BotCommandProcessor Processor { get; set; }

        public async Task<(string Result, bool Hold)> Action(BotCommandArguments args)
        {
            if (args.Arguments.Length < 2)
                return ("Not enough arguments", false);
            if (args.Arguments[1] == "getlogchat")
            {
                try
                {
                    var chat = await OutBot.EnqueueFunc(b => b.GetChatAsync(Settings<WatcherSettings>.Current.LogChatId));
                    return (chat.InviteLink, false);
                }
                catch(Exception e)
                {
                    return (e.Message, false);
                }
            }

            return ("Unknown option", false);
        }

        public Task<(string Result, bool Hold)> ActionReply(BotCommandArguments args)
        {
            throw new NotImplementedException();
        }

        public void Cancel(User user)
        {
            throw new NotImplementedException();
        }
    }
}
