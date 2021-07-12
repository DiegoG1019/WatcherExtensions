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
            ("/utils getlogchat", "Attempts to obtain an invite link to the log chat"),
            ("/utils apisat", "Gets or Sets the API Saturation limit for the Message Queue. Lower values help prevent API Requests exception, but result in fewer messages sent at a time")
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

            if(args.Arguments[1] == "apisat")
            {
                if(args.Arguments.Length > 2)
                {
                    if (int.TryParse(args.Arguments[2], out var result) && result > 0)
                    {
                        Processor.MessageQueue.ApiSaturationLimit = result;
                        return ($"API Saturation Limit set to {result}", false);
                    }
                    return ($"Invalid value, please write a decimal number between 0 and {int.MaxValue}", false);
                }
                return ($"The API Saturation Limit is currently set to {Processor.MessageQueue.ApiSaturationLimit}", false);
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
