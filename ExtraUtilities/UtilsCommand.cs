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

        public IEnumerable<OptionDescription> HelpOptions { get; } = new OptionDescription[]
        {
            new("option","getlogchat"),
            new("/utils getlogchat", "Attempts to obtain an invite link to the log chat"),
            new("/utils apisat", "Gets or Sets the API Saturation limit for the Message Queue. Lower values help prevent API Requests exception, but result in fewer messages sent at a time")
        };

        public string Trigger => "/utils";

        public string Alias => null;

        public TelegramBotCommandClient Processor { get; set; }

        public async Task<CommandResponse> Action(BotCommandArguments args)
        {
            var m = args.Message;

            if (args.Arguments.Length < 2)
                return new(m, false, "Not enough arguments");
            if (args.Arguments[1] == "getlogchat")
            {
                try
                {
                    var chat = await OutBot.EnqueueFunc(b => b.GetChatAsync(Settings<WatcherSettings>.Current.LogChatId));
                    return new(m, false, chat.InviteLink);
                }
                catch(Exception e)
                {
                    return new(m, false, e.Message);
                }
            }

            if(args.Arguments[1] == "apisat")
            {
                if(args.Arguments.Length > 2)
                {
                    if (int.TryParse(args.Arguments[2], out var result) && result > 0)
                    {
                        Processor.MessageQueue.ApiSaturationLimit = result;
                        return new(m, false, $"API Saturation Limit set to {result}");
                    }
                    return new(m, false, $"Invalid value, please write a decimal number between 0 and {int.MaxValue}");
                }
                return new(m, false, $"The API Saturation Limit is currently set to {Processor.MessageQueue.ApiSaturationLimit}");
            }
            return new(m, false, "Unknown option");
        }

        public Task<CommandResponse> ActionReply(BotCommandArguments args)
        {
            throw new NotImplementedException();
        }

        public void Cancel(User user)
        {
            throw new NotImplementedException();
        }
    }
}
